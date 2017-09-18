using System.IO;
using System.Linq;
using System.Collections.Generic;

using Windows.UI.Xaml.Controls;

using BaiduYun.Misc;
using BaiduYun.Global;
using BaiduYun.Net.API;
using BaiduYun.Net.API.Response;
using BaiduYun.Xaml.Input;

namespace BaiduYun.ViewModels {

    public class SharedFileViewModel {
        
        public SharedFileViewModel(SharedFile file) {
            File = file;
            DisableShareCommand = new DelegateCommand<ListViewBase>(DisableShare);
        }

        public SharedFile File { get; private set; }

        public string Name {
            get { return Path.GetFileName(File.typicalPath); }
        }

        public string CreatedAt {
            get { return Utils.SanitizeTimeStamp(File.ctime); }
        }

        public bool HasPassword {
            get { return File.passwd != "0"; }
        }

        public ICommand DisableShareCommand { get; private set; }

        public async void DisableShare(ListViewBase list) {
            var selected = list.SelectedItems.Count == 1 ?
                           (IEnumerable<SharedFile>)new[] { this.File } :
                           list.SelectedItems.Select(file => (file as SharedFileViewModel).File).ToList();

            if(await PCS.DisableShare(Globals.bdstoken, selected)) {

            }
            //var list = view.SelectedItems.Count == 0 ?
            //           new List<SharedFile>() { item.File } :
            //           ShareList.SelectedItems.Select(file => (file as SharedFileAdapter).File).ToList();
            //if (await BaiduYun.DisableShare(list))
            //    UpdateItems(shares.Select(share => share.File).Except(list));
        }
    }
}
