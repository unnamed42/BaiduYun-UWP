using System;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.UI.Xaml.Controls;

using BaiduYun.Storage;

namespace BaiduYun.Global {

    public static class Globals {

        private static IStorageFolder localRoot = ApplicationData.Current.LocalFolder;

        private static volatile bool loaded = false;
        private static Accounts accounts;

        public static string uk;
        public static string bdstoken;

        public static HttpBaseProtocolFilter filter;
        public static HttpClient Client { get; private set; }
        public static HttpClient NoRedirectClient { get; private set; }

        public static Accounts Accounts {
            get { while (!loaded) ; return accounts; }
        }

        public static Frame MainFrame { get; set; }

        public static async Task Init() {
            filter = new HttpBaseProtocolFilter() { AllowAutoRedirect = false };
            Client = new HttpClient();
            NoRedirectClient = new HttpClient(filter);

            var config = await localRoot.CreateFileAsync(Constants.CONFIG_FILE, CreationCollisionOption.OpenIfExists);
            accounts = await Accounts.Read(config);
            loaded = true;
        }

        public static async Task Dispose() {
            filter.Dispose();
            Client.Dispose();
            NoRedirectClient.Dispose();

            var config = await localRoot.CreateFileAsync(Constants.CONFIG_FILE, CreationCollisionOption.OpenIfExists);
            await Accounts.Save(config, accounts);
        }

    }
}
