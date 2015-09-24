using Refractored.Xam.Settings;
using Refractored.Xam.Settings.Abstractions;
using System.Xml.Serialization;
using System.IO;
using Uptred.YouTube;
using Uptred.Vimeo;

namespace Uptred.Mobile
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }
        
        public static string VimeoAccessToken
        {
            get
            {
                return AppSettings.GetValueOrDefault("vimeo_authtoken", string.Empty);
            }
            set
            {
                AppSettings.AddOrUpdateValue("vimeo_authtoken", value);
            }
        }

        public static string YouTubeRefreshToken
        {
            get
            {
                return AppSettings.GetValueOrDefault("youtube_refreshtoken", string.Empty);
            }
            set
            {
                AppSettings.AddOrUpdateValue("youtube_refreshtoken", value);
            }
        }
        
        public static void SaveInfos()
        {
            saveVimeo();
            saveYouTube();
        }

        public static void LoadInfos()
        {
            loadVimeo();
            loadYouTube();
        }

        static void saveVimeo()
        {
            if (VimeoInfo == null)
            {
                AppSettings.AddOrUpdateValue("vimeo_meta", string.Empty);
                return;
            }
            XmlSerializer xs = new XmlSerializer(VimeoInfo.GetType());
            using (StringWriter sw = new StringWriter())
            {
                xs.Serialize(sw, VimeoInfo);
                AppSettings.AddOrUpdateValue("vimeo_meta", sw.ToString());
            }
        }

        static void saveYouTube()
        {
            if (YouTubeInfo == null)
            {
                AppSettings.AddOrUpdateValue("youtube_meta", string.Empty);
                return;
            }
            XmlSerializer xs = new XmlSerializer(YouTubeInfo.GetType());
            using (StringWriter sw = new StringWriter())
            {
                xs.Serialize(sw, YouTubeInfo);
                AppSettings.AddOrUpdateValue("youtube_meta", sw.ToString());
            }
        }

        static void loadVimeo()
        {
            var meta = AppSettings.GetValueOrDefault("vimeo_meta", string.Empty);
            if (meta == string.Empty)
            {
                VimeoInfo = null;
                return;
            }
            XmlSerializer xs = new XmlSerializer(typeof(VimeoUploadTask));
            using (TextReader tr = new StringReader(meta)) VimeoInfo = (VimeoUploadTask)xs.Deserialize(tr);
        }

        static void loadYouTube()
        {
            var meta = AppSettings.GetValueOrDefault("youtube_meta", string.Empty);
            if (meta == string.Empty)
            {
                YouTubeInfo = null;
                return;
            }
            XmlSerializer xs = new XmlSerializer(typeof(YouTubeUploadTask));
            using (TextReader tr = new StringReader(meta)) YouTubeInfo = (YouTubeUploadTask)xs.Deserialize(tr);
        }

        public static VimeoUploadTask VimeoInfo = null;
        public static YouTubeUploadTask YouTubeInfo = null;

        public static VimeoHook VimeoHook = null;
        public static YouTubeHook YouTubeHook = null;
    }
}