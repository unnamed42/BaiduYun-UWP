using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Windows.UI.Xaml.Controls;

using BaiduYun.Misc;
using BaiduYun.Global;
using BaiduYun.Extensions;
using BaiduYun.Net.API;
using BaiduYun.Net.API.Response;
using BaiduYun.Xaml.Input;

namespace BaiduYun.ViewModels {

    public class FilesViewModel : ViewModelBase {

        private ObservableCollection<YunFileViewModel> items = null;
        private ObservableCollection<YunFileViewModel> path = new ObservableCollection<YunFileViewModel>();
        private Quota quota = null;

        public ObservableCollection<YunFileViewModel> Items {
            get { return items; }
            private set { SetProperty(ref items, value); }
        }

        public ObservableCollection<YunFileViewModel> Path {
            get { return path; }
        }

        private Quota Quota {
            set {
                quota = value;
                OnPropertyChanged(nameof(QuotaValue));
                OnPropertyChanged(nameof(QuotaTooltip));
            }
        }

        public double QuotaValue {
            get { return quota != null ? ((double)quota.used) / quota.total : 100; }
        }

        public string QuotaTooltip {
            get { return quota != null ? String.Format("{0}/{1}", Utils.SanitizeSize(quota.used), Utils.SanitizeSize(quota.total)) : ""; }
        }

        public async Task<bool> Init() {
            var quota = await PCS.GetQuota();
            if (quota == null)
                return false;
            Quota = quota;
            return await ChangeDirectory(new YunFileViewModel(new YunFile() {
                server_filename = "", path = "/", isdir = 1
            }));
        }

        public async Task<bool> ChangeDirectory(YunFileViewModel folder) {
            if (folder.Children == null) {
                var list = await PCS.GetFileList(Globals.bdstoken, folder.File.path);
                if (list == null)
                    return false; 
                folder.Children = list.Select(item => new YunFileViewModel(item)).ToObservable();
            }
            path.Add(folder);
            Items = folder.Children;
            return true;
        }

        public void PathChanged(object sender, ItemClickEventArgs e) {
            var folder = e.ClickedItem as YunFileViewModel;
            int count = path.Count - 1;
            while (count > 0 && !ReferenceEquals(path[count], folder))
                path.RemoveAt(count--);
            Items = folder.Children;
        }

        public async void FileClicked(object sender, ItemClickEventArgs e) {
            (sender as ListView).SelectedItems.Clear();
            var item = e.ClickedItem as YunFileViewModel;
            switch (item.Type) {
                case FileType.Folder:
                    await ChangeDirectory(item);
                    break;
                case FileType.Image:
                    var thumbs = item.File.thumbs;
                    var url = thumbs?.url1 ?? thumbs?.url2 ?? thumbs?.url3;
                    if (url == null)
                        return;
                    await Dialogs.ImageDialog(item.FileName, url);
                    break;
                case FileType.Audio:
                case FileType.Video:
                    var media = await PCS.GetFileDownloadLink(Globals.bdstoken, item.File);
                    if (media == null)
                        return;
                    await Dialogs.MediaDialog(item.FileName, media);
                    break;
                default: break;
            }
        }
    }
}
