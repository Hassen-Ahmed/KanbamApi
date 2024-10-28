using System.ComponentModel.DataAnnotations;

namespace KanbamApi.Dtos;

public class DtoListPost
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string BoardId { get; set; }
}
