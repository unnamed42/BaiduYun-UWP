namespace BaiduYun.API.JsonResponse {
    public class AccountState {
        public class AccountStateData {
            public string codeString { get; set; }
            public string vcodetype { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public AccountStateData data { get; set; }
    };
}
