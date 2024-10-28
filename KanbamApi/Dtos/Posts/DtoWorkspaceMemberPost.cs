using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Dtos.Posts
{
    public class DtoWorkspaceMemberPost
    {
        [BsonRequired]
        [Required]
        public string workspaceId { get; set; }

        [BsonRequired]
        [Required]
        public string Role { get; set; }
    }
}
