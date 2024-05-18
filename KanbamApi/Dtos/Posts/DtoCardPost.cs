using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Dtos;

public class DtoCardPost
{
    [Required]
    public string? ListId { get; set; }

    [Required]
    public string? Title { get; set; }

    [Required]
    public int IndexNumber { get; set; }
}
