using System.ComponentModel.DataAnnotations;
namespace TestApi.Models.DTOs;

public class UserUpdateDto
{
    [RegularExpression("^[a-zA-Zа-яА-Я]+$", ErrorMessage = "the name contains invalid characters")]
    public string? Name { get; set; }
    [Range(0, 2, ErrorMessage = "the gender can only be 0, 1 or 2")]
    public int? Gender { get; set; }
    public DateTime? BirthDay { get; set; }

}

public class UpdateLoginDto
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the login contains invalid characters")]
    public required string NewLogin { get; set; }
}

public class UpdatePasswordDto
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the password contains invalid characters")]
    public required string NewPassword { get; set; }
}