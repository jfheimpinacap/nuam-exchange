namespace NuamExchange.Application.Security;

public interface IPasswordPolicy
{
    bool IsValid(string? password);
}
