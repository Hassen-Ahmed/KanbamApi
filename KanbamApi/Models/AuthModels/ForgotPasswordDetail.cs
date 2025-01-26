using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Models.AuthModels
{
    public class ForgotPasswordDetail
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(
            @"^[\w\.-]+@[a-zA-Z\d-]+(\.[a-zA-Z]{2,})+$",
            ErrorMessage = "Wrong email address!"
        )]
        public string Email { get; set; } = string.Empty;
    }
}
