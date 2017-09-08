using Windows.Data.Json;

using BaiduYun.Extensions;

namespace BaiduYun.Models {

    public class User {

        public string username;
        public string password;
        public string uk;

        public User() {
            username = password = uk = "";
        }

        public User(string username, string password, string uk) {
            this.username = username;
            this.password = password;
            this.uk = uk;
        }

        public User(string username, JsonObject otherData) {
            this.username = username;
            password = otherData.GetString(nameof(password));
            uk = otherData.GetString(nameof(uk));
        }

        public JsonObject ToJsonObject() {
            return (new JsonObject())
                .SetString(nameof(password), password)
                .SetString(nameof(uk), uk);
        }
    }
}
