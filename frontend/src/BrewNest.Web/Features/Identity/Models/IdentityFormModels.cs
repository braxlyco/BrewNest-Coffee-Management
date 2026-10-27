using System.ComponentModel.DataAnnotations;

namespace BrewNest.Web.Features.Identity.Models;

internal sealed class LoginFormModel
{
    [Display(Name = "email address")]
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "password")]
    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

internal sealed class CreateUserFormModel : IValidatableObject
{
    [Display(Name = "email address")]
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "full name")]
    [Required, StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IdentityPasswordRules.IsValid(Password))
            yield return new ValidationResult(IdentityPasswordRules.Guidance, [nameof(Password)]);
    }
}

internal sealed class CreateRoleFormModel
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}

internal sealed class ChangePasswordFormModel : IValidatableObject
{
    [Display(Name = "current password")]
    [Required(ErrorMessage = "Enter your current password."), DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Display(Name = "new password")]
    [Required(ErrorMessage = "Enter a new password.")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Your new password must be between 12 and 128 characters.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "password confirmation")]
    [Required(ErrorMessage = "Confirm your new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "The two new passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IdentityPasswordRules.IsValid(NewPassword))
            yield return new ValidationResult(IdentityPasswordRules.Guidance, [nameof(NewPassword)]);
    }
}

internal static class IdentityPasswordRules
{
    public const string Guidance = "Use 12–128 characters with upper and lower case letters, a digit, a symbol, and at least four distinct characters.";

    public static bool IsValid(string? password) => password is { Length: >= 12 and <= 128 }
        && password.Distinct().Count() >= 4
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit)
        && password.Any(character => !char.IsLetterOrDigit(character));
}
