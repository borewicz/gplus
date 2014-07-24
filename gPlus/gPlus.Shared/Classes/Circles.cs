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
    static class Circles
    {
        const string HOST = "https://www.googleapis.com";
        const string CIRCLES_API = HOST + "/plusi/v2/ozInternal/loadsocialnetwork";

        public class Circle
        {
            /*
            public class User
            {
                public string id;
                public string name { get; set; }
                public string avatar { get; set; }
            }
            public List<User> users = new List<User>();
            */
            public string name { get; set; }
            public string description { get; set; }        
            public string id;
            public int memberCount { get; set; }
        }

        static string returnRequestData()
        {
            JObject json = new JObject(
                new JProperty("circlesOptions", new JObject(
                    new JProperty("includeCircles", true),
                    new JProperty("includeMemberCounts", true))
                ),
                new JProperty("systemGroupsOptions", new JObject(
                    new JProperty("includeMemberCounts", false),
                    new JProperty("includeSystemGroups", false))
                )
            );
            return json.ToString();       
        }

        public static async Task<ObservableCollection<Circle>> GetCircles()
        {
            ObservableCollection<Circle> circles = new ObservableCollection<Circle>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            HttpContent content = new StringContent(returnRequestData());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(CIRCLES_API), content);
            //Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (var c in result["viewerCircles"]["circle"])
                {
                    Circle circle = new Circle();
                    circle.id = (string)c["circleId"]["focusId"];
                    circle.name = (string)c["circleProperties"]["name"];
                    circle.description = (string)c["circleProperties"]["description"];
                    circle.memberCount = (int)c["circleProperties"]["memberCount"];
                    circles.Add(circle);
                }
                return circles;
            }
            else
            {
                return null;
            }
        }

    }
}
