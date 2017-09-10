using System;
using System.Threading.Tasks;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Core;

namespace BaiduYun.Xaml.Input {

    public static class Dialogs {

        public static async Task Popup(string title, string content) {
            var grid = new Grid();
            grid.Children.Add(new TextBlock() {
                Text = content,
                FontSize = 25,
                VerticalAlignment = VerticalAlignment.Center,
            });
            var dialog = new ContentDialog() {
                Content = grid,
                Title = title,
                PrimaryButtonText = "确认",
                IsSecondaryButtonEnabled = false
            };
            await dialog.ShowAsync();
        }

        public static async Task<string> InputDialog(string title, string preset = "", string placeholder = "") {
            var textbox = new TextBox() {
                Height = 32,
                AcceptsReturn = false,
                Text = preset,
                PlaceholderText = placeholder
            };
            var dialog = new ContentDialog() {
                Content = textbox,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "确认",
                SecondaryButtonText = "取消"
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return textbox.Text;
            return null;
        }

        public static async Task ImageDialog(string title, string url) {
            var grid = new Grid();
            grid.Children.Add(new Image() {
                Source = new BitmapImage(new Uri(url)),
            });

            await new ContentDialog() {
                PrimaryButtonText = "确认",
                Content = grid,
                Title = title,
            }.ShowAsync();
        }

        public static async Task MediaDialog(string title, string url) {
            var player = new MediaPlayerElement() {
                Background = new SolidColorBrush(Colors.Transparent),
                Source = MediaSource.CreateFromUri(new Uri(url)),
                AutoPlay = false,
                AreTransportControlsEnabled = true,
            };
            var dialog = new ContentDialog() {
                PrimaryButtonText = "确认",
                Content = player,
                Title = title,
            };
            await dialog.ShowAsync();
            if (player.MediaPlayer.PlaybackSession.CanPause)
                player.MediaPlayer.Pause();
        }
    }
}