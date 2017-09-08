using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {
    public class UBI {
        public class UBIData {
            public List<string> displayname { get; set; }
        }

        public ErrInfo errInfo { get; set; }
        public UBIData data { get; set; }
    };
}
