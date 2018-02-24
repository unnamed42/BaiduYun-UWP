using System.IO;
using System.Collections.Generic;

using Windows.Data.Json;

namespace BaiduYun {

    public class User {
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public string UK { get; set; }
        public bool SavePassword { get; set; }
        public bool AutoLogin { get; set; }
        
        public static JsonObject ToJson(User u) {
            var json = new JsonObject();
            json.Add("userName", JsonValue.CreateStringValue(u.UserName));
            json.Add("avatarUrl", JsonValue.CreateStringValue(u.AvatarUrl));
            json.Add("uk", JsonValue.CreateStringValue(u.UK));
            json.Add("savePassword", JsonValue.CreateBooleanValue(u.SavePassword));
            json.Add("autoLogin", JsonValue.CreateBooleanValue(u.AutoLogin));
            return json;
        }

        public static User FromJson(JsonObject json) {
            return new User() {
                UserName = json.GetNamedString("userName"),
                AvatarUrl = json.GetNamedString("avatarUrl"),
                UK = json.GetNamedString("uk"),
                SavePassword = json.GetNamedBoolean("savePassword"),
                AutoLogin = json.GetNamedBoolean("autoLogin")
            };
        }
    };

    public enum FileType {
        Folder=0,
        File,
        Archive,
        Image,
        Code,
        Text,
        Audio,
        Video,
        PDF
    };

    public class YunFileAdapter {
        public YunFileAdapter(YunFile file) {
            File = file;
            Type = GetType(file.isdir, file.path);
        }

        public YunFile File { get; private set; }
        public FileType Type { get; private set; }
        public string FileIcon {
            get {
                switch(Type) {
                    case FileType.Folder: return Icons.Folder;
                    case FileType.File: default: return Icons.File;
                    case FileType.Archive: return Icons.ArchiveFile;
                    case FileType.Code: return Icons.CodeFile;
                    case FileType.Audio: return Icons.AudioFile;
                    case FileType.Video: return Icons.VideoFile;
                    case FileType.Image: return Icons.ImageFile;
                    case FileType.PDF: return Icons.PDFFile;
                    case FileType.Text: return Icons.TextFile;
                }
            }
        }
        public string ModifiedAt { get { return Utils.FormattedTimeStamp(File.server_mtime); } }
        public string FileSize { get { return Utils.FormattedSize(File.size); } }
        public List<YunFileAdapter> Children { get; set; } = null;

        public static FileType GetType(int isdir, string path) {
            if (isdir == 1)
                return FileType.Folder;
            var extension = Path.GetExtension(path.ToLower());
            switch (extension) {
                case ".jpg": case ".png": case ".bmp":
                    return FileType.Image;
                case ".zip": case ".rar": case ".7z": 
                case ".iso":
                    return FileType.Archive;
                case ".c": case ".cpp": case ".h": case ".hpp":
                case ".html": case ".js": case ".cs": case ".css":
                case ".py": case ".java": case ".sh":
                    return FileType.Code;
                case ".mp3": case ".wmv": case ".wav": 
                    return FileType.Audio;
                case ".mp4": case ".mkv": case ".avi": case ".rm":
                case ".rmvb": case ".flv": case ".ogg": 
                    return FileType.Video;
                case ".txt":
                    return FileType.Text;
                case ".pdf":
                    return FileType.PDF;
                default:
                    return FileType.File;
            }
        }
    };

    public class SharedFileAdapter {
        public SharedFileAdapter(SharedFile file) {
            File = file;
        }

        public SharedFile File { get; set; }
        public string Name { get { return Path.GetFileName(File.typicalPath); } }
        public string CreatedAt { get { return Utils.FormattedTimeStamp(File.ctime); } }
        public bool HasPassword { get { return File.passwd != "0"; } }
    };
}
