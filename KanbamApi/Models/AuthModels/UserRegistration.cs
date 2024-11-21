using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Models.AuthModels;

public class UserRegistration
{
    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(
        @"^[\w\.-]+@[a-zA-Z\d-]+(\.[a-zA-Z]{2,})+$",
        ErrorMessage = "Wrong email address!"
    )]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(
        @"^(?=.*[0-9])(?=.*[a-zA-Z])(?=.*[#$@!%&*?])[A-Za-z0-9#$@!%&*?]{8,20}$",
        ErrorMessage = "Password should be 8-20 characters and include at least 1 letter, 1 number, and only 1 special character!"
    )]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "Password does not match!")]
    public string? PasswordConfirm { get; set; }
}
