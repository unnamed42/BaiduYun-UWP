using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduYun.ViewModels {

    public class ShareViewModel : ViewModelBase {

        public ObservableCollection<SharedFileViewModel> Items { get; private set; } = new ObservableCollection<SharedFileViewModel>();

        public async Task Init() {

        }
    }
}
