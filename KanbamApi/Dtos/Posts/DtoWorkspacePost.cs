using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Dtos.Posts
{
    public class DtoWorkspacePost
    {
        [BsonRequired]
        [BsonElement("Name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string? Description { get; set; }
    }
}
