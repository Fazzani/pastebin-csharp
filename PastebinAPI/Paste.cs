using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PastebinAPI
{
    public class Paste
    {
        /// <summary>
        /// Create Paste From Xml
        /// </summary>
        /// <param name="xpaste"></param>
        /// <returns></returns>
        internal static Paste FromXML(XElement xpaste)
        {
            var paste = new Paste();
            paste.Key = xpaste.Element("paste_key").Value;
            paste.CreateDate = Utills.GetDate((long)xpaste.Element("paste_date"));
            paste.Title = xpaste.Element("paste_title").Value;
            paste.Size = (int)xpaste.Element("paste_size");
            var exdate = (long)xpaste.Element("paste_expire_date");
            paste.ExpireDate = exdate != 0 ? Utills.GetDate(exdate) : paste.CreateDate;
            paste.Expiration = Expiration.FromTimeSpan(paste.ExpireDate - paste.CreateDate);
            paste.Visibility = (Visibility)(int)xpaste.Element("paste_private");
            paste.Language = Language.Parse(xpaste.Element("paste_format_short").Value);
            paste.Url = xpaste.Element("paste_url").Value;
            paste.Hits = (int)xpaste.Element("paste_hits");
            return paste;
        }

        internal static Paste Create(string userKey, string text, string title = null, Language language = null, Visibility visibility = Visibility.Public, Expiration expiration = null)
        {
            title = title ?? "Untitled";
            language = language ?? Language.Default;
            expiration = expiration ?? Expiration.Default;

            var result = Utills.PostRequest(Utills.URL_API,
                                            //required parameters
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_option=" + "paste",
                                            "api_paste_code=" + Uri.EscapeDataString(text),
                                            //optional parameters
                                            "api_user_key=" + userKey,
                                            "api_paste_name=" + Uri.EscapeDataString(title),
                                            "api_paste_format=" + language,
                                            "api_paste_private=" + (int)visibility,
                                            "api_paste_expire_date=" + expiration);

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);

            return FillPaste(text, title, language, visibility, expiration, result);
        }

        internal static async Task<Paste> CreateAsync(string userKey, string text, string title = null, Language language = null, Visibility visibility = Visibility.Public, Expiration expiration = null)
        {
            title = title ?? "Untitled";
            language = language ?? Language.Default;
            expiration = expiration ?? Expiration.Default;

            var result = await Utills.PostRequestAsync(Utills.URL_API,
                                            //required parameters
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_option=" + "paste",
                                            "api_paste_code=" + Uri.EscapeDataString(text),
                                            //optional parameters
                                            "api_user_key=" + userKey,
                                            "api_paste_name=" + Uri.EscapeDataString(title),
                                            "api_paste_format=" + language,
                                            "api_paste_private=" + (int)visibility,
                                            "api_paste_expire_date=" + expiration);

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);

            return FillPaste(text, title, language, visibility, expiration, result);
        }

        private static Paste FillPaste(string text, string title, Language language, Visibility visibility, Expiration expiration, string result)
        {
            var paste = new Paste();
            paste.Key = result.Replace(Utills.URL, string.Empty);
            paste.CreateDate = DateTime.Now;
            paste.Title = title;
            paste.Size = Encoding.UTF8.GetByteCount(text);
            paste.ExpireDate = paste.CreateDate + expiration.Time;
            paste.Expiration = expiration;
            paste.Visibility = visibility;
            paste.Language = language;
            paste.Hits = 0;
            paste.Url = result;
            paste.Text = text;

            return paste;
        }

        /// <summary>
        /// Creates a new paste anonymously and uploads it to pastebin
        /// </summary>
        /// <returns>Paste object containing the Url given from Pastebin</returns>
        public static Paste Create(string text, string title = null, Language language = null, Visibility visibility = Visibility.Public, Expiration expiration = null)
            => Create("", text, title, language, visibility, expiration);

        ///<summary>String of 8 characters that is appended at the end of the url</summary>
        public string Key { get; private set; }
        ///<summary>Date at witch the paste was created</summary>
        public DateTime CreateDate { get; private set; }
        public string Title { get; private set; }
        ///<summary>File size in bytes</summary>
        public int Size { get; private set; }
        ///<summary>Date at witch the paste will be removed from Pastebin</summary>
        public DateTime ExpireDate { get; private set; }
        public Expiration Expiration { get; private set; }
        public Visibility Visibility { get; private set; }
        public Language Language { get; private set; }

        /// <summary>
        /// Paste Url
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Raw Url
        /// </summary>
        public string RawUrl => $"{Utills.URL}{Utills.RAW_PATH}{Key}";

        ///<summary>Number of views</summary>
        public int Hits { get; private set; }

        /// <summary>
        /// Paste Content
        /// </summary>
        public string Text { get; private set; }

        private Paste() { }

        /// <summary>
        /// Gets the raw text for a given url
        /// </summary>
        public string GetRaw()
        {
            if (Visibility == Visibility.Private)
                throw new PastebinException("Private pastes can not be accessed");
            return Text = Utills.PostRequest(Utills.URL_RAW + Key);
        }

        /// <summary>
        /// Gets the raw text for a given url
        /// </summary>
        public async Task<string> GetRawAsync()
        {
            if (Visibility == Visibility.Private)
                throw new PastebinException("Private pastes can not be accessed");
            return Text = await Utills.PostRequestAsync(Utills.URL_RAW + Key);
        }

        public override string ToString() => Text ?? GetRaw();
    }
}
