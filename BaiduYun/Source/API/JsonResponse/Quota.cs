namespace BaiduYun.API.JsonResponse {
    public class Quota {
        public int errno { get; set; }
        public ulong total { get; set; }
        public int is_show_window { get; set; }
        public ulong request_id { get; set; }
        public ulong used { get; set; }
    };
}
