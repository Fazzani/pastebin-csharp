﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PastebinAPI
{
    public class User
    {
        private readonly string userKey;

        internal User(string userKey)
        {
            this.userKey = userKey;
        }

        /// <summary>Account name</summary>
        public string Name { get; private set; }
        public Language PreferedLanguage { get; private set; }
        public Expiration PreferedExpiration { get; private set; }
        public Visibility PreferedVisibility { get; private set; }
        public string AvatarURL { get; private set; }
        public string Website { get; private set; }
        public string Email { get; private set; }
        public string Location { get; private set; }
        /// <summary>Weather this user has pro account or not</summary>
        public bool IsPro { get; private set; }

        /// <summary>
        /// Creates a new paste from this user and uploads it to pastebin.
        /// To create anonymous paste use Paste.Create() or User.Guest.CreatePaste()
        /// </summary>
        /// <param name="language">If left out then user's PreferedLanguage will be used</param>
        /// <param name="visibility">If left out then user's PreferedVisibility will be used</param>
        /// <param name="expiration">If left out then user's PreferedExpiration will be used</param>
        /// <returns>Paste object containing the Url given from Pastebin</returns>
        public Paste CreatePaste(string text, string title = null, Language language = null, Visibility? visibility = null, Expiration expiration = null)
        {
            return Paste.Create(userKey, text, title, language ?? PreferedLanguage, visibility ?? PreferedVisibility, expiration ?? PreferedExpiration);
        }

        /// <summary>
        /// Creates a new paste from this user and uploads it to pastebin.
        /// To create anonymous paste use Paste.Create() or User.Guest.CreatePaste()
        /// </summary>
        /// <param name="language">If left out then user's PreferedLanguage will be used</param>
        /// <param name="visibility">If left out then user's PreferedVisibility will be used</param>
        /// <param name="expiration">If left out then user's PreferedExpiration will be used</param>
        /// <returns>Paste object containing the Url given from Pastebin</returns>
        public async Task<Paste> CreatePasteAsync(string text, string title = null, Language language = null, Visibility? visibility = null, Expiration expiration = null)
        {
            return await Paste.CreateAsync(userKey, text, title, language ?? PreferedLanguage, visibility ?? PreferedVisibility, expiration ?? PreferedExpiration);
        }

        /// <summary>
        /// Lists all pastes created by user
        /// </summary>
        /// <param name="resultsLimit">limits the paste count</param>
        /// <returns>Enumerable of pastes of this user</returns>
        public IEnumerable<Paste> ListPastes(int resultsLimit = 50)
        {
            var result = Utills.PostRequest(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_results_limit=" + resultsLimit,
                                            "api_option=" + "list");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);

            return Utills.PastesFromXML(result);
        }

        /// <summary>
        /// Lists all pastes created by user
        /// </summary>
        /// <param name="resultsLimit">limits the paste count</param>
        /// <returns>Enumerable of pastes of this user</returns>
        public async Task<IEnumerable<Paste>> ListPastesAsync(int resultsLimit = 50)
        {
            var result = await Utills.PostRequestAsync(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_results_limit=" + resultsLimit,
                                            "api_option=" + "list");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);

            return Utills.PastesFromXML(result);
        }

        /// <summary>
        /// Deletes a paste created by this user
        /// </summary>
        public void DeletePaste(Paste paste)
        {
            var result = Utills.PostRequest(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_paste_key=" + paste.Key,
                                            "api_option=" + "delete");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);
        }

        /// <summary>
        /// Updates user preferences information properties
        /// </summary>
        public void RequestPreferences()
        {
            var result = Utills.PostRequest(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_option=" + "userdetails");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);
            FillPreferences(result);
        }

        /// <summary>
        /// Updates user preferences information properties
        /// </summary>
        public async Task RequestPreferencesAsync()
        {
            var result = await Utills.PostRequestAsync(Utills.URL_API,
                                            "api_dev_key=" + Pastebin.DevKey,
                                            "api_user_key=" + userKey,
                                            "api_option=" + "userdetails");

            if (result.Contains(Utills.ERROR))
                throw new PastebinException(result);
            FillPreferences(result);
        }

        private void FillPreferences(string result)
        {
            XElement xuser = XElement.Parse(result);
            Name = xuser.Element("user_name").Value;
            PreferedLanguage = Language.Parse(xuser.Element("user_format_short").Value);
            PreferedExpiration = Expiration.Parse(xuser.Element("user_expiration").Value);
            PreferedVisibility = (Visibility)(int)xuser.Element("user_private");
            AvatarURL = xuser.Element("user_avatar_url").Value;
            Website = xuser.Element("user_website").Value;
            Email = xuser.Element("user_email").Value;
            Location = xuser.Element("user_location").Value;
            IsPro = xuser.Element("user_account_type").Value == "1";
        }

        /// <summary>
        /// User Name
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;
    }
}
