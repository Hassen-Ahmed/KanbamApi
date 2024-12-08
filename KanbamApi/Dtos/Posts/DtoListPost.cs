using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Dtos;

public class DtoListPost
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string BoardId { get; set; } = string.Empty;
    public int IndexNumber { get; set; }
}
