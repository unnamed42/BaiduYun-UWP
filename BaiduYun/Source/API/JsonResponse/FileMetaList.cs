using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {
    public class FileMetaList {
        public int errno { get; set; }
        public List<FileMeta> info { get; set; }
        public ulong request_id { get; set; }
    };
}
