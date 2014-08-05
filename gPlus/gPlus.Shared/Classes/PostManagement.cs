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
    static class PostManagement
    {

        const string POSTACTIVITY_API = "https://www.googleapis.com/plusi/v2/ozInternal/postactivity";
        const string LINKPREVIEW_API = "https://www.googleapis.com/plusi/v2/ozInternal/linkpreview";
        const string PLUSONE_API = "https://www.googleapis.com/plusi/v2/ozInternal/plusone";
        const string EDIT_ACTIVITY_API = "https://www.googleapis.com/plusi/v2/ozInternal/editactivity";
        const string DELETE_ACTIVITY_API = "https://www.googleapis.com/plusi/v2/ozInternal/deleteactivity";
        const string REPORT_ABUSE_API = "https://www.googleapis.com/plusi/v2/ozInternal/reportabuseactivity";

        static JArray _getAclItems(List<AclItem> list)
        {
            JArray array = new JArray ();
			foreach (AclItem item in list) {
                if (item.type == AclType.Public)
                {
                    array.Add(new JObject(new JProperty("groupType", "PUBLIC")));
                    return array;
                }
                else if (item.type == AclType.YourCircles)
                {
                    array.Add(new JObject(new JProperty("groupType", "YOUR_CIRCLES")));
                    return array;
                }
                else if (item.type == AclType.Square)
                {
                    array.Add(new JObject(new JProperty("squareId", new JObject(new JProperty("obfuscatedSquareId", item.id)))));
                    return array;
                }
                //else 
                else if (item.type == AclType.SpecifiedCircle)
                    array.Add(new JObject(new JProperty("circleId", item.id)));
                else if (item.type == AclType.SpecifiedPerson)
                    array.Add(new JObject(new JProperty("personId", new JObject(new JProperty("obfuscatedGaiaId", item.id)))));
			}
            return array;
        }

        private static string PasswordGenerator(int passwordLength, bool strongPassword)
        {
            Random random = new Random();
            int seed = random.Next(1, int.MaxValue);
            //const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            const string specialCharacters = @"!#$%&'()*+,-./:;<=>?@[\]_";

            var chars = new char[passwordLength];
            var rd = new Random(seed);

            for (var i = 0; i < passwordLength; i++)
            {
                // If we are to use special characters
                if (strongPassword && i % random.Next(3, passwordLength) == 0)
                {
                    chars[i] = specialCharacters[rd.Next(0, specialCharacters.Length)];
                }
                else
                {
                    chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                }
            }

            return new string(chars);
        }

        static async Task<JObject> buildPost(string content, List<AclItem> list, string link, string emoticon, string reshareId, Location.Place place, string imageContent)
        {
            JObject json = new JObject(
                new JProperty("updateText", content),
                new JProperty("externalId", Other.getTicks().ToString()+":"+PasswordGenerator(32, false)),
                new JProperty("sharingRoster", new JObject(
                    new JProperty("sharingTargetId", _getAclItems(list))
                )
            ));
            var square = (from item in list
                          where item.extraId != null
                          select item).FirstOrDefault();
            if (square != null)
            {
                json["squareStreams"] = new JArray(new JObject(new JProperty("squareId", square.id), new JProperty("streamId", square.extraId)));
            }
            if (link != null)
            {
                json["embed"] = await getMedia(link);
            }
            if (emoticon != null)
            {
                json["embed"] = new JObject(
                    new JProperty("emotishare", new JObject(
                        new JProperty("emotion", emoticon.ToUpper()),
                        new JProperty("url", "http://www.gstatic.com/s2/oz/images/emotishare/hdpi/" + emoticon + "_image.gif"),
                        new JProperty("proxiedImage", new JObject(
                            new JProperty("imageUrl", "http://www.gstatic.com/s2/oz/images/emotishare/hdpi/" + emoticon + "_image.gif")
                        ))
                    )),
                    new JProperty("thing", new JObject(
                        new JProperty("url", "http://www.gstatic.com/s2/oz/images/emotishare/hdpi/" + emoticon + "_image.gif"),
                        new JProperty("imageUrl", "http://www.gstatic.com/s2/oz/images/emotishare/hdpi/" + emoticon + "_image.gif"),
                        new JProperty("name", emoticon.ToUpperInvariant())
                    )),
                    new JProperty("type", new JArray(
                        "EMOTISHARE",
                        "THING")
                    ));
            }
            if (reshareId != null)
            {
                json["resharedUpdateId"] = reshareId;
            }
            if (place != null)
            {
                /*
                 * "latitudeE7":516207510,
"locationTag":"Widok, Ostrów Wielkopolski, wielkopolskie",
"longitudeE7":177878380

                 */
                json["location"] = new JObject(
                    new JProperty("latitudeE7", place.latitude),
                    new JProperty("locationTag", place.locationTag),
                    new JProperty("longitudeE7", place.longitude));
            }
            if (imageContent != null)
            {
                /*
                 * "imageStatus":"1",
"mediaType":"1",
"timestamp":-{"creationTimestampMs":1.406375684079E12
}
*/
                json["photosShareData"] = new JObject(
                    new JProperty("mediaRef", new JArray(
                        new JObject(
                            new JProperty("clientAssignedUniqueId", "cs_01_" + PasswordGenerator(32, false)),
                            new JProperty("imageData", imageContent),
                            new JProperty("imageStatus", "1"),
                            new JProperty("mediaType", "1")
                            )
                            )
                            ));
            }
            return json;
        }

        static async Task<JObject> getMedia(string link)
        {
            JObject json = new JObject(
                new JProperty("content", link),
                new JProperty("fallbackToUrl", true),
                new JProperty("useBlackboxPreviewData", true)
            );

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(LINKPREVIEW_API), content);
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                return (JObject)result["embedItem"][0];
            }
            else
                return null;
        }
		

        public static async Task<int> PostActivity(string text, List<AclItem> aclItems, string link, string emoticon, string reshareId, Location.Place place, string imageContent)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent((await buildPost(text, aclItems, link, emoticon, reshareId, place, imageContent)).ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(POSTACTIVITY_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> EditActivity(string activityID, string text)
        {
            JObject json = new JObject(
                new JProperty("externalId", activityID),
                new JProperty("updateText", text),
                new JProperty("preserveExistingAttachment", true)
            );

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(EDIT_ACTIVITY_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> DeleteActivity(string activityID)
        {
            JObject json = new JObject(
                new JProperty("activityId", activityID)
            );

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(DELETE_ACTIVITY_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> PlusOne(string activityID, bool isPlusOne)
        {
            JObject json = new JObject(
                new JProperty("isPlusone", isPlusOne),
                new JProperty("type", "TACO"),
                new JProperty("itemId", activityID)
            );

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(PLUSONE_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }

        public static async Task<int> ReportAbuse(string activityID)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");
            /*
             * {"abuseReport":{"abuseType":"SPAM"},"isUndo":false,"itemId":["z131i3hxwzfqchzci23rxvfzawrkilo0x"]} */
            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent("{\"abuseReport\":{\"abuseType\":\"SPAM\"},\"isUndo\":false,\"itemId\":[\"" + activityID + "\"]}");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(REPORT_ABUSE_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
                return 0;
            else
                return 1;
        }
    }
}
