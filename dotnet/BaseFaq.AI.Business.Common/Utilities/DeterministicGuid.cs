using System.Security.Cryptography;
using System.Text;

namespace BaseFaq.AI.Business.Common.Utilities;

public static class DeterministicGuid
{
    public static Guid CreateV3(params string[] segments)
    {
        var serialized = string.Join(':', segments);
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(serialized));

        hash[6] = (byte)((hash[6] & 0x0F) | (3 << 4));
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        return new Guid(hash);
    }
}
