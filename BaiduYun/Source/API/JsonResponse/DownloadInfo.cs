using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {
    public class DownloadInfo {
        public string host { get; set; }
        public string client_ip { get; set; }
        public List<string> server { get; set; }
        public string path { get; set; }
        public long request_id { get; set; }
    }
}
