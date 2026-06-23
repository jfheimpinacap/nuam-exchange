namespace NuamExchange.Application.Security;

public sealed class DefaultPasswordPolicy : IPasswordPolicy
{
    public const int MinimumLength = 12;

    public bool IsValid(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumLength)
        {
            return false;
        }

        return password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
