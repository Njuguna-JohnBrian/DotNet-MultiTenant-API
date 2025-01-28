namespace MultitenancyApp.Interfaces;

public interface IPasswordService
{
    string CreatePasswordHash(string rawPassword);

    bool PasswordIsValid(string rawPassword, string hashedPassword);
}