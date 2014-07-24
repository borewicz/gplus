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
    static class Comments
    {
        const string HOST = "https://www.googleapis.com";
        const string POST_COMMENT_API = HOST + "/plusi/v2/ozInternal/postcomment";
        const string PLUS_ONE_COMMENT_API = HOST + "/plusi/v2/ozInternal/plusone";
        const string DELETE_COMMENT_API = HOST + "/plusi/v2/ozInternal/deletecomment";
        const string EDIT_COMMENT_API = HOST + "/plusi/v2/ozInternal/editcomment";

        public static async Task<int> PostComment(string text, string postID, string userID)
        {
            var ticks = Other.getTicks().ToString();
            JObject json = new JObject(
                new JProperty("activityId", postID),
                new JProperty("commentText", text),
                new JProperty("creationTimeMs", ticks),
                new JProperty("clientId", userID+ticks+"-1633881820")
                );
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(POST_COMMENT_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> EditComment(string text, string commentID, string postID)
        {
            JObject json = new JObject(
                new JProperty("activityId", postID),
                new JProperty("commentId", commentID),
                new JProperty("commentText", text)
                );
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(EDIT_COMMENT_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> DeleteComment(string commentID)
        {
            JObject json = new JObject(
                new JProperty("commentId", commentID)
                );
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(DELETE_COMMENT_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> PlusOne(string commentID, bool isPlusOne)
        {
            JObject json = new JObject(
                new JProperty("isPlusone", isPlusOne),
                new JProperty("type", "TACO_COMMENT"),
                new JProperty("itemId", commentID)
            );

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(PLUS_ONE_COMMENT_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }
    }
}
