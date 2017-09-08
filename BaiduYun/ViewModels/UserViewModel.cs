﻿using System.Windows.Input;
using System.Threading.Tasks;

using Windows.UI.Core;

using BaiduYun.API;
using BaiduYun.UWP;
using BaiduYun.Global;

namespace BaiduYun.ViewModels {

    public class UserViewModel : ViewModelBase {

        private const string PLACEHOLDER = "ms-appx:///Assets/placeholder.jpg";

        private string username;
        private string avatarUrl = PLACEHOLDER;

        private ICommand logout;

        public UserViewModel() {
            logout = new ActionCommand(ExecuteLogout);
        }

        public string UserName {
            get { return username; }
            private set { SetProperty(ref username, value); }
        }

        public string AvatarUrl {
            get { return avatarUrl; }
            private set { SetProperty(ref avatarUrl, value); }
        }

        public ICommand Logout {
            get { return logout; }
        }

        public async Task<bool> UpdateUserInfo() {
            var name = await PCS.GetUserName();
            if (name == null)
                return false;

            var user = await PCS.GetUserInfo(Globals.bdstoken, Globals.uk);
            if (user?.errno != 0) 
                return false;

            UserName = name;
            AvatarUrl = user.user_info.avatar_url;
            return true;
        }

        private async void ExecuteLogout() {
            if (await Auth.Logout()) {
                UserName = "";
                AvatarUrl = PLACEHOLDER;
                Globals.MainFrame.BackStack.Clear();
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

        }
    }
}