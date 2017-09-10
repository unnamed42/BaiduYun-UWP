using System;

namespace BaiduYun.Xaml.Input {

    public class DelegateCommand<T> : ICommand {

        private Action<T> action;
        private Func<T, bool> canExecute = (t) => true;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> action, Func<T, bool> canExecute = null) {
            this.action = action;
            if (canExecute != null)
                this.canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public void SetCanExecute(Func<T, bool> canExecute) {
            this.canExecute = canExecute;
            RaiseCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return canExecute((T)parameter);
        }

        public void Execute(object parameter) {
            action((T)parameter);
        }
    }
}
