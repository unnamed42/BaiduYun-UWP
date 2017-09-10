using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Windows.Web;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;

using BaiduYun.Global;
using BaiduYun.Net.API;
using BaiduYun.Net.API.Response;

namespace BaiduYun.ViewModels {

    public class DownloadViewModel : IDisposable {

        public ObservableCollection<DownloadTaskViewModel> Tasks { get; } = new ObservableCollection<DownloadTaskViewModel>();

        public async Task InitTasks() {
            IReadOnlyList<DownloadOperation> download;
            try {
                download = await BackgroundDownloader.GetCurrentDownloadsAsync();
            } catch (Exception e) {
                if (BackgroundTransferError.GetStatus(e.HResult) == WebErrorStatus.Unknown)
                    throw;
                return;
            }
            if (download.Count > 0)
                await Task.WhenAll(download.Select((item) => HandleDownloadAsync(item, false)));
        }

        public void ResumeAll() {
            foreach (var task in Tasks)
                task.Resume();
        }

        public void PauseAll() {
            foreach (var task in Tasks)
                task.Pause();
        }

        public void CancelAll() {
            foreach (var task in Tasks)
                task.Cancel();
        }

        // Returns a new Task (not started) rather than run the Task in background thread
        private async Task HandleDownloadAsync(DownloadOperation op, bool start = true) {
            var task = new DownloadTaskViewModel(op);
            Tasks.Add(task);
            try {
                await (start ? task.StartAsync() : task.AttachAsync());
            } catch (TaskCanceledException) {
                await op.ResultFile.DeleteAsync();
                System.Diagnostics.Debug.WriteLine(String.Format("Cancelled task {0}", op.ResultFile.Name));
            } finally {
                Tasks.Remove(task);
            }
        }

        private Task NewAsyncDownloadTask(string url, IStorageFile location) {
            var downloader = new BackgroundDownloader() {
                //SuccessToastNotification = UWPUtils.SimpleToastNotification("下载已完成：" + location.Name),
            };
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("Empty download url");
            var download = downloader.CreateDownload(new Uri(url), location);
            return HandleDownloadAsync(download);
        }

        private async Task<List<Task>> NewAsyncDownloadTasks(IEnumerable<YunFile> files, IStorageFolder location) {
            var urllist = await PCS.GetFileDownloadLink(Globals.bdstoken, files);
            var tasks = new List<Task>();
            if (urllist == null)
                return tasks;
            var url = urllist.GetEnumerator();
            foreach (var file in files) {
                if (!url.MoveNext())
                    return tasks;
                var fileitem = await location.CreateFileAsync(file.server_filename, CreationCollisionOption.GenerateUniqueName);
                tasks.Add(NewAsyncDownloadTask(url.Current, fileitem));
            }
            return tasks;
        }

        private async Task<List<Task>> HandleFolder(YunFile folder, IStorageFolder location) {
            var tasks = new List<Task>();
            var files = new List<YunFile>();
            foreach (var file in await PCS.GetFileList(Globals.bdstoken, folder.path)) {
                if (file.isdir == 1) {
                    var subfolder = await location.CreateFolderAsync(file.server_filename, CreationCollisionOption.OpenIfExists);
                    tasks.AddRange(await HandleFolder(file, subfolder));
                } else
                    files.Add(file);
            }
            tasks.AddRange(await NewAsyncDownloadTasks(files, location));
            return tasks;
        }

        public async Task AddTask(YunFile file, IStorageItem location) {
            if (file.isdir == 0) {
                var url = await PCS.GetFileDownloadLink(Globals.bdstoken, new[] { file });
                if (url == null)
                    return;
                await NewAsyncDownloadTask(url[0], location as IStorageFile);
            } else {
                var subfolder = await (location as IStorageFolder).CreateFolderAsync(file.server_filename, CreationCollisionOption.OpenIfExists);
                var tasks = await HandleFolder(file, subfolder);
                await Task.WhenAll(tasks);
            }
        }

        public async Task AddTasks(IEnumerable<YunFile> list, IStorageFolder location) {
            var tasks = new List<Task>();
            var files = new List<YunFile>();
            foreach (var file in list) {
                if (file.isdir == 1) {
                    var subfolder = await location.CreateFolderAsync(file.server_filename, CreationCollisionOption.OpenIfExists);
                    tasks.AddRange(await HandleFolder(file, subfolder));
                } else
                    files.Add(file);
            }
            tasks.AddRange(await NewAsyncDownloadTasks(files, location));
            await Task.WhenAll(tasks);
        }

        public void Dispose() {
            foreach (var task in Tasks)
                task.Dispose();
        }
    }
}
