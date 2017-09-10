using System;

namespace BaiduYun.Xaml.Input {

    public class ActionCommand : ICommand {

        private Action action;
        private Func<bool> canExecute = () => true;

        public event EventHandler CanExecuteChanged;

        public ActionCommand(Action action, Func<bool> canExecute = null) {
            this.action = action;
            if (canExecute != null)
                this.canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public void SetCanExecute(Func<bool> canExecute) {
            this.canExecute = canExecute;
            RaiseCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return canExecute();
        }

        public void Execute(object parameter) {
            action();
        }
    }
}
