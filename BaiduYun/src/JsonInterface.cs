using System.Collections.Generic;

namespace BaiduYun {
    
    public class ErrInfo {
        public int no { get; set; }
    };

    public class Token {
        public class LoginRecord {
            public List<string> email { get; set; }
            public List<string> phone { get; set; }
        };

        public class TokenData {
            public string rememberedUserName { get; set; }
            public string codeString { get; set; }
            public string token { get; set; }
            public int cookie { get; set; }
            public string usernametype { get; set; }
            public string spLogin { get; set; }
            public string disable { get; set; }
            public LoginRecord loginrecord { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public TokenData data { get; set; }
    };

    public class UBI {
        public class UBIData {
            public List<string> displayname { get; set; }
        }

        public ErrInfo errInfo { get; set; }
        public UBIData data { get; set; }
    };

    public class AccountState {
        public class AccountStateData {
            public string codeString { get; set; }
            public string vcodetype { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public AccountStateData data { get; set; }
    };

    public class RefreshCaptcha {
        public class RefreshCaptchaData {
            public string verifyStr { get; set; }
            public string verifySign { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public RefreshCaptchaData data { get; set; }
    };

    public class RSA {
        public int errno { get; set; }
        public string msg { get; set; }
        public string pubkey { get; set; }
        public string key { get; set; }
    };

    public class VerifyInfo {
        public class VerifyInfoData {
            public string hao123Param { get; set; }
            public string mail { get; set; }
            public string phoneNumber { get; set; }
            public string u { get; set; }
            public string userName { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public VerifyInfoData data { get; set; }
    };

    public class CaptchaInfo {
        public class ErrInfo {
            public string no { get; set; }
            public string msg { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public object data { get; set; }
    };

    // Baidu PCS API
    public class Quota {
        public int errno { get; set; }
        public ulong total { get; set; }
        public int is_show_window { get; set; }
        public ulong request_id { get; set; }
        public ulong used { get; set; }
    };

    public class UserInfo {
        public class UserInfoData {
            public string uname { get; set; }
            public string intro { get; set; }
            public string avatar_url { get; set; }
            public int follow_count { get; set; }
            public int fans_count { get; set; }
            public ulong uk { get; set; }
            public int album_count { get; set; }
            public int pubshare_count { get; set; }
            public int tui_user_count { get; set; }
            public int c2c_user_sell_count { get; set; }
            public int c2c_user_buy_count { get; set; }
            public int c2c_user_product_count { get; set; }
            public int pair_follow_type { get; set; }
        };

        public int errno { get; set; }
        public ulong request_id { get; set; }
        public UserInfoData user_info { get; set; }
    };

    public class YunFile {
        public class Thumbs {
            public string icon { get; set; }
            public string url3 { get; set; }
            public string url2 { get; set; }
            public string url1 { get; set; }
        };

        public int category { get; set; }
        public int unlist { get; set; }
        public ulong fs_id { get; set; }
        public int dir_empty { get; set; }
        public int oper_id { get; set; }
        public long server_mtime { get; set; }
        public long local_mtime { get; set; }
        public long server_ctime { get; set; }
        public long local_ctime { get; set; }
        public ulong size { get; set; }
        public int isdir { get; set; }
        public string path { get; set; }
        public string server_filename { get; set; }
        public int empty { get; set; }

        public string md5 { get; set; }
        public Thumbs thumbs { get; set; }
    };

    public class YunFileList {
        public int errno { get; set; }
        public string guid_info { get; set; }
        public List<YunFile> list { get; set; }
        public ulong request_id { get; set; }
        public ulong guid { get; set; }
    };

    public class FileMeta {
        public int extent_tinyint4 { get; set; }
        public int extent_tinyint3 { get; set; }
        public int extent_tinyint2 { get; set; }
        public int extent_tinyint1 { get; set; }
        public int extent_int3 { get; set; }
        public int category { get; set; }
        public ulong fs_id { get; set; }
        public int isdir { get; set; }
        public int ifhassubdir { get; set; }
        public int errno { get; set; }
        public long server_mtime { get; set; }
        public long server_ctime { get; set; }
        public long local_mtime { get; set; }
        public long local_ctime { get; set; }
        public ulong size { get; set; } 
        public string path { get; set; }
        public string server_filename { get; set; }

        public string dlink { get; set; }
    };

    public class FileMetaList {
        public int errno { get; set; }
        public List<FileMeta> info { get; set; }
        public ulong request_id { get; set; }
    };

    public class DownloadInfo {
        public string host { get; set; }
        public string client_ip { get; set; }
        public List<string> server { get; set; }
        public string path { get; set; }
        public long request_id { get; set; }
    }

    public class TrashInfo {
        public class Info {
            public int errno { get; set; }
            public string path { get; set; }
        };

        public int errno { get; set; }
        public List<Info> info { get; set; }
        public long request_id { get; set; }
    }

    public class SharedFile {
        public long shareId { get; set; }
        public List<string> fsIds { get; set; }
        public int channel { get; set; }
        public List<object> channelInfo { get; set; }
        public int status { get; set; }
        public int expiredType { get; set; }
        public string passwd { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int ctime { get; set; }
        public int appId { get; set; }
        public int @public { get; set; }
        public int publicChannel { get; set; }
        public int tplId { get; set; }
        public string shorturl { get; set; }
        public long ip { get; set; }
        public int tag { get; set; }
        public string shareinfo { get; set; }
        public int bitmap { get; set; }
        public int port { get; set; }
        public object dtime { get; set; }
        public int vCnt { get; set; }
        public int dCnt { get; set; }
        public int tCnt { get; set; }
        public string shortlink { get; set; }
        public int typicalCategory { get; set; }
        public string typicalPath { get; set; }
    };

    public class SharedFileList {
        public int errno { get; set; }
        public long request_id { get; set; }
        public int count { get; set; }
        public int nextpage { get; set; }
        public List<SharedFile> list { get; set; }
    };

    public class ShareResponse {
        public int errno { get; set; }
        public long request_id { get; set; }
        public long shareid { get; set; }
        public string link { get; set; }
        public string shorturl { get; set; }
        public int ctime { get; set; }
        public bool premis { get; set; }
    };
}
