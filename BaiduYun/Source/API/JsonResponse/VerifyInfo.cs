namespace BaiduYun.API.JsonResponse {
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
}
