namespace TestApi.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

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
    public bool Admin { get; set; } = false;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the password contains invalid characters")]
    public required string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the password contains invalid characters")]
    public string? ModifiedBy { get; set; }

    public DateTime? RevokedOn { get; set; }

    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the password contains invalid characters")]
    public string? RevokedBy { get; set; }
}