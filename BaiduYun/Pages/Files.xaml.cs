using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

using BaiduYun.UWP;

namespace BaiduYun.Pages {

    public sealed partial class Files : Page {

        public Files() {
            this.InitializeComponent();

            this.Loaded += async (sender, e) => {
                if (!await vm.Init())
                    await Dialogs.Popup("错误", "无法获取用户信息");
            };
        }

        private void ShowItemFlyout(object sender, RightTappedRoutedEventArgs e) {
            var element = (FrameworkElement)sender;
            var flyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(element);
            flyout.ShowAt(element, e.GetPosition(element));
        }
    }
}
