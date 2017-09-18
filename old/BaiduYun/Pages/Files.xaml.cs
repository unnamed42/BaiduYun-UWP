using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.Storage;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BaiduYun.Pages {

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Files : Page {

        private MainPage mainpage = (App.Current as App).MainPage;
        private ObservableCollection<YunFileAdapter> navigator;
        private List<YunFileAdapter> current;
        
        public Files() {
            this.InitializeComponent();
            navigator = new ObservableCollection<YunFileAdapter>();
            BreadcrumbList.ItemsSource = navigator;
            SelectAllBox.Checked += (sender, e) => {
                FileList.SelectAll();
            };
            SelectAllBox.Unchecked += (sender, e) => {
                FileList.SelectedItems.Clear();
            };
            SelectAllBox.Click += (sender, e) => {
                if (!SelectAllBox.IsChecked.HasValue)
                    SelectAllBox.IsChecked = false;
            };
            BrowsePath(new YunFileAdapter(new YunFile() {
                server_filename = "", path = "/", isdir = 1
            }));
            UpdateQuota();
        }

        private async void UpdateQuota() {
            var quota = await BaiduYun.GetQuota();
            if (quota == null)
                return;
            QuotaInfo.Value = ((double)quota.used) / quota.total;
            ToolTipService.SetToolTip(QuotaInfo, new ToolTip() {
                Content = Utils.FormattedSize(quota.used) + '/' + Utils.FormattedSize(quota.total),
            });
        }

        private void UpdateItems(List<YunFileAdapter> list) {
            FileList.ItemsSource = current = list;
            SelectAllBox.IsChecked = false;
        }

        private async void ItemTapped(object sender, TappedRoutedEventArgs e) {
            var item = (sender as FrameworkElement).DataContext as YunFileAdapter;
            switch(item.Type) {
                case FileType.Folder: await BrowsePath(item); break;
                case FileType.Image:
                    var thumbs = item.File.thumbs;
                    var url = thumbs?.url1 ?? thumbs?.url2 ?? thumbs?.url3;
                    if (url == null)
                        return;
                    await UWPUtils.ImageDialog(item.File.server_filename, url);
                    FileList.SelectedItems.Clear();
                    break;
                case FileType.Audio:
                case FileType.Video:
                    var media = await BaiduYun.GetFileDownloadLink(item.File);
                    if (media == null)
                        return;
                    await UWPUtils.MediaDialog(item.File.server_filename, media);
                    FileList.SelectedItems.Clear();
                    break;
                default: break;
            }
        }

        private async Task BrowsePath(YunFileAdapter folder) {
            if (folder.Children == null) {
                var list = await BaiduYun.GetFileList(folder.File.path);
                if (list == null)
                    return;
                folder.Children = list.Select((item) => new YunFileAdapter(item)).ToList();
            }
            navigator.Add(folder);
            UpdateItems(folder.Children);
        }

        private void ToFolder(object sender, ItemClickEventArgs e) {
            var folder = e.ClickedItem as YunFileAdapter;
            int count = navigator.Count - 1;
            while(count > 0 && navigator[count] != folder)
                navigator.RemoveAt(count--);
            if (folder.Children != current)
                UpdateItems(folder.Children);
        }

        private async void MoveToTrash(object sender, RoutedEventArgs e) {
            var item = ((sender as FrameworkElement).DataContext as YunFileAdapter).File;
            var list = FileList.SelectedItems.Count == 0 ?
                       new List<YunFile>() { item } :
                       FileList.SelectedItems.Select((file) => (file as YunFileAdapter).File).ToList();
            if (await BaiduYun.MoveToTrash(list)) {
                // TODO: improve performance of remove action
                foreach (var file in list)
                    current.RemoveAll((adapter) => adapter.File == file);
                FileList.ItemsSource = new List<YunFileAdapter>(current);
            }
        }

        private async void RenameTo(object sender, RoutedEventArgs e) {
            var context = (sender as FrameworkElement).DataContext as YunFileAdapter;
            var file = context.File;
            var newname = await UWPUtils.InputDialog("重命名为：", file.server_filename);
            if (newname != null && await BaiduYun.RenameFile(file.path, newname)) {
                var oldpath = file.path;
                var newpath = oldpath.Replace(file.server_filename, newname);
                file.server_filename = newname;
                file.path = newpath;
                if (file.isdir == 1)
                    context.Children = null;
                UpdateItems(current);
            } 
        }

        private async void CreateShare(object sender, RoutedEventArgs e) {
            var item = (sender as FrameworkElement).DataContext as YunFileAdapter;
            var list = FileList.SelectedItems.Count == 0 ?
                       new List<YunFile>() { item.File } :
                       FileList.SelectedItems.Select(file => (file as YunFileAdapter).File).ToList();
            var password = await UWPUtils.InputDialog("设置分享密码", "", "空值为无密码");
            if (await BaiduYun.CreateShare(list, password)) {
                Share.ListRefreshed = false;
                FileList.SelectedItems.Clear();
            }
        }

        private async void DownloadTo(object sender, RoutedEventArgs e) {
            var item = ((sender as FrameworkElement).DataContext as YunFileAdapter).File;
            if (FileList.SelectedItems.Count == 0) {
                IStorageItem location;
                if (item.isdir == 0)
                    location = await UWPUtils.SavePathPicker(item.server_filename);
                else
                    location = await UWPUtils.SaveFolderPicker();
                if (location == null)
                    return;
                Globals.downloads.AddTask(item, location);
            } else {
                // Make copy because they will be deseleted soon
                var files = FileList.SelectedItems.Select((file) => (file as YunFileAdapter).File).ToList();
                var location = await UWPUtils.SaveFolderPicker();
                if (location == null)
                    return;
                Globals.downloads.AddTasks(files, location);
                FileList.SelectedItems.Clear();
            }
        }

        private void FileListSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var total = FileList.Items.Count;
            var selected = FileList.SelectedItems.Count;
            if (selected == total)
                SelectAllBox.IsChecked = true;
            else if (selected == 0)
                SelectAllBox.IsChecked = false;
            else
                SelectAllBox.IsChecked = null;
        }

        private void ShowItemFlyout(object sender, RightTappedRoutedEventArgs e) {
            var element = sender as FrameworkElement;
            var flyout = Flyout.GetAttachedFlyout(element) as MenuFlyout;
            flyout.ShowAt(element, e.GetPosition(element));
        }
    }
}
