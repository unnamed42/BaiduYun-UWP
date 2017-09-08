using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {
    public class SharedFileList {
        public int errno { get; set; }
        public long request_id { get; set; }
        public int count { get; set; }
        public int nextpage { get; set; }
        public List<SharedFile> list { get; set; }
    };
}
