using System.ComponentModel.DataAnnotations;
namespace TestApi.Models.DTOs;

public class AuthDto
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the login contains invalid characters")]
    public required string Login { get; set; }

    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the password contains invalid characters")]
    public required string Password { get; set; }

}