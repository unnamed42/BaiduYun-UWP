using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.Web.Http;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

using Newtonsoft.Json;

namespace BaiduYun {

    public static partial class Utils {

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
            for(var i=0; i<7; ++i) {
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

        public static HttpStringContent UTF8StringContent(string content) {
            return new HttpStringContent(content, 
                                         Windows.Storage.Streams.UnicodeEncoding.Utf8, 
                                         Constants.ACCEPT_FORM);
        }

        public static string QueryString(string mn = "netdisk") {
            return String.Format("&tpl={0}&apiver=v3&tt={1}&subpro=netdisk_web&", mn, TimeStamp());
        }

        public static HttpCookieManager AllCookies() {
            return Globals.filter.CookieManager;
        }

        public static HttpCookie GetCookie(string name, string uri = Constants.PASSPORT_BASE) {
            return AllCookies().GetCookies(new Uri(uri))
                               .Where((x) => x.Name == name)
                               .FirstOrDefault();
        }

        public static string GetCookieValue(string name, string uri = Constants.PASSPORT_BASE) {
            return GetCookie(name, uri)?.Value;
        }

        public static bool HasCookie(string name, string uri = Constants.PASSPORT_BASE) {
            return GetCookie(name, uri) != null;
        }

        public static void ClearAllCookies() {
            var all = AllCookies();
            foreach (var cookie in all.GetCookies(new Uri(Constants.PASSPORT_BASE)))
                all.DeleteCookie(cookie);
            foreach (var cookie in all.GetCookies(new Uri(Constants.PAN_URL)))
                all.DeleteCookie(cookie);
        }

        public static HttpRequestMessage NewRequest(HttpMethod method, string uri) {
            return new HttpRequestMessage(method, new Uri(uri));
        }

        public static HttpRequestMessage GetRequest(string uri) {
            return new HttpRequestMessage(HttpMethod.Get, new Uri(uri));
        }

        public static HttpRequestMessage CommonRequest(string uri) {
            var request = NewRequest(HttpMethod.Get, uri);
            request.Headers.Add("Referer", Constants.REFERER);
            return request;
        }

        public static HttpRequestMessage PostRequest(string uri, string content) {
            return new HttpRequestMessage(HttpMethod.Post, new Uri(uri)) {
                Content = Utils.UTF8StringContent(content),
            };
        }

        public static async Task<HttpResponseMessage> RequestAsync(HttpRequestMessage request) {
            try {
                return await Globals.client.SendRequestAsync(request);
            } catch (Exception) {
                return null;
            }
        }

        public static async Task<HttpResponseMessage> RequestNoRedirectAsync(HttpRequestMessage request) {
            try {
                return await Globals.noRedirectClient.SendRequestAsync(request);
            } catch(Exception) {
                return null;
            }
        }

        public static string RegexMatch(string content, string regex, int group) {
            var match = Regex.Match(content, regex);
            if (match.Success)
                return match.Groups[group].Value;
            return null;
        }

        public static async Task<T> ParseAsObject<T>(HttpRequestMessage request) {
            try {
                using (var response = await Globals.client.SendRequestAsync(request)) {
                    if (response.IsSuccessStatusCode) {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
            } catch(Exception) {}

            return default(T);
        }

        public static async Task<string> ParseAsString(HttpRequestMessage request) {
            try {
                using (var response = await Globals.client.SendRequestAsync(request)) {
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsStringAsync();
                }
            } catch (Exception) {}

            return null;
        }

        // the regex given must have only one matching group
        public static async Task<string> ParseWithRegex(HttpRequestMessage request, string regex, RegexOptions opt = RegexOptions.None) {
            try {
                using (var response = await Globals.client.SendRequestAsync(request)) {
                    if (response.IsSuccessStatusCode) {
                        var str = await response.Content.ReadAsStringAsync();
                        return Utils.RegexMatch(str, regex, 1);
                    }
                }
            } catch(Exception) {}

            return null;
        }

        public static string StripPublicKey(string pubkey) {
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

        public static Dictionary<string, string> ParseQueryString(string s) {
            var dict = new Dictionary<string, string>();

            // remove anything other than query string from url
            if (s.Contains("?"))
                s = s.Substring(s.IndexOf('?') + 1);

            foreach (var vp in s.Split('&')) {
                var pair = vp.Split('=');
                dict.Add(WebUtility.UrlDecode(pair[0]),
                         pair.Length == 2 ? WebUtility.UrlDecode(pair[1]) : "");
            }
            return dict;
        }
        
        // convert size in bytes to human-readable string
        public static string FormattedSize(ulong size) {
            if (size == 0)
                return "-";
            var units = new[] { "", "K", "M", "G", "T", "P", "E", "Z" };
            ulong remainder = 0;
            var unit = units.GetEnumerator();
            while(unit.MoveNext()) {
                if (size < 1024)
                    break;
                remainder = size & 1023;
                size >>= 10;
            }
            return String.Format("{0:N1} {1}B", size + remainder / 1024d, unit.Current);
        }

        public static string FormattedTimeStamp(long timestamp, bool second = true) {
            return (second ? 
                    DateTimeOffset.FromUnixTimeSeconds(timestamp) : 
                    DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                   ).ToString("yyyy-MM-dd HH:mm");
        }
    };
}
