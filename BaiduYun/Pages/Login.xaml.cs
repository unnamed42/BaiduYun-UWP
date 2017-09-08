using System;
using System.Linq;
using System.Collections.Generic;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BaiduYun.Pages {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Login : Page { 

        private bool Logging {
            set {
                LoggingProgress.IsActive = value;
                LoggingStepText.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private string LoggingStep { set { LoggingStepText.Text = value; } }
        private string ErrorReason { set { ErrorText.Text = value; } }

        private MainPage mainPage;
        private List<string> names;

        public Login() {
            this.InitializeComponent();

            mainPage = (App.Current as App).MainPage;
            names = LocalUtils.AllUserNames()?.ToList() ?? new List<string>();
            NameBox.TextChanged += (sender, e) => {
                if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                    var txt = sender.Text;
                    sender.ItemsSource = names.Where(item => item.StartsWith(txt)).ToList();
                }
                LoginButton.IsEnabled = !String.IsNullOrWhiteSpace(NameBox.Text) && !String.IsNullOrEmpty(PasswordBox.Password);
            };
            NameBox.SuggestionChosen += (sender, e) => {
                var name = e.SelectedItem as string;
                User u;
                if (LocalUtils.TryGetUser(name, out u))
                    SetUser(u);
            };

            PasswordBox.PasswordChanged += (sender, e) => {
                LoginButton.IsEnabled = !String.IsNullOrWhiteSpace(NameBox.Text) && !String.IsNullOrEmpty(PasswordBox.Password);
            };
        }

        private void SetUser(User u) {
            NameBox.Text = u.UserName;
            if (u.SavePassword)
                PasswordBox.Password = LocalUtils.GetPassword(u.UserName);
            SavePassword.IsChecked = u.SavePassword;
            AutoLogin.IsChecked = u.AutoLogin;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            var user = (e.Parameter as User) ?? LocalUtils.CurrentUser();
            if (user == null)
                return;
            SetUser(user);
            LoginButton.IsEnabled = !String.IsNullOrWhiteSpace(NameBox.Text) && !String.IsNullOrEmpty(PasswordBox.Password);
        }

        private async void LoginButtonClicked(object sender, RoutedEventArgs e) {
            var mainPage = (App.Current as App).MainPage;

            string username = NameBox.Text, password = PasswordBox.Password;

            Logging = true;
            LoggingStep = "测试网络连接……";
            ErrorReason = "";

            if (!await BaiduYun.Init()) {
                Logging = false;
                ErrorReason = "无法与百度获取联系";
                return;
            }

            var gid = Utils.GIDGenerator();

            LoggingStep = "获取登录 token……";

            var token = await BaiduYun.GetToken(gid);
            if (token == null) {
                Logging = false;
                return;
            }

            LoggingStep = "获取 RSA 密钥……";

            var rsa = await BaiduYun.GetRSA(token);
            if (rsa == null || rsa.errno != 0) {
                Logging = false;
                ErrorReason = "获取 RSA 密钥失败";
                return;
            }

            //LoggingStep = "获取账户状态……";

            //var status = await BaiduYun.CheckAccountState(token, username);
            //if (status == null || status.errInfo.no != 0) {
            //    Logging = false;
            //    ErrorReason = "账户异常" + (status == null ? "" : String.Format("-异常代码：{0}", status.errInfo.no));
            //    return;
            //}

            string codestring = null, vcodetype = null, verifycode = null;

            LoggingStep = "正在登录……";

            for (;;) {
                if (!String.IsNullOrWhiteSpace(codestring)) {
                    var dialog = new Pages.CaptchaDialog(token, codestring, vcodetype);
                    Tuple<string, string> tuple = await dialog.CaptchaAsync();
                    verifycode = tuple.Item1; codestring = tuple.Item2;
                    if (String.IsNullOrWhiteSpace(verifycode)) {
                        Logging = false;
                        ErrorReason = "您中止了验证码输入";
                        return;
                    }
                    if(!await BaiduYun.CheckCaptcha(token, codestring, verifycode)) {
                        Logging = false;
                        ErrorReason = "验证码错误";
                        return;
                    }
                }
                var result = await BaiduYun.Login(token, rsa, username, password, codestring, verifycode, gid);
                if (String.IsNullOrEmpty(result)) {
                    Logging = false;
                    ErrorReason = "网络异常，无法继续登录";
                    return;
                }
                var query = Utils.ParseQueryString(result);
                int error = Int32.Parse(query["err_no"]);

                switch (error) {
                    case Errors.OK:
                        goto OK;
                    case Errors.CAPTCHA_REQUIRED: case Errors.CAPTCHA:
                        codestring = query["codeString"];
                        vcodetype = query["vcodetype"];
                        continue;
                    case Errors.EXTERNAL_VERIFICATION_REQUIRED:
                        var auth_token = query["authtoken"];
                        var lstr = query["lstr"];
                        var ltoken = query["ltoken"];
                        var dialog = new Pages.EmailVerifyDialog(auth_token);
                        if (!await dialog.VCodeAsync()) {
                            Logging = false;
                            ErrorReason = "邮箱验证失败";
                            return;
                        }
                        if (!await BaiduYun.ReVerifyLogin(lstr, ltoken)) {
                            Logging = false;
                            ErrorReason = "登录失败";
                            return;
                        }
                        goto OK;
                    default:
                        Logging = false;
                        ErrorReason = String.Format("登录异常，错误码{0}", error);
                        return;
                }
            }

            OK:
            LoggingStep = "正在获取 BDSToken……";

            var bdstoken = await BaiduYun.GetBDSToken();
            var uk = await BaiduYun.GetUserUK();

            if(bdstoken == null || uk == null) {
                Logging = false;
                ErrorReason = "网络异常";
                return;
            }

            LoggingStep = "正在获取用户名……";

            var realun = await BaiduYun.GetUserName(); // real username
            if (realun == null) {
                Logging = false;
                ErrorReason = "网络异常";
                return;
            }

            var json = Globals.everything;

            var user = await BaiduYun.GetUserInfo(uk);
            if (user == null || user.errno != 0) {
                Logging = false;
                ErrorReason = "无法获取账户信息";
                return;
            }

            var userinfo = user.user_info;
            
            json.SetNamedValue("currentUser", JsonValue.CreateStringValue(userinfo.uname));
            var u = new User() {
                UserName = realun,
                AvatarUrl = userinfo.avatar_url,
                UK = uk,
                SavePassword = SavePassword.IsChecked ?? false,
                AutoLogin = (SavePassword.IsChecked ?? false) && (AutoLogin.IsChecked ?? false),
            };

            LocalUtils.AddUser(u, password);
            Globals.bdstoken = bdstoken;

            Logging = false;
            mainPage.SetAsOnline(u);
            mainPage.RootFrame.Navigate(typeof(Files), u);
            mainPage.RootFrame.BackStack.Clear();
        }
    }
}
