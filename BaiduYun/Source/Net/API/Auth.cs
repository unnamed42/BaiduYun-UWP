using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Windows.Storage.Streams;

using BaiduYun.Misc;
using BaiduYun.Global;
using BaiduYun.Net.API.Response;

namespace BaiduYun.Net.API {

    public class Auth {

        // Get BAIDUID for further use
        public static async Task<bool> Init() {
            var sb = new StringBuilder(Constants.PASSPORT_URL);
            sb.Append("?getapi").Append(Utils.QueryString())
              .Append("&class=login")
              .Append("&logintype=basicLogin");

            return await WebUtils.GetAsync(sb.ToString(), null, async (response) => {
                return await Task.Run(() => true);
            });
        }

        public static async Task<string> GetToken(string gid) {
            var sb = new StringBuilder(Constants.PASSPORT_URL);
            sb.Append("?getapi").Append("&tpl=netdisk")
              .Append("&subpro=netdisk_web").Append("&apiver=v3")
              .Append("&tt=").Append(Utils.TimeStamp())
              .Append("&gid=").Append(gid)
              .Append("&class=login")
              .Append("&logintype=basicLogin");

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Cache-control", "max-age=0");
            }, async (response) => {
                var token = await WebUtils.ParseAs<Token>(response);
                return token != null && token.errInfo.no == 0 ? token.data.token : null;
            });
        }

        public static async Task<UBI> CheckLoginHistory(string token) {
            var sb = new StringBuilder(Constants.PASSPORT_URL);
            sb.Append("?loginhistory")
              .Append(Utils.QueryString())
              .Append("&token=").Append(token);

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.REFERER);
            }, WebUtils.ParseAs<UBI>);
        }

        public static async Task<AccountState> CheckAccountState(string token, string username) {
            var sb = new StringBuilder(Constants.PASSPORT_URL);
            sb.Append("?logincheck").Append(Utils.QueryString())
              .Append("&token=").Append(token)
              .Append("&username=").Append(WebUtils.UrlEncode(username));

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.REFERER);
            }, WebUtils.ParseAs<AccountState>);
        }

        public static async Task<IBuffer> CaptchaImage(string codestring) {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("cgi-bin/genimage?").Append(WebUtils.UrlEncode(codestring));

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.REFERER);
            }, async (response) => {
                return await response.Content.ReadAsBufferAsync();
            });
        }

        public static async Task<bool> CheckCaptcha(string token, string codestring, string verifycode) {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("v2/?checkvcode").Append(Utils.QueryString())
              .Append("&token=").Append(token)
              .Append("&verifycode=").Append(WebUtils.UrlEncode(verifycode))
              .Append("&codestring=").Append(WebUtils.UrlEncode(codestring));

            return await WebUtils.GetAsync(sb.ToString(), null, async (response) => {
                var captcha = await WebUtils.ParseAs<CaptchaInfo>(response);
                return captcha?.errInfo.no == "0";
            });
        }

        public static async Task<RefreshCaptcha> RefreshCaptcha(string token, string vcodetype) {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("v2/?reggetcodestr").Append(Utils.QueryString())
              .Append("&token=").Append(token)
              .Append("&fr=login&vcodetype=").Append(WebUtils.UrlEncode(vcodetype));

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.REFERER);
            }, WebUtils.ParseAs<RefreshCaptcha>);
        }

        public static async Task<RSA> GetRSA(string token) {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("v2/getpublickey?").Append(Utils.QueryString())
              .Append("&token=").Append(token);

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.REFERER);
            }, WebUtils.ParseAs<RSA>);
        }

        public static async Task<bool> SendEmailVCode(string authtoken) {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("v2/sapi/authwidgetverify?")
              .Append("apiver=v3&subpro=netdisk_web")
              .Append("&authtoken=").Append(authtoken)
              .Append("&type=email").Append("&action=send")
              .Append("&vcode=&verifychannel=&questionAndAnswer=&needsid=&rsakey=&countrycode=");

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.PAN_URL);
            }, async (response) => {
                // TODO: errno must equals 110000
                // var str = await response.Content.ReadAsStringAsync();
                return await Task.Run(() => true);
            });
        }

        public static async Task<bool> VerifyEmailVCode(string authtoken, string verifycode) {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("v2/sapi/authwidgetverify?")
              .Append("apiver=v3&subpro=netdisk_web")
              .Append("&authtoken=").Append(authtoken)
              .Append("&type=email&action=check")
              .Append("&vcode=").Append(verifycode)
              .Append("&verifychannel=&questionAndAnswer=&needsid=&rsakey=&countrycode=");

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.PAN_URL);
            }, async (response) => {
                // TODO: errno must equals 110000
                // var str = await response.Content.ReadAsStringAsync();
                return await Task.Run(() => true);
            });
        }

        public static async Task<bool> ReVerifyLogin(string lstr, string ltoken) {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("v2/?loginproxy").Append(Utils.QueryString())
              .Append("&u=").Append(WebUtils.UrlEncode(Constants.PAN_HOME))
              .Append("&ltoken=").Append(ltoken)
              .Append("&lstr=").Append(lstr);

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.PAN_URL);
            }, async (response) => {
                var json = await WebUtils.ParseAs<VerifyInfo>(response);
                return json?.errInfo.no == 0;
            });
        }

        public static async Task<string> Login(string token, RSA rsa, string username, string password, string codestring, string verifycode, string gid) {
            var sb = new StringBuilder();
            sb.Append("staticpage=").Append(WebUtils.UrlEncode("http://pan.baidu.com/res/static/thirdparty/pass_v3_jump.html"))
              .Append("&charset=utf-8")
              .Append("&token=").Append(token)
              .Append("&tpl=netdisk&subpro=netdisk_web&apiver=v3")
              .Append("&tt=").Append(Utils.TimeStamp())
              .Append("&codestring=").Append(WebUtils.UrlEncode(codestring))
              .Append("&safeflg=0&u=").Append(WebUtils.UrlEncode(Constants.PAN_HOME))
              .Append("&isPhone=false&detect=1&gid=").Append(gid)
              .Append("&quick_user=0&logintype=basicLogin&logLoginType=pc_loginBasic&idc=")
              .Append("&loginmerge=true&foreignusername=")
              .Append("&username=").Append(WebUtils.UrlEncode(username))
              .Append("&password=").Append(WebUtils.UrlEncode(Utils.RSA_PCKS1(rsa.pubkey, password)))
              .Append("&verifycode=").Append(WebUtils.UrlEncode(verifycode))
              .Append("&mem_pass=on")
              .Append("&rsakey=").Append(WebUtils.UrlEncode(rsa.key)) // private key
              .Append("&crypttype=12&ppui_logintime=").Append(17966)
              .Append("&countrycode=")
              .Append("&dv=").Append(WebUtils.UrlEncode("MDEwAAoAdAAKA4cAIwAAAF00AA0CABvLy-o4Lno7dTJgIWwzbDxvP2BYB1grXjxROEwNAgAfy8vqIjhsLWMkdjd6JXoqeSl2ThFOOF0vRiBZGnURdA0CAB_Ly9rn_anopuGz8r_gv--87LOL1Iv9mOqD5ZzfsNSxDQIAHcvL2vTsuPm38KLjrvGu_q39oprFmuqL-Iv8k-GFDAIAI9OYmJiYiAZSE10aSAlEG0QURxdIcC9wAGESYRZ5C28fbBt_DAIAI9OwsLCwvA9bGlQTQQBNEk0dTh5BeSZ5CWgbaB9wAmYWZRJ2BwIABMvLy8sHAgAEy8vLywwCACPTtLS0tLJGElMdWghJBFsEVAdXCDBvMEAhUiFWOUsvXyxbPwcCAATLy8vLCQIAJNPQ3tze3t7e3t2np_Oy_LvpqOW65bXmtunRjtGhwLPAt9iqzgcCAATLy8vLEwIAGsvd3d21wbXFtoyjjPuM-9W31r_broDjjOHOEAIAAcsXAgAIycnW1tXy2ZQWAgAi6p71xevc6tjo3erc797q3unY7Nvs3enQ4tbl0-HU5Nzq3gUCAATLy8vBAQIABsvJycZCtBUCAAjLy8qR7cw8wwQCAAbJycvK_84OAgABywYCACjLy8uHh4eHh4eHgYGBgYNvb29paWlpagoKCgwMDAwPNzc3MT8_Pzx6DQIABcvLy-npDQIAHcvLy-_3o-Ks67n4teq15bbmuYHegfGQ45DniPqeCAIAIdPQxsc1NTU3jNiZ15DCg86Rzp7NncL6pfqK65jrnPOB5QwCACPTtLS0tLPLn96Q14XEidaJ2Yrahb3ivc2s36zbtMai0qHWsgwCACPTsrKysrqcyInHgNKT3oHejt2N0uq16pr7iPuM45H1hfaB5QcCAATLy8vLDAIAI9O0tLS0vA9bGlQTQQBNEk0dTh5BeSZ5CWgbaB9wAmYWZRJ2DAIAI9O2tra2vy56O3UyYCFsM2w8bz9gWAdYKEk6ST5RI0c3RDNXDAIAI9O0tLS0vu66-7XyoOGs86z8r_-gmMeY6In6if6R44f3hPOXDAIAI9PDw8PDzip-P3E2ZCVoN2g4aztkXANcLE0-TTpVJ0MzQDdTDAIAI9Obn5-fkKn9vPK156brtOu76Ljn34Dfr869zrnWpMCww7TQCQIAIt3e2thXV1dXV3bFxZHQntmLyofYh9eE1Iuz7LPAtde606cHAgAEy8vLyw"))
              .Append("&callback=parent.bd__pcbs__8nq67t");

            return await WebUtils.PostAsync(Constants.PASSPORT_LOGIN, sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.PAN_URL);
            }, async (response) => {
                var content = await response.Content.ReadAsStringAsync();
                var match = Regex.Match(content, Constants.LOGIN_ERROR_RE, RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : null;
            });
        }

        public static async Task<bool> Logout() {
            var sb = new StringBuilder(Constants.PASSPORT_BASE);
            sb.Append("?logout")
              .Append("&u=").Append(WebUtils.UrlEncode(Constants.BAIDU_URL));

            return await WebUtils.GetAsync(sb.ToString(), null, async (response) => {
                return await Task.Run(() => true);
            });
        }
    }
}
