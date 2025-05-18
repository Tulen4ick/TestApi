using System.ComponentModel.DataAnnotations;
namespace TestApi.Models.DTOs;

public class UserCreateDto
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the login contains invalid characters")]
    public required string Login { get; set; }

    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the password contains invalid characters")]
    public required string Password { get; set; }

    [Required]
    [RegularExpression("^[a-zA-Zа-яА-Я]+$", ErrorMessage = "the name contains invalid characters")]
    public required string Name { get; set; }

    [Range(0, 2, ErrorMessage = "the gender can only be 0, 1 or 2")]
    public int Gender { get; set; } = 2;

    public DateTime? BirthDay { get; set; }
    public bool? Admin { get; set; }
}