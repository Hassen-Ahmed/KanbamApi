using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Dtos.Posts
{
    public class DtoBoardPost
    {
        [BsonRequired]
        [Required]
        public string Name { get; set; } = string.Empty;

        [BsonRequired]
        [Required]
        public string WorkspaceId { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
