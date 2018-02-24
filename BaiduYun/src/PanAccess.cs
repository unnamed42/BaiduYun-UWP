using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace BaiduYun {

    public sealed partial class BaiduYun {

        public static async Task<string> GetUserUK() {
            using (var request = Utils.GetRequest(Constants.PAN_URL))
                return await Utils.ParseWithRegex(request, Constants.UK_RE, RegexOptions.IgnoreCase);
        }

        public static async Task<string> GetBDSToken() {
            using (var request = Utils.GetRequest(Constants.PAN_HOME))
                return await Utils.ParseWithRegex(request, Constants.BDSTOKEN_RE, RegexOptions.IgnoreCase);
        }

        public static async Task<string> GetDSign() {
            using (var request = Utils.GetRequest(Constants.PAN_HOME))
            using (var response = await Utils.RequestAsync(request)) {
                if (response != null && response.IsSuccessStatusCode) {
                    var html = await response.Content.ReadAsStringAsync();
                    var sign1 = Utils.RegexMatch(html, @"""sign1""\s*:\s*""([A-Za-z0-9])""", 1);
                    var sign3 = Utils.RegexMatch(html, @"""sign1""\s*:\s*""([A-Za-z0-9])""", 1);
                    var timestamp = Utils.RegexMatch(html, @"""timestamp""\s*:\s*([0-9]+)[^0-9]", 1);

                    if (sign1 == null || sign3 == null || timestamp == null)
                        return null;

                    // Evil baidu javascript code
                    return new Func<string, string, string>((string j, string r) => {
                        List<char> a = new List<char>(), p = new List<char>();
                        var o = "";
                        var v = j.Length;

                        for (var q = (char)0; q < 256; ++q) {
                            a.Add(j[q % v]);
                            p.Add(q);
                        }
                        var u = 0;
                        for (var q = (char)0; q < 256; ++q) {
                            u = (u + p[q] + a[q]) % 256;
                            var t = p[q];
                            p[q] = p[u];
                            p[u] = t;
                        }
                        var i = 0;
                        u = 0;
                        for (int q = 0; q < r.Length; ++q) {
                            i = (i + 1) % 256;
                            u = (u + p[i]) % 256;
                            var t = p[i];
                            p[i] = p[u];
                            p[u] = t;
                            var k = p[(p[i] + p[u]) % 256];
                            o += r[q] ^ k;
                        }
                        return Convert.ToBase64String(new UTF8Encoding().GetBytes(o));
                    })(sign3, sign1);
                }
                return null;
            }
        }

        public static async Task<string> GetUserName() {
            using (var request = Utils.GetRequest(Constants.PAN_HOME)) {
                var name = await Utils.ParseWithRegex(request, Constants.USERNAME_RE, RegexOptions.IgnoreCase);
                return Regex.Replace(name, @"\\[Uu]([0-9A-Fa-f]{4})", (m) => {
                    return Char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier));
                });
            }
        }

        public static async Task<Quota> GetQuota() {
            var url = Constants.PAN_API +
                      String.Format("quota?channel=chunlei&clienttype={0}&web=1&t={1}",
                                    Constants.PC_CLIENT_TYPE,
                                    Utils.TimeStamp());

            using (var request = Utils.GetRequest(url))
                return await Utils.ParseAsObject<Quota>(request);
        }

        public static async Task<UserInfo> GetUserInfo(string uk) {
            var url = Constants.PAN_URL + "pcloud/user/getinfo?channel=chunlei" +
                      String.Format("&clienttype={0}&web=1&bdstoken={1}&query_uk={2}&t={3}",
                                    Constants.PC_CLIENT_TYPE, Globals.bdstoken, uk, Utils.TimeStamp());

            using (var request = Utils.GetRequest(url))
                return await Utils.ParseAsObject<UserInfo>(request);
        }
        
        // Deprecated
        public static async Task<string> GetPublicSharedFiles(string uk) {
            var url = Constants.PAN_URL + "pcloud/feed/getsharelist?&category=0&auth_type=1&request_location=share_home&channel=chunlei&web=1" +
                      String.Format("&t={0}&start={1}&limit={2}&query_uk={3}&bdstoken={4}&client_type={5}",
                                    Utils.TimeStamp(), 0, 100, uk, Globals.bdstoken, Constants.PC_CLIENT_TYPE);

            using (var request = Utils.GetRequest(url)) {
                request.Headers.Add("Referer", Constants.PAN_HOME);
                using (var response = await Utils.RequestAsync(request))
                    if (response != null && response.IsSuccessStatusCode) {
                        string json = await response.Content.ReadAsStringAsync();
                        return json;
                    } else
                        return null;
            }
        }

        public static async Task<List<YunFile>> GetFileList(string path, int page = 1, int count = 100) {
            var url = Constants.PAN_API + "list?channel=chunlei&web=1&order=time&desc=1" +
                      String.Format("&client_type={0}&num={1}&t={2}&_={2}&page={3}&dir={4}&bdstoken={5}",
                                    Constants.PC_CLIENT_TYPE, count, Utils.TimeStamp(),
                                    page, WebUtility.UrlEncode(path), Globals.bdstoken);

            using (var request = Utils.GetRequest(url)) {
                // request.Headers.Add("Content-type", Constants.ACCEPT_FORM_UTF8);

                var list = await Utils.ParseAsObject<YunFileList>(request);
                if (list != null && list.errno == 0) {
                    return list.list.OrderByDescending(x => x.isdir).ThenBy(x => x.server_filename).ToList();
                }
                return null;
            }
        }

        public static async Task<bool> RenameFile(string path, string newname) {
            var url = Constants.PAN_API + "filemanager?channel=chunlei&web=1&opera=rename&appid=250528" +
                      String.Format("&clienttype={0}&bdstoken={1}&logid={2}", 0, Globals.bdstoken, WebUtility.UrlEncode(Utils.LogIDGenerator()));
            var data = String.Format("filelist=[{{\"path\":\"{0}\",\"newname\":\"{1}\"}}]", path, newname);
            using (var request=Utils.PostRequest(url,data)) {
                var response = await Utils.ParseAsString(request);
                var pattern = new { errno = 0 };

                return response != null && 
                       JsonConvert.DeserializeAnonymousType(response, pattern).errno == 0;
            }
        }

        // dlink: have download link included in returned object
        public static async Task<List<FileMeta>> GetFileMetadata(IEnumerable<YunFile> files, bool dlink = true) {
            var url = Constants.PAN_API + "filemetas?channel=chunlei&web=1" +
                      String.Format("&clienttype={0}&bdstoken={1}", Constants.PC_CLIENT_TYPE, Globals.bdstoken);

            var data = String.Format("dlink={0}&target=[{1}]",
                                     dlink ? '1' : '0', 
                                     String.Join(",", files.Select((file) => { return '"' + file.path + '"'; })));
            using(var request = Utils.PostRequest(url, data)) {
                var response = await Utils.ParseAsObject<FileMetaList>(request);
                if (response != null && response.errno == 0)
                    return response.info;
                return null;
            }
        }

        public static async Task<string> GuanjiaDownload(string path) {
            var url = Constants.PCS_URL + "file?" + FileRequestParams("locatedownload") +
                      String.Format("&path={0}", WebUtility.UrlEncode(path));

            using (var request = Utils.GetRequest(url)) {
                var response = await Utils.ParseAsObject<DownloadInfo>(request);
                if (response == null)
                    return null;
                return String.Format("https://{0}{1}", response.host, response.path);
            }
        }

        public static async Task<List<string>> GetFileDownloadLink(IEnumerable<YunFile> files) {
            var metas = await BaiduYun.GetFileMetadata(files);
            if (metas == null)
                return null;

            var ret = new List<string>();

            foreach (var meta in metas) {
                if (meta.dlink == null)
                    return null;
                var url = meta.dlink + String.Format("&cflg={0}", Utils.GetCookieValue("cflag", Constants.PAN_URL));
                using (var request = Utils.GetRequest(url)) {
                    request.Headers.Add("Accept", Constants.ACCEPT_HTML);

                    using (var response = await Utils.RequestNoRedirectAsync(request)) {
                        if (response != null && response.StatusCode == Windows.Web.Http.HttpStatusCode.Found)
                            ret.Add(response.Headers.Location.ToString());
                        else {
                            var guanjia = await BaiduYun.GuanjiaDownload(meta.path);
                            if (guanjia != null)
                                ret.Add(guanjia);
                            else
                                return null;
                        }
                    }
                }
            }

            return ret;
        }

        public static async Task<string> GetFileDownloadLink(YunFile file) {
            var url = await BaiduYun.GetFileDownloadLink(new List<YunFile>() { file });
            return url.FirstOrDefault();
        }

        public static string FileRequestParams(string method) {
            return "app_id=250528" +
                   String.Format("&method={0}&BDUSS={1}&t={2}&bdstoken={3}", 
                                 method, Utils.GetCookieValue("BDUSS"), Utils.TimeStamp(), Globals.bdstoken);
        }

        public static async Task<bool> MoveToTrash(IEnumerable<YunFile> files) {
            var url = Constants.PAN_API + "filemanager?channel=chunlei&web=1&opera=delete" + 
                      String.Format("&clienttype={0}&bdstoken={1}", Constants.PC_CLIENT_TYPE, Globals.bdstoken);

            var data = String.Format("filelist=[{0}]", String.Join(",", files.Select(file => '"' + file.path + '"')));

            using (var request = Utils.PostRequest(url, data)) {
                var response = await Utils.ParseAsObject<TrashInfo>(request);
                return response != null && response.errno == 0;
            }
        }

        public static async Task<List<SharedFile>> SharedFiles() {
            var url = Constants.PAN_URL + "share/record?channel=chunlei&web=1&page=1&order=ctime&desc=1&app_id=250528" +
                      String.Format("&clienttype={0}&_={1}&logid={2}&bdstoken={3}", 0, Utils.TimeStamp(), Utils.LogIDGenerator(), Globals.bdstoken);

            using(var request = Utils.GetRequest(url)) {
                request.Headers.Add("Referer", "http://pan.baidu.com/share/manage");
                var response = await Utils.ParseAsObject<SharedFileList>(request);
                return response?.list;
            }
        }

        public static async Task<bool> CreateShare(IEnumerable<YunFile> files, string passwd = "") {
            var url = Constants.PAN_URL + "share/set?channel=chunlei&web=1&app_id=250528" +
                      String.Format("&clienttype=0&bdstoken={0}&logid={1}", Globals.bdstoken, Utils.LogIDGenerator());

            var fid_list = '[' + String.Join(",", files.Select(file => file.fs_id)) + ']';

            var data = String.Format("fid_list={0}&channel_list={1}",
                                     WebUtility.UrlEncode(fid_list), WebUtility.UrlEncode("[]"));
            if (String.IsNullOrWhiteSpace(passwd))
                data += "&schannel=0";
            else
                data += String.Format("&pwd={0}&schannel=4", WebUtility.UrlEncode(passwd));

            using (var request = Utils.PostRequest(url, data)) {
                var response = await Utils.ParseAsObject<ShareResponse>(request);
                return response != null && response.errno == 0;
            }
        }

        public static async Task<bool> DisableShare(IEnumerable<SharedFile> files) {
            var url = Constants.PAN_URL + "share/cancel?channel=chunlei&clienttype=0&web=1" + 
                      String.Format("&bdstoken={0}&logid={1}", Globals.bdstoken, Utils.LogIDGenerator());

            var data = String.Format("shareid_list=[{0}]",
                                     String.Join(",", files.Select(file => file.shareId)));
            using(var request=Utils.PostRequest(url,data)) {
                var response = await Utils.RequestAsync(request);
                var pattern = new { errno = 0 };
                if (response != null && response.IsSuccessStatusCode) {
                    var str = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeAnonymousType(str, pattern).errno == 0;
                }
            }
            return false;
        }
    };
    
}
