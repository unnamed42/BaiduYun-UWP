namespace BaiduYun.API.JsonResponse {
    public class YunFile {
        public class Thumbs {
            public string icon { get; set; }
            public string url3 { get; set; }
            public string url2 { get; set; }
            public string url1 { get; set; }
        };

        public int category { get; set; }
        public int unlist { get; set; }
        public ulong fs_id { get; set; }
        public int dir_empty { get; set; }
        public long oper_id { get; set; }
        public long server_mtime { get; set; }
        public long local_mtime { get; set; }
        public long server_ctime { get; set; }
        public long local_ctime { get; set; }
        public ulong size { get; set; }
        public int isdir { get; set; }
        public string path { get; set; }
        public string server_filename { get; set; }
        public int empty { get; set; }

        public string md5 { get; set; }
        public Thumbs thumbs { get; set; }
    };
}
