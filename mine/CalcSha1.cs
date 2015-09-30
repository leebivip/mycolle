using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace sgqy
{
    static class CalcSha1
    {
        static string GetSha1(byte[] input)
        {
            var sha = new SHA1Managed();
            var hash = sha.ComputeHash(input);
            var str = string.Concat(hash.Select(x => string.Format("{0:X2}", x)));
            return str;
        }
        static string GetSha1(string input, Encoding encode)
        { return GetSha1(encode.GetBytes(input)); }
    }
}
