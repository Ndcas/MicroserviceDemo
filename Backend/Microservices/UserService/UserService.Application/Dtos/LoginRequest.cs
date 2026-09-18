using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Dtos;

public record LoginRequest
{
    [Required]
    [StringLength(255)]
    public string Username { get; init; }

    [Required]
    [MinLength(8)]
    public string Password { get; init; }
}
