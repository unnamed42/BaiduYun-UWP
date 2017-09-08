using System;
using System.Linq;

using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

using BaiduYun.UWP;
using BaiduYun.Models;
using BaiduYun.Global;

namespace BaiduYun {
    
    public sealed partial class MainPage : Page {

        public MainPage() {
            Globals.MainFrame = MainFrame;
            this.InitializeComponent();

            var SetBackButton = new Action<Frame>((frame) => {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    frame.CanGoBack ?
                    AppViewBackButtonVisibility.Visible :
                    AppViewBackButtonVisibility.Collapsed;
            });

            Login.LoginSuccess += async (sender, e) => {
                if (!await vm.UpdateUserInfo())
                    await Dialogs.Popup("错误", "无法获取用户信息");
                else
                    MainFrame.Navigate(typeof(Pages.Files));
            };

            NavMenuToggle.Click += (sender, e) => {
                NavMenuContainer.IsPaneOpen = !NavMenuContainer.IsPaneOpen;
            };

            MainFrame.Navigating += (sender, e) => {
                if (e.SourcePageType == MainFrame.SourcePageType) {
                    e.Cancel = true;
                    return;
                }
            };

            MainFrame.Navigated += (sender, e) => {
                SetBackButton(sender as Frame);

                foreach (var item in NavMenu.Items.Cast<NavButtonData>()) {
                    if (item.Page == MainFrame.SourcePageType) {
                        NavMenu.SelectedItem = item;
                        break;
                    }
                }
            };
            
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) => {
                if (MainFrame.CanGoBack) {
                    e.Handled = true;
                    MainFrame.GoBack();
                }
            };

            SetBackButton(MainFrame);
        }

        private void ShowAvatarFlyout(object sender, RightTappedRoutedEventArgs e) {
            var element = (FrameworkElement)sender;
            var flyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(element);
            flyout.ShowAt(element, e.GetPosition(element));
        }

        private void PageNavigate(object sender, SelectionChangedEventArgs e) {
            MainFrame.Navigate(((sender as Selector).SelectedItem as NavButtonData).Page);
        }
    }
}
