using System.Collections.Generic;

namespace BaiduYun.API.JsonResponse {
    public class SharedFile {
        public long shareId { get; set; }
        public List<string> fsIds { get; set; }
        public int channel { get; set; }
        public List<object> channelInfo { get; set; }
        public int status { get; set; }
        public int expiredType { get; set; }
        public string passwd { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int ctime { get; set; }
        public int appId { get; set; }
        public int @public { get; set; }
        public int publicChannel { get; set; }
        public int tplId { get; set; }
        public string shorturl { get; set; }
        public long ip { get; set; }
        public int tag { get; set; }
        public string shareinfo { get; set; }
        public int bitmap { get; set; }
        public int port { get; set; }
        public object dtime { get; set; }
        public int vCnt { get; set; }
        public int dCnt { get; set; }
        public int tCnt { get; set; }
        public string shortlink { get; set; }
        public int typicalCategory { get; set; }
        public string typicalPath { get; set; }
    };
}
