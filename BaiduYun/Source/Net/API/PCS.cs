using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Windows.Web.Http;

using BaiduYun.Misc;
using BaiduYun.Global;
using BaiduYun.Net.API.Response;

using Newtonsoft.Json;

namespace BaiduYun.Net.API {

    public static class PCS {

        public static async Task<string> GetUserUK() {
            return await WebUtils.GetAsync(Constants.PAN_URL, null, async (response) => {
                var str = await response.Content.ReadAsStringAsync();
                var match = Regex.Match(str, Constants.UK_RE, RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : null;
            });
        }

        public static async Task<string> GetBDSToken() {
            return await WebUtils.GetAsync(Constants.PAN_HOME, null, async (response) => {
                var str = await response.Content.ReadAsStringAsync();
                var match = Regex.Match(str, Constants.BDSTOKEN_RE, RegexOptions.IgnoreCase);
                return match.Success ? match.Groups[1].Value : null;
            });
        }

        public static async Task<string> GetDSign() {
            return await WebUtils.GetAsync(Constants.PAN_HOME, null, async (response) => {
                var html = await response.Content.ReadAsStringAsync();
                var sign1 = Utils.RegexSearch(html, @"""sign1""\s*:\s*""([A-Za-z0-9])""");
                var sign3 = Utils.RegexSearch(html, @"""sign3""\s*:\s*""([A-Za-z0-9])""");
                var timestamp = Utils.RegexSearch(html, @"""timestamp""\s*:\s*([0-9]+)[^0-9]");

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
            });
        }

        public static async Task<string> GetUserName() {
            return await WebUtils.GetAsync(Constants.PAN_HOME, null, async (response) => {
                var str = await response.Content.ReadAsStringAsync();
                var match = Regex.Match(str, Constants.USERNAME_RE, RegexOptions.IgnoreCase);
                var name = match.Success ? match.Groups[1].Value : null;
                return Regex.Replace(name, @"\\[Uu]([0-9A-Fa-f]{4})", (m) => {
                    return Char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier));
                });
            });
        }

        public static async Task<Quota> GetQuota() {
            var sb = new StringBuilder(Constants.PAN_API);
            sb.Append("quota?channel=chunlei")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&web=1").Append("&t=").Append(Utils.TimeStamp());

            return await WebUtils.GetAsync(sb.ToString(), null, WebUtils.ParseAs<Quota>);
        }

        public static async Task<UserInfo> GetUserInfo(string bdstoken, string uk) {
            var sb = new StringBuilder(Constants.PAN_URL);
            sb.Append("pcloud/user/getinfo?channel=chunlei")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&web=1").Append("&bdstoken=").Append(bdstoken)
              .Append("&query_uk=").Append(uk)
              .Append("&t=").Append(Utils.TimeStamp());

            return await WebUtils.GetAsync(sb.ToString(), null, WebUtils.ParseAs<UserInfo>);
        }

        // Deprecated
        public static async Task<string> GetPublicSharedFiles(string bdstoken, string uk) {
            var sb = new StringBuilder(Constants.PAN_URL);
            sb.Append("pcloud/feed/getsharelist?")
              .Append("category=0&auth_type=1&request_location=share_home&channel=chunlei&web=1")
              .Append("&t=").Append(Utils.TimeStamp())
              .Append("&start=0").Append("&limit=100")
              .Append("query_uk=").Append(uk)
              .Append("&bdstoken=").Append(bdstoken)
              .Append("&client_type=").Append(Constants.PC_CLIENT_TYPE);

            return await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", Constants.PAN_HOME);
            }, async (response) => {
                return await response.Content.ReadAsStringAsync();
            });
        }

        public static async Task<IList<YunFile>> GetFileList(string bdstoken, string path, int page = 1, int count = 100) {
            var timestamp = Utils.TimeStamp();
            var sb = new StringBuilder(Constants.PAN_API);
            sb.Append("list?channel=chunlei&web=1&order=time&desc=1")
              .Append("&client_type=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&num=").Append(count)
              .Append("&t=").Append(timestamp)
              .Append("&_=").Append(timestamp)
              .Append("&page=").Append(page)
              .Append("&dir=").Append(WebUtils.UrlEncode(path))
              .Append("&bdstoken=").Append(bdstoken);

            // request.Headers.Add("Content-type", Constants.ACCEPT_FORM_UTF8);
            return await WebUtils.GetAsync(sb.ToString(), null, async (response) => {
                var list = await WebUtils.ParseAs<YunFileList>(response);
                return list?.list.OrderByDescending(x => x.isdir)
                                 .ThenBy(x => x.server_filename).ToList();
            });
        }

        public static async Task<bool> RenameFile(string bdstoken, string path, string newname) {
            var sb = new StringBuilder(Constants.PAN_API);
            sb.Append("filemanager?channel=chunlei&web=1&opera=rename&appid=250528")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&bdstoken=").Append(bdstoken)
              .Append("&logid=").Append(Utils.LogIDGenerator());
            var data = String.Format("filelist=[{{\"path\":\"{0}\",\"newname\":\"{1}\"}}]", path, newname);

            return await WebUtils.PostAsync(sb.ToString(), data, null, async (response) => {
                var pattern = new { errno = 0 };
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeAnonymousType(str, pattern).errno == 0;
            });
        }

        // dlink: have download link included in returned object
        public static async Task<IList<FileMeta>> GetFileMetadata(string bdstoken, IEnumerable<YunFile> files, bool dlink = true) {
            var sb = new StringBuilder(Constants.PAN_API);
            sb.Append("filemetas?channel=chunlei&web=1")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&bdstoken=").Append(bdstoken);
            
            var data = String.Format("dlink={0}&target=[{1}]", dlink ? '1' : '0',
                                     String.Join(",", files.Select(file => String.Format("\"{0}\"", file.path))));

            return await WebUtils.PostAsync(sb.ToString(), data, null, async (response) => {
                var list = await WebUtils.ParseAs<FileMetaList>(response);
                return list?.errno == 0 ? list.info : null;
            });
        }

        public static async Task<string> GuanjiaDownload(string bdstoken, string path) {
            var sb = new StringBuilder(Constants.PCS_URL);
            sb.Append("file?").Append("app_id=250528&method=locatedownload")
              .Append("&BDUSS=").Append(WebUtils.GetCookieValue("BDUSS"))
              .Append("&t=").Append(Utils.TimeStamp())
              .Append("&bdstoken=").Append(bdstoken)
              .Append("&path=").Append(WebUtils.UrlEncode(path));
            
            return await WebUtils.GetAsync(sb.ToString(), null, async (response) => {
                var info = await WebUtils.ParseAs<DownloadInfo>(response);
                return String.Format("https://{0}{1}", info.host, info.path);
            });
        }

        public static async Task<IList<string>> GetFileDownloadLink(string bdstoken, IEnumerable<YunFile> files) {
            var linkTasks = (await GetFileMetadata(bdstoken, files))?.Select(async (meta) => {
                var url = String.Format("{0}&cflg={1}", meta.dlink, WebUtils.GetCookieValue("cflag", Constants.PAN_URL));
                return await WebUtils.GetAsync(url, (request) => {
                    request.Headers.Add("Accept", Constants.ACCEPT_HTML);
                }, async (response) => {
                    if (response.StatusCode == HttpStatusCode.Found)
                        return response.Headers.Location.ToString();
                    else
                        return await GuanjiaDownload(bdstoken, meta.path);
                }, true);
            });

            await Task.WhenAll(linkTasks);
            return linkTasks.Select(task => task.Result).ToList();
        }

        public static async Task<string> GetFileDownloadLink(string bdstoken, YunFile file) {
            var url = await GetFileDownloadLink(bdstoken, new List<YunFile>() { file });
            return url.FirstOrDefault();
        }

        public static async Task<bool> MoveToTrash(string bdstoken, IEnumerable<YunFile> files) {
            var sb = new StringBuilder(Constants.PAN_API);
            sb.Append("filemanager?channel=chunlei&web=1&opera=delete")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&bdstoken=").Append(bdstoken);
            
            var data = String.Format("filelist=[{0}]", String.Join(",", files.Select(file => String.Format("\"{0}\"", file.path))));

            return (await WebUtils.PostAsync(sb.ToString(), data, null, WebUtils.ParseAs<TrashInfo>))?.errno == 0;
        }

        public static async Task<List<SharedFile>> SharedFiles(string bdstoken) {
            var sb = new StringBuilder(Constants.PAN_URL);
            sb.Append("share/record?channel=chunlei&web=1&page=1&order=ctime&desc=1&app_id=250528")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&_=").Append(Utils.TimeStamp())
              .Append("&logid=").Append(Utils.LogIDGenerator())
              .Append("&bdstoken=").Append(bdstoken);

            return (await WebUtils.GetAsync(sb.ToString(), (request) => {
                request.Headers.Add("Referer", "http://pan.baidu.com/share/manage");
            }, WebUtils.ParseAs<SharedFileList>)).list;
        }

        public static async Task<bool> CreateShare(string bdstoken, IEnumerable<YunFile> files, string passwd = "") {
            var sb = new StringBuilder(Constants.PAN_URL);
            sb.Append("share/set?channel=chunlei&web=1&app_id=250528")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&logid=").Append(Utils.LogIDGenerator())
              .Append("&bdstoken=").Append(bdstoken);

            var data = String.Format("fid_list={0}&channel_list={1}",
                                     WebUtils.UrlEncode('[' + String.Join(",", files.Select(file => file.fs_id)) + ']'),
                                     WebUtils.UrlEncode("[]"));

            if (String.IsNullOrWhiteSpace(passwd))
                data += "&schannel=0";
            else
                data += String.Format("&pwd={0}&schannel=4", WebUtils.UrlEncode(passwd));

            return (await WebUtils.PostAsync(sb.ToString(), data, null, WebUtils.ParseAs<ShareResponse>))?.errno == 0;
        }

        public static async Task<bool> DisableShare(string bdstoken, IEnumerable<SharedFile> files) {
            var sb = new StringBuilder(Constants.PAN_URL);
            sb.Append("share/cancel?channel=chunlei&web=1")
              .Append("&clienttype=").Append(Constants.PC_CLIENT_TYPE)
              .Append("&logid=").Append(Utils.LogIDGenerator())
              .Append("&bdstoken=").Append(bdstoken);
            
            var data = String.Format("shareid_list=[{0}]",
                                     String.Join(",", files.Select(file => file.shareId)));

            return await WebUtils.PostAsync(sb.ToString(), data, null, async (response) => {
                var pattern = new { errno = 0 };
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeAnonymousType(str, pattern)?.errno == 0;
            });
        }
    };

}
