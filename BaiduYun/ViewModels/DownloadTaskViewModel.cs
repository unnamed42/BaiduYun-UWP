using System;
using System.Threading;
using System.Threading.Tasks;

using Windows.Networking.BackgroundTransfer;

using BaiduYun.Misc;
using BaiduYun.Xaml.Input;

namespace BaiduYun.ViewModels {

    public class DownloadTaskViewModel : ViewModelBase, IDisposable {

        private DownloadOperation op;
        private CancellationTokenSource cts = new CancellationTokenSource();

        private uint timeElapsed = 0;
        private double percent;
        private ulong speed;
        private ulong lastBytes;

        public DownloadTaskViewModel(DownloadOperation op) {
            this.op = op;
            ResumeCommand = new ActionCommand(Resume, CanResume);
            PauseCommand = new ActionCommand(Pause, CanPause);
            CancelCommand = new ActionCommand(Cancel);
            UpdateValues();
        }

        // This method is called by a timer
        public void UpdateValues() {
            var progress = Op.Progress;
            speed = progress.BytesReceived - lastBytes;
            lastBytes = progress.BytesReceived;
            if (progress.TotalBytesToReceive > 0)
                Percent = lastBytes * 100.0 / progress.TotalBytesToReceive;
            else
                Percent = 100.0;
            ++TimeElapsed;
            OnPropertyChanged(nameof(Speed));
        }

        public async Task StartAsync() {
            await op.StartAsync().AsTask(cts.Token);
        }

        public async Task AttachAsync() {
            await op.AttachAsync().AsTask(cts.Token);
        }

        public void Dispose() {
            cts.Dispose();
        }

        public bool CanPause() {
            return op.Progress.Status == BackgroundTransferStatus.Running;
        }

        public bool CanResume() {
            switch (op.Progress.Status) {
                case BackgroundTransferStatus.PausedByApplication:
                case BackgroundTransferStatus.PausedCostedNetwork:
                case BackgroundTransferStatus.PausedNoNetwork:
                    return true;
                default: return false;
            }
        }

        private void StateChanged() {
            ResumeCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        public void Resume() {
            op.Resume();
            StateChanged();
        }

        public void Pause() {
            op.Pause();
            StateChanged();
        }

        public void Cancel() {
            cts.Cancel();
            StateChanged();
        }

        public ICommand ResumeCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public DownloadOperation Op {
            get { return op; }
        }

        public string Name {
            get { return Op.ResultFile.Name; }
        }

        public double Percent {
            get { return percent; }
            set { SetProperty(ref percent, value); }
        }

        public string Speed {
            get {
                var formatted = Utils.SanitizeSize(speed);
                if (formatted == "-")
                    formatted = "0 B";
                return formatted + "/s";
            }
        }

        public uint TimeElapsed {
            get { return timeElapsed; }
            set { SetProperty(ref timeElapsed, value); }
        }
    }
}
