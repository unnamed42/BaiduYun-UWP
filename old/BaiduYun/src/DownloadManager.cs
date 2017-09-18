using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.Web;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;


namespace BaiduYun {
    public class DownloadTask : INotifyPropertyChanged {

        private DownloadOperation op;
        private CancellationTokenSource cts;
        private double percent;
        private ulong speed;
        private ulong lastBytes;
        private uint timeElapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        public DownloadTask(DownloadOperation op) {
            this.op = op;
            timeElapsed = 0;
            cts = new CancellationTokenSource();
            UpdateValues();
        }

        ~DownloadTask() {
            cts.Dispose();
        }

        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // This method is called by a timer
        public void UpdateValues() {
            var progress = Op.Progress;
            speed = progress.BytesReceived - lastBytes;
            lastBytes = progress.BytesReceived;
            if (progress.TotalBytesToReceive > 0)
                percent = lastBytes * 100.0 / progress.TotalBytesToReceive;
            else
                percent = 100.0;
            ++timeElapsed;
            OnPropertyChanged("Speed");
            OnPropertyChanged("Percent");
            OnPropertyChanged("TimeElapsed");
        }

        public async Task StartAsync() {
            await op.StartAsync().AsTask(cts.Token);
        }

        public async Task AttachAsync() {
            await op.AttachAsync().AsTask(cts.Token);
        }

        public void Resume() {
            if (op.Progress.Status == BackgroundTransferStatus.PausedByApplication)
                op.Resume();
            OnPropertyChanged("CanPause");
            OnPropertyChanged("CanResume");
        }

        public void Pause() {
            if (op.Progress.Status == BackgroundTransferStatus.Running)
                op.Pause();
            OnPropertyChanged("CanPause");
            OnPropertyChanged("CanResume");
        }

        public void Cancel() {
            cts.Cancel();
            cts.Dispose();
            OnPropertyChanged("CanPause");
            OnPropertyChanged("CanResume");
        }

        public DownloadOperation Op { get { return op; } }

        public string Name { get { return Op.ResultFile.Name; } }

        public double Percent { get { return percent; } }

        public string Speed {
            get {
                var formatted = Utils.FormattedSize(speed);
                if (formatted == "-")
                    formatted = "0 B";
                return formatted + "/s";
            }
        }

        public uint TimeElapsed { get { return timeElapsed; } }

        public bool CanPause { get { return op.Progress.Status == BackgroundTransferStatus.Running; } }

        public bool CanResume {
            get {
                var status = op.Progress.Status;
                return status == BackgroundTransferStatus.PausedByApplication ||
                       status == BackgroundTransferStatus.PausedCostedNetwork ||
                       status == BackgroundTransferStatus.PausedNoNetwork;
            }
        }
    };

    public class DownloadManager {

        private ObservableCollection<DownloadTask> tasks = new ObservableCollection<DownloadTask>();

        public ObservableCollection<DownloadTask> Tasks { get { return tasks; } }
        
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
            foreach (var task in tasks)
                task.Resume();
        }
        
        public void PauseAll() {
            foreach (var task in tasks) 
                task.Pause();
        }

        public void CancelAll() {
            foreach (var task in tasks)
                task.Cancel();
        }

        // Returns a new Task (not started) rather than run the Task in background thread
        private async Task HandleDownloadAsync(DownloadOperation op, bool start = true) {
            var task = new DownloadTask(op);
            tasks.Add(task);
            try {
                await (start ? task.StartAsync() : task.AttachAsync());
            } catch (TaskCanceledException) {
                await op.ResultFile.DeleteAsync();
                System.Diagnostics.Debug.WriteLine(String.Format("Cancelled task {0}", op.ResultFile.Name));
            } finally {
                tasks.Remove(task);
            }
        }

        private Task NewAsyncDownloadTask(string url, IStorageFile location) {
            var downloader = new BackgroundDownloader() {
                SuccessToastNotification = UWPUtils.SimpleToastNotification("下载已完成：" + location.Name),
            };
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("Empty download url");
            var download = downloader.CreateDownload(new Uri(url), location);
            return HandleDownloadAsync(download);
        }

        private async Task<List<Task>> NewAsyncDownloadTasks(IEnumerable<YunFile> files, IStorageFolder location) {
            var urllist = await BaiduYun.GetFileDownloadLink(files);
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
            foreach (var file in await BaiduYun.GetFileList(folder.path)) {
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
                var url = await BaiduYun.GetFileDownloadLink(new[] { file });
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
    }
}
