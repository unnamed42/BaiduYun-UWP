using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaiduYun.ViewModels {

    public abstract class ViewModelBase : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propname = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        protected void SetProperty<T>(ref T src, T val, [CallerMemberName] string propname = "") {
            if (!ReferenceEquals(src, val)) {
                src = val;
                OnPropertyChanged(propname);
            }
        }
    }
}
