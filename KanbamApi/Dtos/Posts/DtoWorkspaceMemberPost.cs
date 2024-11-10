using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Dtos.Posts
{
    public class DtoWorkspaceMemberPost
    {
        [BsonRequired]
        [Required]
        public string WorkspaceId { get; set; } = string.Empty;

        [BsonRequired]
        [Required]
        public string Email { get; set; } = string.Empty;

        [BsonRequired]
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
