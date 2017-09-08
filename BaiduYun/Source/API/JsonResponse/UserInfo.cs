namespace BaiduYun.API.JsonResponse {
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
}
