using System;
using System.Text;
using System.Security.Cryptography;

public class SecurityHelper
{
    public static byte[] Hash(string data)
    {
        byte[] textToBytes = Encoding.UTF8.GetBytes(data);
        SHA256Managed mySHA256 = new SHA256Managed();

        byte[] hashValue = mySHA256.ComputeHash(textToBytes);

        return hashValue;

    }

    public static string GetHexStringFromHash(byte[] hash)
    {
        string hexString = String.Empty;

        foreach (byte b in hash)
            hexString += b.ToString("x2");

        return hexString;
    }
}
