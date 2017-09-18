using System;
using System.IO;
using System.Threading.Tasks;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Notifications;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;

using Microsoft.Toolkit.Uwp.Notifications;

namespace BaiduYun {

    public static partial class UWPUtils {

        public static async Task<bool> SimpleDialog(string content) {
            var grid = new Grid();
            grid.Children.Add(new TextBlock() {
                Text = content,
                FontSize = 25,
                VerticalAlignment = VerticalAlignment.Center,
            });
            var dialog = new ContentDialog() {
                Content = grid,
                PrimaryButtonText = "确认",
            };
            return await dialog.ShowAsync() == ContentDialogResult.Primary;
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
            if(player.MediaPlayer.PlaybackSession.CanPause)
                player.MediaPlayer.Pause();
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

        public static async Task<BitmapImage> ImageFromBuffer(IBuffer buffer) {
            var image = new BitmapImage();
            using(var stream = new InMemoryRandomAccessStream()) {
                stream.Seek(0);
                await stream.WriteAsync(buffer);
                await image.SetSourceAsync(stream);
            }
            return image;
        }

        public static async Task<IStorageFile> SavePathPicker(string name = "") {
            var picker = new FileSavePicker() {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = name,
            };
            var ext = Path.GetExtension(name);
            if (!String.IsNullOrWhiteSpace(ext))
                picker.FileTypeChoices.Add("File", new[] { ext });
            return await picker.PickSaveFileAsync();
        }

        public static async Task<IStorageFolder> SaveFolderPicker() {
            var picker = new FolderPicker() {
                SuggestedStartLocation = PickerLocationId.Downloads,
            };
            picker.FileTypeFilter.Add("*");
            return await picker.PickSingleFolderAsync();
        }

        public static ToastNotification SimpleToastNotification(string content, string title = "Baidu Yun") {
            var toast = new ToastContent() {
                Launch = "app-defined-string",
                Visual = new ToastVisual() {
                    BindingGeneric = new ToastBindingGeneric() {
                        Children = {
                            new AdaptiveText() {
                                Text = title,
                            }, new AdaptiveText() {
                                Text = content,
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom() {
                    Buttons = {
                        new ToastButton("OK", "name")
                    }
                }
            };
            return new ToastNotification(toast.GetXml());
        }
        //public static async Task<string> GetAvatarPath(User u) {
        //    var cacheRoot = ApplicationData.Current.LocalCacheFolder;
        //    var userCacheRoot = await cacheRoot.CreateFolderAsync(u.UserName, CreationCollisionOption.OpenIfExists);
        //    var avatar = await userCacheRoot.TryGetItemAsync("avatar.jpg");
        //    if(avatar == null) {
        //        var client = Globals.client;
        //        var response = await client.GetAsync(new Uri(u.AvatarUrl));
        //        if(response.IsSuccessStatusCode) {
        //            var buffer = await response.Content.ReadAsBufferAsync();
        //            var file = await userCacheRoot.CreateFileAsync("avatar.jpg", CreationCollisionOption.ReplaceExisting);
        //            await FileIO.WriteBufferAsync(file, buffer);
        //            return file.Path;
        //        }
        //        return null;
        //    }
        //    return avatar.Path;
        //}

    };
}