using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace gPlus.Classes
{
    public enum AclType
    {
        Public = 1,
        Square = 2,
        YourCircles = 3,
        SpecifiedCircle = 4,
        SpecifiedPerson = 5
    }

    public class Info
    {
        public string name, userID, avatar;
    }

    public class AclItem
    {
        public string id;
        public string name { get; set; }
        public string extraId;
        public AclType type;

        public AclItem(string id, string extraId, string name, AclType type)
        {
            this.id = id;
            this.extraId = extraId;
            this.name = name;
            this.type = type;
        }
    }  

    public static class Other
    {
        const string LOAD_SOCIAL_API = "https://www.googleapis.com/plusi/v2/ozInternal/getmobilesettings";
        public static Info info;

        public static long getTicks()
        {
            DateTime centuryBegin = new DateTime(1970, 1, 1);
            DateTime currentDate = DateTime.Now;
            long elapsedTicks = (currentDate.Ticks - centuryBegin.Ticks) / 10000; //odcinamy cyferki
            return elapsedTicks;
        }

        public static string parseLink(string link)
        {
            return (link.Contains("https:") ? link : "https:" + link);
        }

        public static async Task<Info> getInfo()
        {
            Info info = new Info();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\"allowNonGooglePlusUsers\":false}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(LOAD_SOCIAL_API), content);
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                info.avatar = Other.parseLink((string)result["user"]["info"]["photoUrl"]);
                info.name = (string)result["user"]["info"]["displayName"];
                info.userID = (string)result["user"]["info"]["obfuscatedGaiaId"];
                return info;
            }
            else
                return null;
        }

        public static async Task<string> StorageFileToBase64(StorageFile file)
        {
            string Base64String = "";

            if (file != null)
            {
                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                var reader = new DataReader(fileStream.GetInputStreamAt(0));
                await reader.LoadAsync((uint)fileStream.Size);
                byte[] byteArray = new byte[fileStream.Size];
                reader.ReadBytes(byteArray);
                Base64String = Convert.ToBase64String(byteArray);
            }
            return Base64String;
        }
    }

}
