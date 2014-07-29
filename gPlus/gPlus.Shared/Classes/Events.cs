using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace gPlus.Classes
{
    static class Events
    {
        const string MANAGEGUESTS_API = "https://www.googleapis.com/plusi/v2/ozInternal/eventmanageguests";
        const string RESPOND_API = "https://www.googleapis.com/plusi/v2/ozInternal/eventrespond";

        /*
        public enum RespondType
        {
            Yes = "ATTENDING",
            No = "NOT_ATTENDING",
            Maybe = "MAYBE"
        }
         */
        public class Event
        {
            public class User { public string userID { get; set; } public string name { get; set; } public string avatar { get; set; } public int guests { get; set; } }
            private List<User> _going = new List<User>();
            private List<User> _maybe = new List<User>();
            private List<User> _noResponse = new List<User>();
            private List<User> _notGoing = new List<User>();
            private List<User> _unknown = new List<User>(); //7
            public List<User> going { get { return _going; } }
            public List<User> maybe { get { return _maybe; } }
            public List<User> noResponse { get { return _noResponse; } }
            public List<User> notGoing { get { return _notGoing; } }
            public List<User> unknown { get { return _unknown; } }
            public int goingCount { get; set; }
            public int maybeCount { get; set; }
            public int noResponseCount { get; set; }
            public int notGoingCount { get; set; }
            public int unknownCount { get; set; }
            //public Poll youGoing { get; set; }
            public int yourGuestsCount { get; set; }
            public string eventID { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string location { get; set; }
            public string link { get; set; }
            public string authKey { get; set; }
            public string image { get; set; }
            public string userID { get; set; }
            public string user { get; set; }
            public string avatar { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
        }

        public static Event parseEvent(JObject eventData)
        {
            Event events = new Event();
            //if (eventData[0].ToString().Contains("gallery"))
            //    eventData = eventData[178].Last.First.First;
            //var eventData = data[97][data[97].Count() - 1].First.First;
            events.eventID = (string)eventData["id"];
            events.title = (string)eventData["name"];
            events.authKey = (string)eventData["authkey"];
            events.description = (string)eventData["description"];
            events.start = (string)eventData["startDate"];
            events.end = (string)eventData["endDate"];
            events.userID = (string)eventData["ownerObfuscatedId"];
            if ((eventData["location"] != null) && (eventData["location"].HasValues))
            {
                events.latitude = (string)eventData["location"]["placeV2"]["geo"]["geoCoordinatesV2"]["latitude"];
                events.longitude = (string)eventData["location"]["placeV2"]["geo"]["geoCoordinatesV2"]["longitude"];
                events.location = (string)eventData["location"]["placeV2"]["name"];
            }
            if (eventData["plusEventV2Data"].HasValues)
            {
                var imageItem = (from i in eventData["plusEventV2Data"]["theme"]["image"]
                                 where i["url"].ToString().Contains(".jpg") == true
                                 select i).FirstOrDefault();
                if (imageItem != null)
                    events.image = (string)imageItem["url"];
            }
            return events;
        }

        public static async Task<int> InviteUser(string eventID, string userID, string type)
        {
            JObject json = new JObject(
                new JProperty("actionType", type),
                new JProperty("eventId", eventID),
                /*
                new JProperty("eventSelector", new JObject(
                    new JProperty("authKey", "CMG70KjhyfP5Zg"),
                    new JProperty("eventId", eventID)
                    )),
                 */
                new JProperty("invitee", new JArray(
                    new JObject(new JProperty(
                        "ownerObfuscatedId", userID))
                    ))
                );
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(MANAGEGUESTS_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> RespondEvent(string eventID, string type)
        {
            JObject json = new JObject(
                new JProperty("response", type),
                new JProperty("eventId", eventID)
                /*
                new JProperty("eventSelector", new JObject(
                    new JProperty("authKey", "CMG70KjhyfP5Zg"),
                    new JProperty("eventId", eventID)
                    )),
                 */
                );
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(RESPOND_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }


    }
}
