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

        public class Notification
        {
            public string title { get; set; }
            public string description { get; set; }
            //public string communityID, userID, postID, eventID, id, url;
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
	
        public static async Task<ObservableCollection<Notification>> GetNotifications()
        {
            ObservableCollection<Notification> notifications = new ObservableCollection<Notification>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\n\"maxResults\":10\n}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(NOTIFICATION_API), content);
            //Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (var i in result["notificationsData"]["coalescedItem"])
                {
                    Notification notification = new Notification();
                    notification.title = (string)i["entityData"]["summarySnippet"]["heading"];
                    notification.description = (string)i["entityData"]["summarySnippet"]["description"];
                    notifications.Add(notification);
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
