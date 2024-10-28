using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Dtos.Posts
{
    public class DtoBoardMemberPost
    {
        [Required]
        public string BoardId { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
