using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {

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
}
