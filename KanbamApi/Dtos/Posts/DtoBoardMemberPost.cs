using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Dtos.Posts
{
    public class DtoBoardMemberPost
    {
        [Required]
        public string BoardId { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
