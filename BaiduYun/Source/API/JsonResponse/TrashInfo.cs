using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {
    public class TrashInfo {
        public class Info {
            public int errno { get; set; }
            public string path { get; set; }
        };

        public int errno { get; set; }
        public List<Info> info { get; set; }
        public long request_id { get; set; }
    }
}
