using System;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Controls;

using BaiduYun.Misc;
using BaiduYun.Models;
using BaiduYun.Global;
using BaiduYun.Net;
using BaiduYun.Net.API;
using BaiduYun.Xaml.Animations;

namespace BaiduYun.Xaml.Controls {
    
    public sealed partial class Login : UserControl {

        private IEnumerable<string> names;

        public Login() {
            this.InitializeComponent();
            this.Loaded += async (sender, e) => {
                if (Globals.Accounts.ActiveUser != null && WebUtils.HasCookie("BDUSS")) {
                    NotLogged = false;
                    Globals.bdstoken = await PCS.GetBDSToken();
                    Globals.uk = await PCS.GetUserUK();
                    LoginSuccess?.Invoke(this, new EventArgs());
                } else
                    this.ClearState();
            };

            username.TextChanged += (sender, e) => {
                if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                    sender.ItemsSource = names.Where((item) => {
                        return item.StartsWith(sender.Text);
                    });
                    remember.IsChecked = false;
                    password.Password = "";
                }
            };

            username.SuggestionChosen += (sender, e) => {
                var username = e.SelectedItem as string;
                password.Password = Globals.Accounts[username].password;
                remember.IsChecked = true;
            };
        }

        /// <summary>
        /// Set control to initial state
        /// </summary>
        public void ClearState() {
            Fade.StartAnimation(backdrop, this, FADE_DELAY);

            // get username and password of last logged user
            var accounts = Globals.Accounts;
            var active = accounts.ActiveUser;
            
            names = accounts.UserNames;
            this.username.Text = active?.username ?? "";
            this.username.ItemsSource = names;
            this.password.Password = active?.password ?? "";
            this.remember.IsChecked = !String.IsNullOrEmpty(active?.password);
            NotLogged = true;
        }

        private async void OnLoginClicked(object sender, RoutedEventArgs e) {
            if (await DoLogin()) {
                NotLogged = false;

                Fade.StartAnimation(backdrop, this, FADE_DELAY, false);

                var accounts = Globals.Accounts;
                accounts.Active = UserName;
                if (RememberMe || accounts[UserName] != null) {
                    accounts[UserName] = new User(UserName, Password, Globals.uk);
                }

                LoginSuccess?.Invoke(this, new EventArgs());
            }
        }

        private async Task<bool> DoLogin() {
            Logging = true;
            Status = "测试网络连接……";

            if (!await Auth.Init()) {
                Logging = false;
                Status = "无法与百度获取联系";
                return false;
            }

            var gid = Utils.GIDGenerator();

            Status = "获取登录 token……";
            var token = await Auth.GetToken(gid);
            if (token == null) {
                Logging = false;
                Status = "获取登录 token 失败";
                return false;
            }

            Status = "获取 RSA 密钥……";
            var rsa = await Auth.GetRSA(token);
            if (rsa == null || rsa.errno != 0) {
                Logging = false;
                Status = "获取 RSA 密钥失败";
                return false;
            }

            string codestring = null, vcodetype = null, verifycode = null;

            Status = "正在登录……";
            for (;;) {
                if (!String.IsNullOrWhiteSpace(codestring)) {
                    var dialog = new CaptchaDialog(token, codestring, vcodetype);
                    var clicked = await dialog.ShowAsync();
                    verifycode = dialog.Captcha;
                    codestring = dialog.CodeString;
                    if (clicked != ContentDialogResult.Primary || String.IsNullOrWhiteSpace(verifycode)) {
                        Logging = false;
                        Status = "您中止了验证码输入";
                        return false;
                    }
                    if (!await Auth.CheckCaptcha(token, codestring, verifycode)) {
                        Logging = false;
                        Status = "验证码错误";
                        return false;
                    }
                }
                var result = await Auth.Login(token, rsa, UserName, Password, codestring, verifycode, gid);
                if (String.IsNullOrEmpty(result)) {
                    Logging = false;
                    Status = "网络异常，无法继续登录";
                    return false;
                }

                var query = WebUtils.ParseQueryString(result);
                var error = Int32.Parse(query["err_no"]);

                switch (error) {
                    case Errors.OK:
                        goto OK;
                    case Errors.CAPTCHA_REQUIRED:
                    case Errors.CAPTCHA:
                        codestring = query["codeString"];
                        vcodetype = query["vcodetype"];
                        continue;
                    case Errors.EXTERNAL_VERIFICATION_REQUIRED:
                        var auth_token = query["authtoken"];
                        var lstr = query["lstr"];
                        var ltoken = query["ltoken"];
                        var dialog = new EmailVerifyDialog(auth_token);
                        if (!await dialog.VCodeAsync()) {
                            Logging = false;
                            Status = "邮箱验证失败";
                            return false;
                        }
                        if (!await Auth.ReVerifyLogin(lstr, ltoken)) {
                            Logging = false;
                            Status = "登录失败";
                            return false;
                        }
                        goto OK;
                    default:
                        Logging = false;
                        Status = String.Format("登录异常，错误码{0}", error);
                        return false;
                }
            }
OK:
            Status = "正在获取 BDSToken……";

            Globals.bdstoken = await PCS.GetBDSToken();
            Globals.uk = await PCS.GetUserUK();

            if (Globals.bdstoken == null || Globals.uk == null) {
                Logging = false;
                Status = "网络异常";
                return false;
            }
            
            Logging = false;
            return true;
        }
    }

    // Describing attached properties
    [ContentProperty(Name = nameof(MainContent))]
    public sealed partial class Login : INotifyPropertyChanged {

        private const int FADE_DELAY = 200;

        private string status;
        private bool logging;
        private bool notLogged;

        public event EventHandler LoginSuccess;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropChanged([CallerMemberName] string propname = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private void SetProperty<T>(ref T src, T val, [CallerMemberName] string propname = "") {
            src = val;
            OnPropChanged(propname);
        }

        /// <summary>
        /// Username from user's input
        /// </summary>
        public string UserName {
            get { return username.Text; }
        }

        /// <summary>
        /// Password from user's input
        /// </summary>
        public string Password {
            get { return password.Password; }
        }

        /// <summary>
        /// Save password when true
        /// </summary>
        public bool RememberMe {
            get { return remember.IsChecked ?? false; }
        }

        public bool NotLogged {
            get { return notLogged; }
            private set { SetProperty(ref notLogged, value); OnPropChanged(nameof(Logged)); }
        }

        public bool Logged {
            get { return !notLogged; }
        }

        public bool Logging {
            get { return logging; }
            private set { SetProperty(ref logging, value); }
        }

        public string Status {
            get { return status; }
            private set { SetProperty(ref status, value); }
        }

        /// <summary>
        /// Main content
        /// </summary>
        public object MainContent {
            get { return GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(nameof(MainContent), typeof(object), typeof(Login), null);
    };
}