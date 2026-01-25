using System.Security.Cryptography;
using System.Text;

public static class LicenseCrypto
{
    private static readonly string SECRET_KEY = "POS_SECRET_2026!@#";

    public static byte[] Sign(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SECRET_KEY));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    }

    public static bool Verify(string payload, byte[] signature)
    {
        var expected = Sign(payload);
        return CryptographicOperations.FixedTimeEquals(expected, signature);
    }
}
