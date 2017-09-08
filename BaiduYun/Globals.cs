using System.Threading.Tasks;

using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Data.Json;
using Windows.Security.Credentials;

namespace BaiduYun {

    public class Errors {
        public const int SYSTEM_ERROR = -1;
        public const int SERVER_MAINTAINANCE = -2;
        public const int OK = 0;
        public const int INEXIST_ACCOUNT = 2;
        public const int CAPTCHA = 7;
        public const int WRONG_CAPTCHA = 6;
        public const int CANNOT_LOGIN_NOW = 16;
        public const int CAPTCHA_REQUIRED = 257;
        public const int MESSAGE_AUTH_REQUIRED = 400031;
        public const int EXTERNAL_VERIFICATION_REQUIRED = 120021;
        public const int ACCOUNT_FROZEN = 120019;
        public const int COOKIE_DISABLED = 100023;
        public const int INEXIST_OR_OUTDATED_CAPTCHA = 200010;
        public const int TOO_FREQUENT_LOGIN = 500010;
        public const int CELLPHONE_BINDING_REQUIRED = 18;
    };

    public static class Icons {
        public const string Menu = "\uf0c9";
        public const string Home = "\uf015";
        public const string Download = "\uf019";
        public const string Share = "\uf1e0";
        public const string File = "\uf016";
        public const string TextFile = "\uf0f6";
        public const string ImageFile = "\uf1c5";
        public const string ArchiveFile = "\uf1c6";
        public const string CodeFile = "\uf1c9";
        public const string AudioFile = "\uf1c7";
        public const string VideoFile = "\uf1c8";
        public const string PDFFile = "\uf1c1";
        public const string Folder = "\uf115";
    };

    public static class Globals {
        public static HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter() { AllowAutoRedirect = false };
        public static HttpClient client = new HttpClient();
        public static HttpClient noRedirectClient = new HttpClient(filter);
        
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static StorageFolder cacheRoot = ApplicationData.Current.LocalCacheFolder;
        public static StorageFolder localRoot = ApplicationData.Current.LocalFolder;

        public static JsonObject everything = null;
        public static PasswordVault vault = new PasswordVault();

        public static DownloadManager downloads = new DownloadManager();

        public static string bdstoken = null;
        public static string dsign = null;

        public static async Task InitGlobals() {
            everything = await LocalUtils.ReadSettings();
            client.DefaultRequestHeaders.TryAppendWithoutValidation("User-Agent", Constants.GUANJIA_AGENT);
            noRedirectClient.DefaultRequestHeaders.TryAppendWithoutValidation("User-Agent", Constants.GUANJIA_AGENT);
            downloads.InitTasks().ContinueWith((task) => {
                downloads.ResumeAll();
            });
        }

        public static async Task DestructGlobals() {
            await LocalUtils.SaveSettings();
            downloads.PauseAll();
            client.Dispose();
            noRedirectClient.Dispose();
        }
    };

    public static class Constants {
        public const int APP_ID = 9436689;
        public const string APP_KEY = "8mM8AmPiCvqPEzi4j3gWrs0a";
        public const string APP_SECRET = "uGZmdhtFalnke7bf52vtjjNd19KjGuCa";

        public const int PC_CLIENT_TYPE = 8;

        public const string PCS_URL = "https://pcs.baidu.com/rest/2.0/pcs/";
        public const string DOWNLOAD_URL = "https://d.pcs.baidu.com/rest/2.0/pcs/";

        public const string BAIDU_URL = "https://www.baidu.com/";

        public const string PASSPORT_BASE = "https://passport.baidu.com/";
        public const string PASSPORT_URL = PASSPORT_BASE + "v2/api/";
        public const string PASSPORT_LOGIN = PASSPORT_BASE + "v2/api/?login";

        public const string PAN_URL = "https://pan.baidu.com/";
        public const string PAN_HOME = PAN_URL + "disk/home/";
        public const string PAN_API = PAN_URL + "api/";

        public const string REFERER = PASSPORT_BASE + "v2/?login";
        public const string PAN_REFERER = "https://pan.baidu.com/disk/home";

        public const string ACCEPT_JSON = "application/json, text/javascript, */*; q=0.8";
        public const string ACCEPT_HTML = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        public const string ACCEPT_FORM = "application/x-www-form-urlencoded";

        public const string UK_RE = @"""uk"":(\d+)";
        public const string BDSTOKEN_RE = @"""bdstoken""\s*:\s*""([^""]+)""";
        public const string LOGIN_ERROR_RE = @"""(err_no[^""]+)";
        public const string USERNAME_RE = @"""username""\s*:\s*""([^""]+)""";

        public const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64; rv:31.0) Gecko/20100101 Firefox/31.0 Iceweasel/31.2.0";
        public const string GUANJIA_AGENT = "netdisk;4.6.2.0;PC;PC-Windows;10.0.10240;WindowsBaiduYunGuanJia";
    };

    public static class Configs {
        public const string CONFIG_FILE = "everything.json";

    };
}