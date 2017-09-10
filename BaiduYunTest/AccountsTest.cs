using System;
using System.Reflection;
using System.Threading.Tasks;

using BaiduYun.Models;
using BaiduYun.Storage;

using Windows.Storage;
using Windows.Data.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BaiduYunTest {

    [TestClass]
    public class AccountsTest {

        private static IStorageFolder folder = ApplicationData.Current.LocalFolder;

        private Accounts accounts;
        private string active;
        private JsonObject users;

        private User[] source;

        private static object GetField(string name, Type type, object obj) {
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        }

        private static bool UserEquals(User one, User two) {
            return one.username == two.username &&
                   one.uk == two.uk &&
                   one.password == two.password;
        }

        public AccountsTest() {
            source = new[] {
                new User("1", "*", "*"),
                new User("2", "*", "*"),
                new User("3", "*", "*"),
                new User("4", "*", "*"),
                new User("5", "*", "*"),
                new User("6", "*", "*"),
                new User("7", "*", "*"),
                new User("8", "*", "*"),
            };
        }

        [TestMethod]
        public async Task TotalUserTest() {
            await InitializeUserTest();
            AddUserTest();
            UpdateUserTest();
            AccessUserTest();
            await ReopenUserTest();
        }
        
        public async Task InitializeUserTest() {
            accounts = await Accounts.Read(await folder.CreateFileAsync("users", CreationCollisionOption.ReplaceExisting));

            active = GetField(nameof(active), typeof(Accounts), accounts) as string;
            users = GetField(nameof(users), typeof(Accounts), accounts) as JsonObject;

            Assert.IsNotNull(active);
            Assert.IsNotNull(users);
        }
        
        public void AddUserTest() {
            foreach (var user in source) {
                accounts.Add(user);
                Assert.AreEqual(user.username, accounts.Active);
                Assert.IsTrue(UserEquals(user, accounts.ActiveUser));
            }

            foreach (var user in source)
                Assert.IsTrue(UserEquals(user, accounts[user.username]));
        }
        
        public void UpdateUserTest() {
            var name = "1";
            var u = new User(name, "125", "758");
            var exist = accounts[name];

            Assert.IsFalse(UserEquals(u, exist));
            accounts.Add(u);
            source[0] = u;
            Assert.IsTrue(UserEquals(u, accounts[name]));
            Assert.IsTrue(UserEquals(u, accounts.ActiveUser));
            Assert.AreEqual(name, accounts.Active);
        }
        
        public void AccessUserTest() {
            foreach (var user in source) {
                var data = accounts[user.username];
                Assert.IsTrue(UserEquals(user, data));
            }
        }
        
        public async Task ReopenUserTest() {
            await Accounts.Save(await folder.CreateFileAsync("users", CreationCollisionOption.ReplaceExisting), accounts);

            accounts = await Accounts.Read(await folder.CreateFileAsync("users", CreationCollisionOption.OpenIfExists));
            AccessUserTest();
        }
    }
}
