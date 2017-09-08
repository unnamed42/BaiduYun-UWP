namespace BaiduYun.API.JsonResponse {
    public class FileMeta {
        public int extent_tinyint4 { get; set; }
        public int extent_tinyint3 { get; set; }
        public int extent_tinyint2 { get; set; }
        public int extent_tinyint1 { get; set; }
        public int extent_int3 { get; set; }
        public int category { get; set; }
        public ulong fs_id { get; set; }
        public int isdir { get; set; }
        public int ifhassubdir { get; set; }
        public int errno { get; set; }
        public long server_mtime { get; set; }
        public long server_ctime { get; set; }
        public long local_mtime { get; set; }
        public long local_ctime { get; set; }
        public ulong size { get; set; }
        public string path { get; set; }
        public string server_filename { get; set; }

        public string dlink { get; set; }
    };
}
