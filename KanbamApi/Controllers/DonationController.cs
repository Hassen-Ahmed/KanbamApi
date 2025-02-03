using KanbamApi.Data.Interfaces;
using KanbamApi.Models;
using KanbamApi.Models.AuthModels;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Services.Interfaces.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

[Authorize]
[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IKanbamDbContext _kanbamDbContext;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PaymentController> _logger;
    private readonly string _frontEndDomain;
    private readonly string _stripeWebhookSecret;

    public PaymentController(
        IKanbamDbContext kanbamDbContext,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        ILogger<PaymentController> logger
    )
    {
        _kanbamDbContext = kanbamDbContext;
        _emailService = emailService;
        _userManager = userManager;
        _logger = logger;
        _frontEndDomain =
            DotNetEnv.Env.GetString("FRONT_END_DOMAIN")
            ?? throw new InvalidOperationException("FRONT_END_DOMAIN is not configured.");
        _stripeWebhookSecret =
            DotNetEnv.Env.GetString("STRIPE_WEBHOOK_SECRET_KEY")
            ?? throw new InvalidOperationException("STRIPE_WEBHOOK_SECRET_KEY is not configured.");
    }

    [Authorize]
    [HttpPost("create-checkout-session")]
    public IActionResult CreateCheckoutSession([FromBody] PaymentRequest request)
    {
        if (request.Amount < 5 || request.Amount > 20)
            return BadRequest("Donation amount must be between £5 and £20");

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            Mode = "payment",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "gbp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Donation"
                        },
                        UnitAmount = (long)(request.Amount * 100),
                    },
                    Quantity = 1,
                },
            },
            SuccessUrl = $"{_frontEndDomain}/payment?status=success",
            CancelUrl = $"{_frontEndDomain}/payment?status=failed",
        };

        var service = new SessionService();
        Session session;

        try
        {
            session = service.Create(options);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating Stripe checkout session. {@ex}", ex);
            return StatusCode(500, "An error occurred while creating the checkout session.");
        }

        return Ok(new { url = session.Url });
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            json = await reader.ReadToEndAsync();
        }

        try
        {
            var signatureHeader = Request.Headers["Stripe-Signature"];

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                _stripeWebhookSecret
            );
            // Handle when the payment is successful
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;

                if (session is not null)
                {
                    Donation donation =
                        new()
                        {
                            DonorEmail = session.CustomerDetails.Email,
                            Amount = (decimal)(session.AmountTotal / 100.0)!,
                            Currency = session.Currency,
                            PaymentIntentId = session.PaymentIntentId
                        };
                    // Save the donation to the database
                    await _kanbamDbContext.DonationsCollection.InsertOneAsync(donation);
                    // Find the user by email to send a thank-you email if the user exists and the payment is successful
                    var user = await _userManager.FindByEmailAsync(session.CustomerDetails.Email);

                    if (user is not null)
                    {
                        EmailRequest emailToSend =
                            new(
                                session.CustomerDetails.Email,
                                "Thank-You Email! 💖",
                                "<p>Thanks for your support! 💖</p>"
                            );

                        await _emailService?.SendEmailAsync(emailToSend)!;
                        _logger.LogInformation(
                            "Email sent to Supporter: {email}",
                            session.CustomerDetails.Email
                        );
                    }
                    else
                    {
                        _logger.LogWarning(
                            "User not found for email: {email}",
                            session.CustomerDetails.Email
                        );
                    }
                }
            }
            // Handle when the payment failed
            else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogError(
                    "Payment failed for PaymentIntent: {paymentIntentId}",
                    paymentIntent?.Id
                );
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe Webhook Error");
            return BadRequest();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
}
