using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {
    public class YunFileList {
        public int errno { get; set; }
        public string guid_info { get; set; }
        public List<YunFile> list { get; set; }
        public ulong request_id { get; set; }
        public ulong guid { get; set; }
    };
}
