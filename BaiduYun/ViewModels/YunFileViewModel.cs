using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.UI.Xaml.Controls;

using BaiduYun.API;
using BaiduYun.UWP;
using BaiduYun.Misc;
using BaiduYun.Global;
using BaiduYun.Extensions;
using BaiduYun.API.JsonResponse;

namespace BaiduYun.ViewModels {

    public enum FileType {
        Folder, Audio, Video, Image, Archive, Text, Code, Executable, Unknown
    };

    public class YunFileViewModel {

        private YunFile file;
        
        private ICommand trash;
        private ICommand rename;
        private ICommand share;
        private ICommand download;

        public YunFileViewModel(YunFile file) {
            this.file = file;
            trash = new DelegateCommand<ListViewBase>(ExecuteTrash);
            rename = new DelegateCommand<ListViewBase>(ExecuteRename);
            share = new DelegateCommand<ListViewBase>(ExecuteShare);
            download = new DelegateCommand<ListViewBase>(ExecuteDownload);
        }

        public string ModifiedAt {
            get { return Utils.SanitizeTimeStamp(file.server_mtime); }
        }

        public string FileSize {
            get { return Utils.SanitizeSize(file.size); }
        }

        public string FileName {
            get { return file.server_filename; }
        }

        public string FileIcon {
            get { return (string)App.Current.Resources[file.isdir != 0 ? "FolderIcon" : "FileIcon"]; }
        }
        
        public FileType Type {
            get {
                if (file.isdir == 1)
                    return FileType.Folder;
                switch (Path.GetExtension(file.path.ToLower())) {
                    case ".jpg": case ".png": case ".bmp":
                        return FileType.Image;
                    case ".zip": case ".rar": case ".7z":
                    case ".iso":
                        return FileType.Archive;
                    case ".c": case ".cpp": case ".h":
                    case ".hpp": case ".html": case ".js":
                    case ".cs": case ".css": case ".py":
                    case ".java": case ".sh":
                        return FileType.Code;
                    case ".mp3": case ".wmv": case ".wav":
                        return FileType.Audio;
                    case ".mp4": case ".mkv": case ".avi":
                    case ".rm": case ".rmvb": case ".flv":
                    case ".ogg":
                        return FileType.Video;
                    case ".txt":
                        return FileType.Text;
                    default:
                        return FileType.Unknown;
                }
            }
        }

        public YunFile File {
            get { return file; }
        }
        
        public ICommand TrashCommand {
            get { return trash; }
        }

        public ICommand RenameCommand {
            get { return rename; }
        }

        public ICommand ShareCommand {
            get { return share; }
        }

        public ICommand DownloadCommand {
            get { return download; }
        }

        public ObservableCollection<YunFileViewModel> Children { get; set; } = null;

        private IEnumerable<YunFile> ThisOrSelected(ListViewBase list) {
            var selected = list.SelectedItems;
            return selected.Count == 0 ?
                   (IEnumerable<YunFile>)new[] { this.file } :
                   selected.Select(item => (item as YunFileViewModel).File).ToList();
        }

        private async void ExecuteTrash(ListViewBase list) {
            var remove = ThisOrSelected(list);
            if(await PCS.MoveToTrash(Globals.bdstoken, remove)) {
                (list.ItemsSource as ObservableCollection<YunFileViewModel>).RemoveAll(item => {
                    return remove.Contains(item.file);
                });
            }
        }

        private async void ExecuteRename(ListViewBase list) {
            var newname = await Dialogs.InputDialog("重命名为：", file.server_filename);
            if (!string.IsNullOrEmpty(newname) && await PCS.RenameFile(Globals.bdstoken, file.path, newname)) {
                var oldpath = file.path;
                var newpath = oldpath.Replace(file.server_filename, newname);
                file.server_filename = newname;
                file.path = newpath;
                if (file.isdir == 1)
                    this.Children = null;
                var items = list.ItemsSource;
                list.ItemsSource = null;
                list.Items.Clear();
                list.ItemsSource = items;
            }
        }

        private async void ExecuteShare(ListViewBase list) {
            var password = await Dialogs.InputDialog("设置分享密码", "", "空值为无密码");
            if (await PCS.CreateShare(Globals.bdstoken, ThisOrSelected(list), password))
                list.SelectedItems.Clear();
        }

        private async void ExecuteDownload(ListViewBase list) {

        }

    }
}
