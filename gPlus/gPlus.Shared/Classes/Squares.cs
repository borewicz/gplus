using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace gPlus.Classes
{
    static class Squares
    {
        const string HOST = "https://www.googleapis.com";
        const string SQUARES_API = HOST + "/plusi/v2/ozInternal/getsquares";
        const string CATEGORIES_API = HOST + "/plusi/v2/ozInternal/getviewersquare";

        public class Square
        {
            public class Category
            {
                public string id;
                public string name { get; set; }
            }
            public string id;
            public string avatar { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string tagline { get; set; }
            public int memberCount { get; set; }
            public List<Category> categories = new List<Category>();
        }

        static string returnRequestData()
        {
            JObject json = new JObject(
                new JProperty("includePeopleInCommon", false),
                new JProperty("squareType", new JArray("JOINED"))
            );
            return json.ToString();
        }

        public static async Task<ObservableCollection<Square.Category>> GetCategories(string squareID)
        {
            ObservableCollection<Square.Category> categories = new ObservableCollection<Square.Category>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\"obfuscatedSquareId\":\"" + squareID + "\"}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(CATEGORIES_API), content);
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (var s in result["viewerSquare"]["streams"]["squareStream"])
                {
                    Square.Category category = new Square.Category();
                    category.id = (string)s["id"];
                    category.name = (string)s["name"];
                    categories.Add(category);
                }
                return categories;
            }
            else
            {
                return null;
            }
        }

        public static async Task<ObservableCollection<Square>> GetSquares()
        {
            ObservableCollection<Square> squares = new ObservableCollection<Square>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            HttpContent content = new StringContent(returnRequestData());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(SQUARES_API), content);
            //Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                try
                {
                    foreach (var s in result["joinedSquare"])
                    {
                        Square square = new Square();
                        square.id = (string)s["viewerSquare"]["square"]["obfuscatedGaiaId"];
                        square.name = (string)s["viewerSquare"]["square"]["profile"]["name"];
                        square.tagline = (string)s["viewerSquare"]["square"]["profile"]["tagline"];
                        square.description = (string)s["viewerSquare"]["square"]["profile"]["aboutText"];
                        square.avatar = "https:" + (string)s["viewerSquare"]["square"]["profile"]["photoUrl"];
                        square.memberCount = (int)s["viewerSquare"]["squareMemberStats"]["memberCount"];
                        squares.Add(square);
                    }
                }
                catch { }
                return squares;
            }
            else
            {
                return null;
            }
        }
    }
}
