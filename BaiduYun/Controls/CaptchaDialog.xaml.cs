using System;
using System.Net;
using System.ComponentModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using BaiduYun.API;
using BaiduYun.Global;

namespace BaiduYun.Controls {

    public sealed partial class CaptchaDialog : ContentDialog, INotifyPropertyChanged {
        
        private string token;
        private string codestring;
        private string vcodetype;

        public string Captcha {
            get { return CaptchaBox.Text; }
        }

        public string CaptchaUrl {
            get { return String.Format("{0}cgi-bin/genimage?{1}", Constants.PASSPORT_BASE, WebUtility.UrlEncode(codestring)); }
        }

        public string CodeString {
            get { return codestring; }
            private set { codestring = value; OnPropChanged(nameof(CaptchaUrl)); }
        }

        public CaptchaDialog(string token, string codestring, string vcodetype) {
            this.token = token;
            this.vcodetype = vcodetype;
            CodeString = codestring;
            this.InitializeComponent();
        }

        private async void UpdateCaptcha(object sender, RoutedEventArgs e) {
            var obj = await Auth.RefreshCaptcha(token, vcodetype);
            if (obj?.errInfo.no != 0)
                return;
            CodeString = obj.data.verifyStr;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
