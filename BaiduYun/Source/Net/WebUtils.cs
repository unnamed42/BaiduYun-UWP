using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Windows.Web.Http;
using Windows.Storage.Streams;

using BaiduYun.Global;

using Newtonsoft.Json;

namespace BaiduYun.Net {

    public delegate void RequestModifier(HttpRequestMessage request);
    public delegate Task<T> ResponseHandler<T>(HttpResponseMessage response);

    public static class WebUtils {

        private const int TIMEOUT = 5;

        private static HttpClient client = Globals.Client;
        private static HttpClient noRedirectClient = Globals.NoRedirectClient;

        public static HttpStringContent UTF8StringContent(string content) {
            return new HttpStringContent(content, UnicodeEncoding.Utf8, Constants.ACCEPT_FORM);
        }

        public static HttpCookieManager AllCookies() {
            return Globals.filter.CookieManager;
        }

        public static HttpCookie GetCookie(string name, string uri = Constants.PASSPORT_BASE) {
            return AllCookies().GetCookies(new Uri(uri))
                               .Where((x) => x.Name == name)
                               .FirstOrDefault();
        }

        public static string GetCookieValue(string name, string uri = Constants.PASSPORT_BASE) {
            return GetCookie(name, uri)?.Value;
        }

        public static bool HasCookie(string name, string uri = Constants.PASSPORT_BASE) {
            return GetCookie(name, uri) != null;
        }

        public static void ClearAllCookies() {
            var all = AllCookies();
            foreach (var cookie in all.GetCookies(new Uri(Constants.PASSPORT_BASE)))
                all.DeleteCookie(cookie);
            foreach (var cookie in all.GetCookies(new Uri(Constants.PAN_URL)))
                all.DeleteCookie(cookie);
        }

        public static string UrlEncode(string url) {
            return WebUtility.UrlEncode(url);
        }

        public static IDictionary<string, string> ParseQueryString(string s) {
            // remove anything other than query string from url
            var delim = s.IndexOf('?');
            if (delim != -1)
                s = s.Substring(delim + 1);

            return s.Split('&').Select(split => split.Split('='))
                        .ToDictionary(pairs => WebUtility.UrlDecode(pairs[0]), 
                                      pairs => pairs.Length == 2 ? WebUtility.UrlDecode(pairs[1]) : "");
        }

        private static async Task<T> IssueRequest<T>(HttpRequestMessage request, ResponseHandler<T> handler, bool noredirect, int timeout) {
            var client = noredirect ? WebUtils.noRedirectClient : WebUtils.client;
            using (var cts = new CancellationTokenSource()) {
                cts.CancelAfter(TimeSpan.FromSeconds(timeout));
                try {
                    using (var response = await client.SendRequestAsync(request).AsTask(cts.Token)) {
                        if (response.IsSuccessStatusCode)
                            return await handler(response);
                        return default(T);
                    }
                } catch (TaskCanceledException) {
                    return default(T);
                } catch (Exception e) {
                    // address or host not resolvable
                    if (e.HResult == unchecked((int)(0x80072ee7)))
                        return default(T);
                    throw;
                }
            }
        }

        public static async Task<T> PostAsync<T>(string url, string content, RequestModifier modifier, ResponseHandler<T> handler, bool noredirect = false, int timeout = TIMEOUT) {
            using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url))) {
                modifier?.Invoke(request);
                request.Content = UTF8StringContent(content);
                return await IssueRequest(request, handler, noredirect, timeout);
            }
        }

        public static async Task<T> GetAsync<T>(string url, RequestModifier modifier, ResponseHandler<T> handler, bool noredirect = false, int timeout = TIMEOUT) {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url))) {
                modifier?.Invoke(request);
                return await IssueRequest(request, handler, noredirect, timeout);
            }
        }

        public static async Task<T> ParseAs<T>(HttpResponseMessage response) {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
