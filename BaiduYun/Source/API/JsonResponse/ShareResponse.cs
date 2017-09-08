namespace BaiduYun.API.JsonResponse {
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
