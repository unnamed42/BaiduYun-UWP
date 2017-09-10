using System.Collections.Generic;

namespace BaiduYun.Net.API.Response {

    public class ErrInfo {
        public int no;
    }

    public class AccountState {
        public class AccountStateData {
            public string codeString;
            public string vcodetype;
        };

        public ErrInfo errInfo;
        public AccountStateData data;
    };

    public class CaptchaInfo {
        public class ErrInfo {
            public string no;
            public string msg;
        };

        public ErrInfo errInfo;
        public object data;
    };

    public class DownloadInfo {
        public string host;
        public string client_ip;
        public List<string> server;
        public string path;
        public long request_id;
    }

    public class FileMeta {
        public int extent_tinyint4;
        public int extent_tinyint3;
        public int extent_tinyint2;
        public int extent_tinyint1;
        public int extent_int3;
        public int category;
        public ulong fs_id;
        public int isdir;
        public int ifhassubdir;
        public int errno;
        public long server_mtime;
        public long server_ctime;
        public long local_mtime;
        public long local_ctime;
        public ulong size;
        public string path;
        public string server_filename;

        public string dlink;
    };

    public class FileMetaList {
        public int errno;
        public List<FileMeta> info;
        public ulong request_id;
    };

    public class Quota {
        public int errno;
        public ulong total;
        public int is_show_window;
        public ulong request_id;
        public ulong used;
    };

    public class RefreshCaptcha {
        public class RefreshCaptchaData {
            public string verifyStr;
            public string verifySign;
        };

        public ErrInfo errInfo;
        public RefreshCaptchaData data;
    };

    public class RSA {
        public int errno;
        public string msg;
        public string pubkey;
        public string key;
    };

    public class SharedFile {
        public long shareId;
        public List<string> fsIds;
        public int channel;
        public List<object> channelInfo;
        public int status;
        public int expiredType;
        public string passwd;
        public string name;
        public string description;
        public int ctime;
        public int appId;
        public int @public;
        public int publicChannel;
        public int tplId;
        public string shorturl;
        public long ip;
        public int tag;
        public string shareinfo;
        public int bitmap;
        public int port;
        public object dtime;
        public int vCnt;
        public int dCnt;
        public int tCnt;
        public string shortlink;
        public int typicalCategory;
        public string typicalPath;
    };

    public class SharedFileList {
        public int errno;
        public long request_id;
        public int count;
        public int nextpage;
        public List<SharedFile> list;
    };

    public class ShareResponse {
        public int errno;
        public long request_id;
        public long shareid;
        public string link;
        public string shorturl;
        public int ctime;
        public bool premis;
    };

    public class Token {
        public class LoginRecord {
            public List<string> email;
            public List<string> phone;
        };

        public class TokenData {
            public string rememberedUserName;
            public string codeString;
            public string token;
            public int cookie;
            public string usernametype;
            public string spLogin;
            public string disable;
            public LoginRecord loginrecord;
        };

        public ErrInfo errInfo;
        public TokenData data;
    };

    public class TrashInfo {
        public class Info {
            public int errno;
            public string path;
        };

        public int errno;
        public List<Info> info;
        public long request_id;
    }

    public class UBI {
        public class UBIData {
            public List<string> displayname;
        }

        public ErrInfo errInfo;
        public UBIData data;
    };

    public class UserInfo {
        public class UserInfoData {
            public string uname;
            public string intro;
            public string avatar_url;
            public int follow_count;
            public int fans_count;
            public ulong uk;
            public int album_count;
            public int pubshare_count;
            public int tui_user_count;
            public int c2c_user_sell_count;
            public int c2c_user_buy_count;
            public int c2c_user_product_count;
            public int pair_follow_type;
        };

        public int errno;
        public ulong request_id;
        public UserInfoData user_info;
    };

    public class VerifyInfo {
        public class VerifyInfoData {
            public string hao123Param;
            public string mail;
            public string phoneNumber;
            public string u;
            public string userName;
        };

        public ErrInfo errInfo;
        public VerifyInfoData data;
    };

    public class YunFile {
        public class Thumbs {
            public string icon;
            public string url3;
            public string url2;
            public string url1;
        };

        public int category;
        public int unlist;
        public ulong fs_id;
        public int dir_empty;
        public long oper_id;
        public long server_mtime;
        public long local_mtime;
        public long server_ctime;
        public long local_ctime;
        public ulong size;
        public int isdir;
        public string path;
        public string server_filename;
        public int empty;

        public string md5;
        public Thumbs thumbs;
    };

    public class YunFileList {
        public int errno;
        public string guid_info;
        public List<YunFile> list;
        public ulong request_id;
        public ulong guid;
    };
}
