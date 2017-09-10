using Windows.UI.Xaml.Controls;

using BaiduYun.Xaml.Input;

namespace BaiduYun.Pages {

    public sealed partial class Files : Page {

        public Files() {
            this.InitializeComponent();

            this.Loaded += async (sender, e) => {
                if (!await vm.Init())
                    await Dialogs.Popup("错误", "无法获取用户信息");
            };
        }
    }
}
