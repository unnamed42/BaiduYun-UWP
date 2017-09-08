namespace BaiduYun.Models {
    public class LoginData {
        public string username { get; set; }
        public string password { get; set; }
        public bool remember { get; set; }
        public string captcha { get; set; }
        public string verifycode { get; set; } // email verify code
    }
}
