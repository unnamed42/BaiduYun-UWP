using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace BaiduYun.Misc {
    public static class Utils {
        public static long TimeStamp(bool milliseconds = true) {
            return milliseconds ?
                   DateTimeOffset.Now.ToUnixTimeMilliseconds() :
                   DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static string Base36Encode(long input) {
            if (input < 0)
                throw new ArgumentOutOfRangeException("input", input, "input cannot be negative");

            var clistarr = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
            var result = new Stack<char>();
            while (input != 0) {
                result.Push(clistarr[input % 36]);
                input /= 36;
            }
            return new string(result.ToArray());
        }

        public static string CallbackGenerator() {
            double d = new Random(DateTime.Now.Millisecond).NextDouble();
            return "bd__cbs__" + Base36Encode((long)Math.Floor(2147483648 * d));
        }

        public static string GIDGenerator() {
            var sb = new StringBuilder();
            var random = new Random(DateTime.Now.Millisecond);
            for (var i = 0; i < 7; ++i) {
                double d = random.NextDouble();
                var n = (int)(16 * d);
                sb.Append(Convert.ToString(n, 16));
            }
            sb.Append('-');
            for (var i = 0; i < 4; ++i) {
                double d = random.NextDouble();
                var n = (int)(16 * d);
                sb.Append(Convert.ToString(n, 16));
            }
            sb.Append("-4");
            for (var i = 0; i < 3; ++i) {
                double d = random.NextDouble();
                var n = (int)(16 * d);
                sb.Append(Convert.ToString(n, 16));
            }
            sb.Append(Convert.ToString(8, 16));
            for (var i = 0; i < 3; ++i) {
                double d = random.NextDouble();
                var n = (int)(16 * d);
                sb.Append(Convert.ToString(n, 16));
            }
            sb.Append('-');
            for (var i = 0; i < 12; ++i) {
                double d = random.NextDouble();
                var n = (int)(16 * d);
                sb.Append(Convert.ToString(n, 16));
            }
            return sb.ToString().ToUpper();
        }

        public static string LogIDGenerator() {
            string codepoint = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/~！@#￥%……&";
            return Regex.Replace(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + new Random(), @"[\s\S]{1,3}", (match) => {
                var str = match.Value;
                var t = new[] { 0, 2, 1 }[str.Length % 3];
                uint r = (uint)(str[0] << 16 | (str.Length > 1 ? str[1] : 0) << 8 | (str.Length > 2 ? str[2] : 0));
                return new string(new char[] {
                    codepoint[(int)(r >> 18)],
                    codepoint[(int)(r >> 12) & 63],
                    t >= 2 ? '=' : codepoint[(int)(r >> 6) & 63],
                    t >= 1 ? '=' : codepoint[(int)r & 63]
                });
            });
        }

        public static void Swap<T>(ref T a, ref T b) {
            T temp = a;
            a = b;
            b = temp;
        }

        public static string RegexSearch(string input, string regex) {
            var match = Regex.Match(input, regex);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string QueryString(string mn = "netdisk") {
            return String.Format("&tpl={0}&apiver=v3&tt={1}&subpro=netdisk_web", mn, TimeStamp());
        }

        private static string StripPublicKey(string pubkey) {
            // pubkey has header and footer
            return pubkey.Replace("\n", "")
                         .Replace("-----BEGIN PUBLIC KEY-----", "")
                         .Replace("-----END PUBLIC KEY-----", "");
        }

        public static string RSA_PCKS1(string pubkey, string message) {
            pubkey = StripPublicKey(pubkey);

            var asymmAlgo = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
            var key = asymmAlgo.ImportPublicKey(CryptographicBuffer.DecodeFromBase64String(pubkey), CryptographicPublicKeyBlobType.X509SubjectPublicKeyInfo);
            var content = CryptographicBuffer.ConvertStringToBinary(message, BinaryStringEncoding.Utf8);
            var encrypted = CryptographicEngine.Encrypt(key, content, null);
            return CryptographicBuffer.EncodeToBase64String(encrypted);
        }

        // convert size in bytes to human-readable string
        public static string SanitizeSize(ulong size) {
            if (size == 0)
                return "-";
            var units = new[] { "", "K", "M", "G", "T", "P", "E", "Z" };
            ulong remainder = 0;
            var unit = units.GetEnumerator();
            while (unit.MoveNext()) {
                if (size < 1024)
                    break;
                remainder = size & 1023;
                size >>= 10;
            }
            return String.Format("{0:N1} {1}B", size + remainder / 1024d, unit.Current);
        }

        public static string SanitizeTimeStamp(long timestamp, bool second = true) {
            return (second ?
                    DateTimeOffset.FromUnixTimeSeconds(timestamp) :
                    DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                   ).ToString("yyyy-MM-dd HH:mm");
        }
    }
}
