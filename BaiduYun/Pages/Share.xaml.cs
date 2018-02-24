using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BaiduYun.Pages {

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Share : Page {

        private List<SharedFileAdapter> shares;
        public static bool ListRefreshed = false;

        public Share() {
            this.InitializeComponent();

            SelectAllBox.Checked += (sender, e) => {
                ShareList.SelectAll();
            };
            SelectAllBox.Unchecked += (sender, e) => {
                ShareList.SelectedItems.Clear();
            };
            SelectAllBox.Click += (sender, e) => {
                if (!SelectAllBox.IsChecked.HasValue)
                    SelectAllBox.IsChecked = false;
            };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (ListRefreshed)
                return;
            await RefreshList();
            ListRefreshed = true;
        }

        private void UpdateItems(IEnumerable<SharedFile> list) {
            ShareList.ItemsSource = shares = list.Select(item => new SharedFileAdapter(item)).ToList();
        }

        private async Task RefreshList() {
            var list = await BaiduYun.SharedFiles();
            if (list != null)
                UpdateItems(list);
        }

        private async void DisableShare(object sender, RoutedEventArgs e) {
            var item = (sender as FrameworkElement).DataContext as SharedFileAdapter;
            var list = ShareList.SelectedItems.Count == 0 ?
                       new List<SharedFile>() { item.File } :
                       ShareList.SelectedItems.Select(file => (file as SharedFileAdapter).File).ToList();
            if (await BaiduYun.DisableShare(list))
                UpdateItems(shares.Select(share => share.File).Except(list));
        }

        private void ShareListSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var total = ShareList.Items.Count;
            var selected = ShareList.SelectedItems.Count;
            if (selected == total)
                SelectAllBox.IsChecked = true;
            else if (selected == 0)
                SelectAllBox.IsChecked = false;
            else
                SelectAllBox.IsChecked = null;
        }

        private void ShowItemFlyout(object sender, RightTappedRoutedEventArgs e) {
            var element = sender as FrameworkElement;
            var flyout = FlyoutBase.GetAttachedFlyout(element) as MenuFlyout;
            flyout.ShowAt(element, e.GetPosition(element));
        }
    }
}
