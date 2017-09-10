using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.Storage;
using Windows.Data.Json;

using BaiduYun.Models;
using BaiduYun.Extensions;

namespace BaiduYun.Storage {

    public class Accounts {
        /// <summary>
        /// last logged user
        /// </summary>
        private string active;
        /// <summary>
        /// all users selected remember password
        /// </summary>
        private JsonObject users;

        public Accounts(string active, JsonObject users) {
            this.active = active;
            this.users = users;
        }

        public static async Task<Accounts> Read(IStorageFile file) {
            var content = await FileIO.ReadTextAsync(file);
            JsonObject users;
            string active = "";
            if (JsonObject.TryParse(content, out users)) {
                active = users.GetString(nameof(Accounts.active)) ?? "";
                users = users.GetObject(nameof(Accounts.users)) ?? new JsonObject();
            } else 
                users = new JsonObject();
            
            return new Accounts(active, users);
        }

        public static async Task Save(IStorageFile location, Accounts accounts) {
            await FileIO.WriteTextAsync(location, accounts.ToString());
        }

        public Accounts Add(User user) {
            users.SetObject(user.username, user.ToJsonObject());
            active = user.username;
            return this;
        }

        public User this[string username] {
            get {
                IJsonValue user;
                if (users.TryGetValue(username, out user)) {
                    return new User(username, user.GetObject());
                } else
                    return null;
            }
            set { Add(value); }
        }

        public string Active {
            get { return active; }
            set { active = value; }
        }

        public User ActiveUser {
            get {
                if (String.IsNullOrEmpty(active))
                    return null;
                return this[active];
            }
            set {
                active = value.username;
                this[active] = value;
            }
        }

        public ICollection<string> UserNames {
            get { return users.Keys; }
        }

        public static explicit operator JsonObject(Accounts accounts) {
            return (new JsonObject())
                .SetString(nameof(Accounts.active), accounts.active)
                .SetObject(nameof(Accounts.users), accounts.users);
        }

        public override string ToString() {
            return ((JsonObject)this).ToString();
        }
    }
}
