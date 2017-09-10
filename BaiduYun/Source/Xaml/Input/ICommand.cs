namespace BaiduYun.Xaml.Input {

    public interface ICommand : System.Windows.Input.ICommand {
        void RaiseCanExecuteChanged();
    }
}
