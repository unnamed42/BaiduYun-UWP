using System;

using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.System.Threading;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BaiduYun.Pages {

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Download : Page {

        public static DownloadManager Downloads { get { return Globals.downloads; } }

        private static MainPage mainPage = (App.Current as App).MainPage;

        public Download() {
            this.InitializeComponent();
            DownloadView.ItemsSource = Downloads.Tasks;
            ThreadPoolTimer.CreatePeriodicTimer((source) => {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    foreach (var item in Downloads.Tasks) {
                        System.Diagnostics.Debug.WriteLine(String.Format("{0}-{1}-{2}", item.Name, item.Percent, item.Speed));
                        item.UpdateValues();
                    }
                });
            }, TimeSpan.FromMilliseconds(1000));
        }

        private void ShowActions(object sender, PointerRoutedEventArgs e) {
            var panel = (sender as FrameworkElement).FindName("ActionContainer") as FrameworkElement;
            panel.Visibility = Visibility.Visible;
        }

        private void HideActions(object sender, PointerRoutedEventArgs e) {
            var panel = (sender as FrameworkElement).FindName("ActionContainer") as FrameworkElement;
            panel.Visibility = Visibility.Collapsed;
        }

        private void ResumeTask(object sender, RoutedEventArgs e) {
            var task = (sender as FrameworkElement).DataContext as DownloadTask;
            task.Resume();
        }

        private void PauseTask(object sender, RoutedEventArgs e) {
            var task = (sender as FrameworkElement).DataContext as DownloadTask;
            task.Pause();
        }

        private void CancelTask(object sender, RoutedEventArgs e) {
            var task = (sender as FrameworkElement).DataContext as DownloadTask;
            task.Cancel();
        }
    }
}
