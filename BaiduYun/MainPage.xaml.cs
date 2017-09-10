using System;
using System.Linq;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

using BaiduYun.Models;
using BaiduYun.Global;
using BaiduYun.Xaml.Input;

namespace BaiduYun {
    
    public sealed partial class MainPage : Page {

        public MainPage() {
            Globals.MainFrame = MainFrame;
            this.InitializeComponent();

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
                e.Cancel = e.SourcePageType == MainFrame.SourcePageType;
            };
            
            MainFrame.Navigated += (sender, e) => {
                foreach (var item in NavMenu.Items.Cast<NavButtonData>()) {
                    if (item.Page == MainFrame.SourcePageType) {
                        NavMenu.SelectedItem = item;
                        break;
                    }
                }
            };
        }

        private void PageNavigate(object sender, SelectionChangedEventArgs e) {
            MainFrame.Navigate(((sender as Selector).SelectedItem as NavButtonData).Page);
        }
    }
}
