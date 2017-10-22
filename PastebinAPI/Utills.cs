using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PastebinAPI
{
    class Utills
    {
        public const string ERROR = @"Bad API request";
        public const string URL = @"https://pastebin.com/";
        public const string URL_API = URL + @"api/api_post.php";
        public const string URL_LOGIN = URL + @"api/api_login.php";
        public const string URL_RAW = URL + @"raw.php?i=";
        public const string RAW_PATH = @"raw/";

        public static IEnumerable<Paste> PastesFromXML(string xml)
        {
            foreach (var paste in XElement.Parse("<pastes>" + xml + "</pastes>").Descendants("paste"))
                yield return Paste.FromXML(paste);
        }

        public static DateTime GetDate(long ticks) =>
            new DateTime(1970, 1, 1).AddSeconds(ticks).ToLocalTime();

        public static string PostRequest(string url, params string[] parameters)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            //TODO: Catch net exceptions
            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var postString = string.Join("&", parameters);
            byte[] byteArray = Encoding.UTF8.GetBytes(postString);
            request.ContentLength = byteArray.Length;
            try
            {
                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                using (var response = request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                throw new PastebinException("Connection to Pastebin failed", ex);
            }
        }

        public static async Task<string> PostRequestAsync(string url, params string[] parameters)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            //TODO: Catch net exceptions
            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var postString = string.Join("&", parameters);
            byte[] byteArray = Encoding.UTF8.GetBytes(postString);
            request.ContentLength = byteArray.Length;
            try
            {
                using (var dataStream = await request.GetRequestStreamAsync())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                
                using (var response = await request.GetResponseAsync())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (WebException ex)
            {
                throw new PastebinException("Connection to Pastebin failed", ex);
            }
        }
    }
}
