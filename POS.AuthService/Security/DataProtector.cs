using System.Security.Cryptography;
using System.Text;

public static class DataProtector
{
    private static readonly byte[] Key =
        SHA256.HashData(Encoding.UTF8.GetBytes("POS_DATA_KEY_2026"));

    public static byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

        return aes.IV.Concat(encrypted).ToArray();
    }

    public static byte[] Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        aes.Key = Key;

        var iv = encryptedData.Take(16).ToArray();
        var cipher = encryptedData.Skip(16).ToArray();

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
    }
}
