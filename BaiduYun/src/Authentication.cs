using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Windows.Storage.Streams;

namespace BaiduYun {

    public partial class BaiduYun {

        // Get BAIDUID for further use
        public static async Task<bool> Init() {
            var url = Constants.PASSPORT_URL + "?getapi" + 
                      Utils.QueryString() +
                      "class=login&logintype=basicLogin";

            using (var request = Utils.GetRequest(url))
            using (var response = await Utils.RequestAsync(request))
                return response != null && response.IsSuccessStatusCode;
        }

        public static async Task<string> GetToken(string gid) {
            var url = Constants.PASSPORT_URL + String.Format("?getapi&tpl=netdisk&subpro=netdisk_web&apiver=v3&tt={0}&gid={1}&class=login&logintype=basicLogin",
                                                             Utils.TimeStamp(), gid);

            using (var request = Utils.GetRequest(url)) {
                request.Headers.Add("Cache-control", "max-age=0");

                var json = await Utils.ParseAsObject<Token>(request);
                return (json != null && json.errInfo.no == 0) ? json.data.token : null;
            }
        }

        public static async Task<UBI> CheckLoginHistory(string token) {
            var url = Constants.PASSPORT_URL + "?loginhistory" + 
                      Utils.QueryString() +
                      String.Format("token={0}", token);

            using (var request = Utils.CommonRequest(url))
                return await Utils.ParseAsObject<UBI>(request);
        }

        public static async Task<AccountState> CheckAccountState(string token, string username) {
            var url = Constants.PASSPORT_URL + "?logincheck" + 
                      Utils.QueryString() +
                      String.Format("token={0}&username={1}", token, WebUtility.UrlEncode(username));

            using (var request = Utils.CommonRequest(url))
                return await Utils.ParseAsObject<AccountState>(request);
        }

        public static async Task<IBuffer> CaptchaImage(string codestring) {
            var url = Constants.PASSPORT_BASE +
                      String.Format("cgi-bin/genimage?{0}", WebUtility.UrlEncode(codestring));

            using (var request = Utils.CommonRequest(url))
            using (var response = await Utils.RequestAsync(request))
                return await response?.Content.ReadAsBufferAsync();
        }

        public static async Task<bool> CheckCaptcha(string token, string codestring, string verifycode) {
            var url = Constants.PASSPORT_BASE + "v2/?checkvcode" + 
                      Utils.QueryString() +
                      String.Format("token={0}&verifycode={1}&codestring={2}",
                                    token, WebUtility.UrlEncode(verifycode), WebUtility.UrlEncode(codestring));

            using (var request = Utils.GetRequest(url)) {
                var response = await Utils.ParseAsObject<CaptchaInfo>(request);
                return response != null && response.errInfo.no == "0";
            }
        }

        public static async Task<RefreshCaptcha> RefreshCaptcha(string token, string vcodetype) {
            var url = Constants.PASSPORT_BASE + "v2/?reggetcodestr" + 
                      Utils.QueryString() +
                      String.Format("token={0}&fr=login&vcodetype={1}",
                                    token, WebUtility.UrlEncode(vcodetype));

            using (var request = Utils.CommonRequest(url))
                return await Utils.ParseAsObject<RefreshCaptcha>(request);
        }

        public static async Task<RSA> GetRSA(string token) {
            var url = Constants.PASSPORT_BASE + "v2/getpublickey?" + 
                      String.Format("token={0}", token) +
                      Utils.QueryString();

            using (var request = Utils.CommonRequest(url))
                return await Utils.ParseAsObject<RSA>(request);
        }

        public static async Task<bool> SendEmailVCode(string authtoken) {
            var url = Constants.PASSPORT_BASE + "v2/sapi/authwidgetverify?" + 
                      String.Format("authtoken={0}&type=email&apiver=v3&action=send&vcode=&verifychannel=&questionAndAnswer=&needsid=&rsakey=&countrycode=&subpro=netdisk_web", authtoken);

            using (var request = Utils.GetRequest(url)) {
                request.Headers.Add("Referer", Constants.PAN_URL);

                using (var response = await Utils.RequestAsync(request)) {
                    var str = await response.Content.ReadAsStringAsync();
                    // TODO: errno must equals 110000
                    return response != null && response.IsSuccessStatusCode;
                }
            }
        }

        public static async Task<bool> VerifyEmailVCode(string authtoken, string verifycode) {
            var url = Constants.PASSPORT_BASE + "v2/sapi/authwidgetverify?" +
                      String.Format("authtoken={0}&type=email&apiver=v3&action=check&vcode={1}&verifychannel=&questionAndAnswer=&needsid=&rsakey=&countrycode=&subpro=netdisk_web", authtoken, verifycode);

            using (var request = Utils.GetRequest(url)) {
                request.Headers.Add("Referer", Constants.PAN_URL);

                using (var response = await Utils.RequestAsync(request)) {
                    var str = await response.Content.ReadAsStringAsync();
                    // TODO: errno must equals 110000
                    return response != null && response.IsSuccessStatusCode;
                }
            }
        }

        public static async Task<bool> ReVerifyLogin(string lstr, string ltoken) {
            var url = Constants.PASSPORT_BASE + "v2/?loginproxy" +
                      String.Format("&u={0}&tpl=netdisk&ltoken={1}&lstr={2}&apiver=v3&tt={3}",
                                    WebUtility.UrlEncode(Constants.PAN_HOME), ltoken, lstr, Utils.TimeStamp());

            using (var request = Utils.GetRequest(url)) {
                request.Headers.Add("Referer", Constants.PAN_URL);

                var json = await Utils.ParseAsObject<VerifyInfo>(request);
                return json != null && json.errInfo.no == 0;
            }
        }

        public static async Task<string> Login(string token, RSA rsa, string username, string password, string codestring, string verifycode, string gid) {
            var sb = new StringBuilder();
            sb.Append("staticpage=").Append(WebUtility.UrlEncode("http://pan.baidu.com/res/static/thirdparty/pass_v3_jump.html"))
              .Append("&charset=utf-8")
              .Append("&token=").Append(token)
              .Append("&tpl=netdisk&subpro=netdisk_web&apiver=v3")
              .Append("&tt=").Append(Utils.TimeStamp())
              .Append("&codestring=").Append(WebUtility.UrlEncode(codestring))
              .Append("&safeflg=0&u=").Append(WebUtility.UrlEncode(Constants.PAN_HOME))
              .Append("&isPhone=false&detect=1&gid=").Append(gid)
              .Append("&quick_user=0&logintype=basicLogin&logLoginType=pc_loginBasic&idc=")
              .Append("&loginmerge=true&foreignusername=")
              .Append("&username=").Append(WebUtility.UrlEncode(username))
              .Append("&password=").Append(WebUtility.UrlEncode(Utils.RSA_PCKS1(rsa.pubkey, password)))
              .Append("&verifycode=").Append(WebUtility.UrlEncode(verifycode))
              .Append("&mem_pass=on")
              .Append("&rsakey=").Append(WebUtility.UrlEncode(rsa.key)) // private key
              .Append("&crypttype=12&ppui_logintime=").Append(17966)
              .Append("&countrycode=")
              .Append("&dv=").Append(WebUtility.UrlEncode("MDEwAAoAdAAKA4cAIwAAAF00AA0CABvLy-o4Lno7dTJgIWwzbDxvP2BYB1grXjxROEwNAgAfy8vqIjhsLWMkdjd6JXoqeSl2ThFOOF0vRiBZGnURdA0CAB_Ly9rn_anopuGz8r_gv--87LOL1Iv9mOqD5ZzfsNSxDQIAHcvL2vTsuPm38KLjrvGu_q39oprFmuqL-Iv8k-GFDAIAI9OYmJiYiAZSE10aSAlEG0QURxdIcC9wAGESYRZ5C28fbBt_DAIAI9OwsLCwvA9bGlQTQQBNEk0dTh5BeSZ5CWgbaB9wAmYWZRJ2BwIABMvLy8sHAgAEy8vLywwCACPTtLS0tLJGElMdWghJBFsEVAdXCDBvMEAhUiFWOUsvXyxbPwcCAATLy8vLCQIAJNPQ3tze3t7e3t2np_Oy_LvpqOW65bXmtunRjtGhwLPAt9iqzgcCAATLy8vLEwIAGsvd3d21wbXFtoyjjPuM-9W31r_broDjjOHOEAIAAcsXAgAIycnW1tXy2ZQWAgAi6p71xevc6tjo3erc797q3unY7Nvs3enQ4tbl0-HU5Nzq3gUCAATLy8vBAQIABsvJycZCtBUCAAjLy8qR7cw8wwQCAAbJycvK_84OAgABywYCACjLy8uHh4eHh4eHgYGBgYNvb29paWlpagoKCgwMDAwPNzc3MT8_Pzx6DQIABcvLy-npDQIAHcvLy-_3o-Ks67n4teq15bbmuYHegfGQ45DniPqeCAIAIdPQxsc1NTU3jNiZ15DCg86Rzp7NncL6pfqK65jrnPOB5QwCACPTtLS0tLPLn96Q14XEidaJ2Yrahb3ivc2s36zbtMai0qHWsgwCACPTsrKysrqcyInHgNKT3oHejt2N0uq16pr7iPuM45H1hfaB5QcCAATLy8vLDAIAI9O0tLS0vA9bGlQTQQBNEk0dTh5BeSZ5CWgbaB9wAmYWZRJ2DAIAI9O2tra2vy56O3UyYCFsM2w8bz9gWAdYKEk6ST5RI0c3RDNXDAIAI9O0tLS0vu66-7XyoOGs86z8r_-gmMeY6In6if6R44f3hPOXDAIAI9PDw8PDzip-P3E2ZCVoN2g4aztkXANcLE0-TTpVJ0MzQDdTDAIAI9Obn5-fkKn9vPK156brtOu76Ljn34Dfr869zrnWpMCww7TQCQIAIt3e2thXV1dXV3bFxZHQntmLyofYh9eE1Iuz7LPAtde606cHAgAEy8vLyw"))
              .Append("&callback=parent.bd__pcbs__8nq67t");

            using (var request = Utils.PostRequest(Constants.PASSPORT_LOGIN, sb.ToString())) {
                request.Headers.Add("Referer", Constants.PAN_URL);
                return await Utils.ParseWithRegex(request, Constants.LOGIN_ERROR_RE, RegexOptions.IgnoreCase);
            }
        }

        public static async Task<bool> Logout() {
            var url = Constants.PASSPORT_BASE + "?logout&u=https://www.baidu.com";
            using (var request = Utils.GetRequest(url))
            using (var response = await Utils.RequestAsync(request))
                return response != null && response.IsSuccessStatusCode;
        }
    }
}
