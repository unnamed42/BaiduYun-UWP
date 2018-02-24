using System;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace BaiduYun.Pages {
    public sealed partial class CaptchaDialog : ContentDialog {
        private string token;
        private string codestring;
        private string vcodetype;

        public CaptchaDialog(string token, string codestring, string vcodetype) {
            this.InitializeComponent();
            this.token = token;
            this.codestring = codestring;
            this.vcodetype = vcodetype;
        }

        private async void UpdateCaptcha(string codestring) {
            
            var buffer = await BaiduYun.CaptchaImage(codestring);
            
            CaptchaImage.Source = await UWPUtils.ImageFromBuffer(buffer);
        }
        
        private async void RefreshCaptcha(object sender, RoutedEventArgs args) {
            var obj = await BaiduYun.RefreshCaptcha(token, vcodetype);
            if (obj == null || obj.errInfo.no != 0)
                return;
            codestring = obj.data.verifyStr;
            UpdateCaptcha(codestring);
        }

        public async Task<Tuple<string, string>> CaptchaAsync() {
            UpdateCaptcha(codestring);
            await ShowAsync();
            return Tuple.Create(CaptchaText.Text, codestring);
        }
    }
}
