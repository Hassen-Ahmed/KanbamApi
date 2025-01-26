using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Models.AuthModels;

public class ResetPasswordDetail
{
    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(
        @"^[\w\.-]+@[a-zA-Z\d-]+(\.[a-zA-Z]{2,})+$",
        ErrorMessage = "Wrong email address!"
    )]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(
        @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[#$@!%&*?])[A-Za-z0-9#$@!%&*?]{8,20}$",
        ErrorMessage = "assword must be 8–20 characters long and include at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character."
    )]
    public string NewPassword { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
