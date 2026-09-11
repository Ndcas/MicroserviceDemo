using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Dtos;

public record LoginRequest
{
    [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống")]
    [StringLength(255, ErrorMessage = "Tên đăng nhập không hợp lệ")]
    public string Username { get; init; }

    [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
    public string Password { get; init; }
}
