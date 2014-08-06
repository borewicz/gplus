using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace gPlus.Classes
{
    static class Notifications
    {
        const string HOST = "https://www.googleapis.com";
        const string NOTIFICATION_API = HOST + "/plusi/v2/ozInternal/getnotifications";
        const string UNREAD_API = HOST + "/plusi/v2/ozInternal/fetchnotificationscount";
        const string SAVESTATES_API = "https://www.googleapis.com/plusi/v2/ozInternal/setnotificationsreadstates";
        const string UPDATELASTREAD_API = "https://www.googleapis.com/plusi/v2/ozInternal/notificationsupdatelastviewedversion";

        public class Notification
        {
            public string title { get; set; }
            public string description { get; set; }
            public bool isRead { get; set; }
            public string communityID, userID, postID, id, timestamp;
        }

        public class NotificationsList
        {
            private ObservableCollection<Notification> _list = new ObservableCollection<Notification>();
            public ObservableCollection<Notification> list { get { return _list; } }
            public string lastReadTime, continuationToken;
        }

        public static async Task<int> GetNotificationCount()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\"fetchCountParam\":{\"limitToPriority\":[\"HIGH\"],\"limitToReadState\":[\"UNREAD\"],\"view\":\"GPLUS_APP\"},\"requestHeader\":{\"clientId\":\"android_gplus\"}}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(UNREAD_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                return (int)result["count"];
            }
            else
                return -1;
        }

        public static async Task<int> UpdateLastReadTime(string lastReadTime)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");
            JObject json = new JObject(
                new JProperty("requestHeader", new JObject(
                    new JProperty("clientId", "android_gplus"))),
                new JProperty("updateLastViewedVersionParam", new JObject(
                    new JProperty("lastViewedVersion", lastReadTime),
                    new JProperty("view", "GPLUS_APP")
                )
            ));
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(UPDATELASTREAD_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return -1;
        }

        public static async Task<int> SetNotificationsReadState(string lastReadTime, List<string> notificationIDs)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");
            JObject json = new JObject(
                new JProperty("setReadStatesParam", new JObject(
                    new JProperty("newReadState",  "READ"),
                    new JProperty("notificationToSet", new JArray())
                )
            ));
            foreach (var s in notificationIDs)
            {
                JObject item = new JObject(
                    new JProperty("key", s),
                    new JProperty("latestVersion", lastReadTime)
                    );
                ((JArray)json["setReadStatesParam"]["notificationToSet"]).Add(item);
            }
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(SAVESTATES_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return -1;
        }

        public static async Task<NotificationsList> GetNotifications()
        {
            NotificationsList notifications = new NotificationsList();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\n\"maxResults\":10\n}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(NOTIFICATION_API), content);
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                notifications.lastReadTime = Convert.ToString((long)result["notificationsData"]["lastReadTime"]);
                notifications.continuationToken = (string)result["notificationsData"]["continuationToken"];
                foreach (var i in result["notificationsData"]["coalescedItem"])
                {
                    Notification notification = new Notification();
                    notification.id = (string)i["id"];
                    notification.title = (string)i["entityData"]["summarySnippet"]["heading"];
                    notification.description = (string)i["entityData"]["summarySnippet"]["description"];
                    notification.isRead = (bool)i["isRead"];
                    switch ((string)i["category"])
                    {
                        case "STREAM": notification.postID = (string)i["entityReference"];
                            break;
                        case "SQUARE": try { notification.communityID = (string)i["entityData"]["squares"]["subscription"].First()["square"]["oid"]; }
                            catch { }
                            break;
                        case "CIRCLE": notification.userID = (string)i["entityReference"];
                            break;
                    }
                    notifications.list.Add(notification);
                }
                return notifications;
                //return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }
    }
}
