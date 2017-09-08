using System;
using System.Linq;
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
        private IDictionary<string, User> users;

        private Accounts(string active, JsonObject users) {
            this.active = active;
            this.users = users.ToDictionary(kvp => kvp.Key, kvp => new User(kvp.Key, kvp.Value.GetObject()));
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

        public User this[string username] {
            get { return users.TryGetValue(username, out var ret) ? ret : null; }
            set { users.Add(username, value); }
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
                Active = value.username;
                users[active] = value;
            }
        }

        public ICollection<string> UserNames {
            get { return users.Keys; }
        }

        public static explicit operator JsonObject(Accounts accounts) {
            var users = new JsonObject();
            foreach (var kvp in accounts.users) 
                users.SetObject(kvp.Key, kvp.Value.ToJsonObject());
            
            return (new JsonObject())
                .SetString(nameof(Accounts.active), accounts.active)
                .SetObject(nameof(Accounts.users), users);
        }

        public override string ToString() {
            return ((JsonObject)this).ToString();
        }
    }
}
