namespace BaiduYun.API.JsonResponse {
    public class RefreshCaptcha {
        public class RefreshCaptchaData {
            public string verifyStr { get; set; }
            public string verifySign { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public RefreshCaptchaData data { get; set; }
    };
}
