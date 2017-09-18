using System;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace BaiduYun.Pages {
    public sealed partial class EmailVerifyDialog : ContentDialog {
        private DispatcherTimer timer;
        private int clock;
        private string authtoken;

        public EmailVerifyDialog(string authtoken) {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += TimerTick;

            this.authtoken = authtoken;
        }

        private void TimerTick(object sender, object e) {
            if(--clock < 0) {
                SendButton.Visibility = Visibility.Visible;
                Countdown.Visibility = Visibility.Collapsed;
                timer.Stop();
            } else {
                Countdown.Text = String.Format("{0}s", clock);
            }
        }

        private async void SendVCode(object sender, RoutedEventArgs e) {
            clock = 60;
            if (!await BaiduYun.SendEmailVCode(authtoken))
                this.Hide();
            SendButton.Visibility = Visibility.Collapsed;
            Countdown.Visibility = Visibility.Visible;
            timer.Start();
        }

        public async Task<bool> VCodeAsync() {
            var ButtonClicked = await ShowAsync();
            if (ButtonClicked == ContentDialogResult.Primary) {
                if (clock <= 0)
                    return false;
                return await BaiduYun.VerifyEmailVCode(authtoken, VCodeBox.Text);
            } else
                return false;
        }
    }
}
