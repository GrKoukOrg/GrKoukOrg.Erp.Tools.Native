using System.Security.Cryptography;
using System.Text;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

public static class ChecksumHelper
{
    public static string CalculateChecksum(params string[] fields)
    {
        var concatenated = string.Join("|", fields);
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}