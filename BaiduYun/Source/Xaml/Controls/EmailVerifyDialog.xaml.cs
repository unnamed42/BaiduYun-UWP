using System;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using BaiduYun.Net.API;

namespace BaiduYun.Xaml.Controls {

    public sealed partial class EmailVerifyDialog : ContentDialog {

        private DispatcherTimer timer;
        private int clock;
        private string authtoken;

        public EmailVerifyDialog(string authtoken) {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => {
                if (--clock < 0) {
                    SendButton.Visibility = Visibility.Visible;
                    Countdown.Visibility = Visibility.Collapsed;
                    timer.Stop();
                } else {
                    Countdown.Text = String.Format("{0}s", clock);
                }
            };

            this.authtoken = authtoken;

            this.InitializeComponent();
        }

        private async void SendVCode(object sender, RoutedEventArgs e) {
            clock = 60;
            if (!await Auth.SendEmailVCode(authtoken))
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
                return await Auth.VerifyEmailVCode(authtoken, VCodeBox.Text);
            } else
                return false;
        }
    }
}
