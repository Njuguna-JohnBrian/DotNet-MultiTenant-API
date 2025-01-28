namespace MultitenancyApp.Services;

using MultitenancyApp.Interfaces;
using BC = BCrypt.Net.BCrypt;

public class PasswordService : IPasswordService
{
    /// <summary>
    /// Creates a password hash
    /// </summary>
    /// <param name="rawPassword">The password to hash</param>
    /// <returns>The hashed password</returns>
    public string CreatePasswordHash(string rawPassword)
    {
        var encDataByte = System.Text.Encoding.UTF8.GetBytes(rawPassword);
        string encodedData = Convert.ToBase64String(encDataByte);
        return encodedData;
    }

    /// <summary>
    /// Compares the hashed password with the un-hashed password
    /// </summary>
    /// <param name="rawPassword">un-hashed password</param>
    /// <param name="hashedPassword">hashed password</param>
    /// <returns>true if password matched or false</returns>
    public bool PasswordIsValid(string rawPassword, string hashedPassword)
    {
        return DecryptPassword(hashedPassword) == rawPassword;
    }

    public string DecryptPassword(string encryptedPassword)
    {
        var encoder = new System.Text.UTF8Encoding();
        var utf8Decode = encoder.GetDecoder();
        byte[] toDecodeByte = Convert.FromBase64String(encryptedPassword);
        int charCount = utf8Decode.GetCharCount(toDecodeByte, 0, toDecodeByte.Length);
        char[] decodedChar = new char[charCount];
        utf8Decode.GetChars(toDecodeByte, 0, toDecodeByte.Length, decodedChar, 0);
        string result = new String(decodedChar);
        return result;
    }
}