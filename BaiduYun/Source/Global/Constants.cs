namespace BaiduYun.Global {

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

        public const string CONFIG_FILE = "everything.json";
    }
}
