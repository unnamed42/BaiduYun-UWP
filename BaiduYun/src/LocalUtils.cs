using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Data.Json;
using Windows.Security.Credentials;

namespace BaiduYun {

    public static class LocalUtils {

        private static JsonObject json = Globals.everything;

        public static async Task<JsonObject> ReadSettings() {
            var file = await Globals.localRoot.CreateFileAsync(Configs.CONFIG_FILE, CreationCollisionOption.OpenIfExists);
            var jstring = await FileIO.ReadTextAsync(file);
            JsonObject res;
            if (JsonObject.TryParse(jstring, out res))
                return res;
            else
                return new JsonObject();
        }

        public static async Task SaveSettings() {
            var file = await Globals.localRoot.CreateFileAsync(Configs.CONFIG_FILE, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json.ToString());
        }

        public static string CurrentUserName() {
            IJsonValue val;
            if (json.TryGetValue("currentUser", out val))
                return val.GetString();
            return null;
        }

        public static User CurrentUser() {
            var username = CurrentUserName();
            if (username == null)
                return null;
            User u;
            TryGetUser(CurrentUserName(), out u);
            return u;
        }

        public static ICollection<string> AllUserNames() {
            IJsonValue users;
            if(json.TryGetValue("users", out users))
                return users.GetObject().Keys;
            return null;
        }

        public static bool TryGetUser(string username, out User u) {
            IJsonValue users, user;
            if (json.TryGetValue("users", out users)) {
                if (users.GetObject().TryGetValue(username, out user)) {
                    u = User.FromJson(user.GetObject());
                    return true;
                }
            }
            u = null;
            return true;
        }

        private static PasswordCredential GetPasswordByUsername(string username) {
            try {
                var list = Globals.vault.FindAllByUserName(username);
                return list.Count != 0 ? list[0] : null;
            } catch(Exception e) {
                if (e.HResult != unchecked((int)0x80070490)) // Element not found exception
                    throw;
                return null;
            }
        }

        public static void AddUser(User u, string password) {
            IJsonValue users;
            if(!json.TryGetValue("users", out users)) {
                users = new JsonObject();
                json.Add("users", users);
            }
            users.GetObject().SetNamedValue(u.UserName, User.ToJson(u));
            if(u.SavePassword) {
                var ifexist = GetPasswordByUsername(u.UserName);
                if (ifexist != null)
                    Globals.vault.Remove(ifexist);
                Globals.vault.Add(new PasswordCredential("BaiduYun", u.UserName, password));
            }
        }

        public static string GetPassword(string username) {
            var item = GetPasswordByUsername(username);
            if(item != null) {
                item.RetrievePassword();
                return item.Password;
            }
            return "";
        }

        public static IJsonValue GetCurrentUserInfo(string infoname) {
            var username = CurrentUserName();
            if (username == null)
                return null;
            IJsonValue users, user;
            if(json.TryGetValue("users", out users)) {
                if (users.GetObject().TryGetValue(username, out user))
                    return user.GetObject().GetNamedValue(infoname);
            }
            return null;
        }
    };

}