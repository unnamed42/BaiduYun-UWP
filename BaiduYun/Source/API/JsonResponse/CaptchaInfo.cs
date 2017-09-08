namespace BaiduYun.API.JsonResponse {
    public class CaptchaInfo {
        public class ErrInfo {
            public string no { get; set; }
            public string msg { get; set; }
        };

        public ErrInfo errInfo { get; set; }
        public object data { get; set; }
    };
}
