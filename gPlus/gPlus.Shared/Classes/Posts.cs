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
    static class Posts
    {
        const string HOST = "https://www.googleapis.com";
        const string ACTIVITIES_API = HOST + "/plusi/v2/ozInternal/getactivities";
        public const string ACTIVITY_API = HOST + "/plusi/v2/ozInternal/getactivity";
        public const string POSTACTIVITY_API = HOST + "/plusi/v2/ozInternal/postactivity";
        const string SEARCH_API = HOST + "/plusi/v2/ozInternal/searchquery";

        public class Post
        {
            public class Photo {
                public string desc { get; set; }
                public string url { get; set; }
                public string albumUrl { get; set; }
                public string image { get; set; }
                public string thumbImage { get; set; }
            }
            public class Comment {
                public string author { get; set; }
                public string text { get; set; }
                public string originalText { get; set; }
                public string time { get; set; }
                public string avatar { get; set; }
                public string commentID { get; set; }
                public string userID { get; set; }
                public int plusCount { get; set; }
                public bool isPlusonedByViewer { get; set; }
            }
            public class Shared {
                public string postID, userID;
                public string name { get; set; }
                public string avatar { get; set; }
                public string content { get; set; }
                public string richContent { get; set; }
            }
            public class Media {
                public string url { get; set; }
                public string title { get; set; }
                public string source { get; set; }
                public string image { get; set; }
                public string description { get; set; }
                //public string address { get; set; }
                //public string type { get; set; }
                //public string thumbUrl { get; set; }
                //public string photo { get; set; }
            }

            private Events.Event _event = new Events.Event();
            public Events.Event events { get { return _event; } set { this._event = value; } }

            public float latitude { get; set; }
            public float longitude { get; set; }
            public string locationTag { get; set; }
            public string mapUrl { get; set; }
            public string mapPageUrl { get; set; }

            private Media _media = new Media();
            public Media media { get { return _media; } }

            private Shared _shared = new Shared();
            public Shared shared { get { return _shared; } }

            private ObservableCollection<Comment> _comments = new ObservableCollection<Comment>();
            public ObservableCollection<Comment> comments { get { return _comments; } set { this._comments = value; } }

            private List<Photo> _photos = new List<Photo>();
            public List<Photo> photos { get { return _photos; } }


            public string type { get; set; }
            public string userID { get; set; }
            public string content { get; set; }
            public string richContent { get; set; }
            public string author { get; set; }
            public long time { get; set; }
            public string avatar { get; set; }
            public string postID { get; set; }
            public int plusCount { get; set; }
            public int reshareCount { get; set; }
            public int commentsCount { get; set; }
            public bool canViewerComment { get; set; }
            public bool isLocked { get; set; }
            public bool isMuted { get; set; }
            public bool isPlusonedByViewer { get; set; }
        }

        public class PostStream
        {
            public string pageToken { get; set; }
            private ObservableCollection<Post> _posts = new ObservableCollection<Post>();
            public ObservableCollection<Post> posts { get; set; }
        }

        static string returnRequestPost(string userID, string circleID, string squareID, string categoriesID)
        {
            JObject json = new JObject(
                new JProperty("skipPopularMixin", true),
                new JProperty("streamParams", new JObject(
                    new JProperty("sort", "LATEST"),
                    //new JProperty("collapserType", "MOBILE"),
                    //new JProperty("skipCommentCollapsing", true),
                    new JProperty("maxComments", 0)
                    //new JProperty("maxNumUpdates", 0)
                )
            ));
            //var extension1 = obj.SelectToken("extensions.settings.extension1") as JObject;
            if (userID != null)
            {
                json["streamParams"]["productionStreamOid"] = userID;
                //json["streamParams"]["viewType"] = "SQUARES"; //na pewno będzie trzeba to zrobić xD

            }
            else if (squareID != null)
            {
                json["streamParams"]["productionStreamOid"] = squareID;
                json["streamParams"]["viewType"] = "SQUARES";
                if (categoriesID != null)
                {
                    json["streamParams"]["squareStreamId"] = categoriesID;
                }
            }
            else if (circleID != null)
            {
                json["streamParams"]["focusGroupId"] = circleID;
                json["streamParams"]["viewType"] = "CIRCLES";
            }
            else
                json["streamParams"]["viewType"] = "CIRCLES";
            //focusGroupId = circleID
            return json.ToString();
        
        }

        static Post _parsePost(JToken result)
        {
            Post post = new Post();
            post.type = (string)result["sourceStreamName"];
            post.postID = (string)result["updateId"];
            post.author = (string)result["authorName"];
            post.avatar = (string)result["author"]["photoUrl"];
            post.content = (string)result["textTitle"];
            post.time = (long)result["timestamp"];
            post.richContent = (string)result["title"];
            post.userID = (string)result["author"]["obfuscatedId"];
            if (result["sharedFromAuthor"] != null)
            {
                post.shared.name = (string)result["sharedFromAuthor"]["userName"];
                post.shared.userID = (string)result["sharedFromAuthor"]["obfuscatedId"];
                post.shared.postID = (string)result["originalItemId"];
                post.shared.content = (string)result["originalAnnotation"];
                post.shared.richContent = (string)result["annotation"];
            }
            if (result["plusone"] != null)
            {
                post.plusCount = Convert.ToInt32((string)result["plusone"]["globalCount"]);
                post.isPlusonedByViewer = (bool)result["plusone"]["isPlusonedByViewer"];
            }
            post.commentsCount = (int)result["totalCommentCount"];
            post.reshareCount = (int)result["originalReshareCount"];
            post.isMuted = (bool)result["isMute"];
            post.isLocked = (bool)result["isStrangerPost"];
            post.canViewerComment = (bool)result["canViewerComment"];
            if (result["comment"] != null)
            foreach (var c in result["comment"])
            {
                Post.Comment comment = new Post.Comment();
                comment.author = (string)c["author"]["userName"];
                comment.userID = (string)c["obfuscatedId"];
                comment.commentID = (string)c["commentId"];
                comment.text = (string)c["text"];
                comment.originalText = (string)c["originalText"];
                comment.avatar = (string)c["author"]["photoUrl"];
                comment.time = (string)c["timestamp"];
                if (c["plusone"] != null)
                {
                    comment.plusCount = Convert.ToInt32((string)c["plusone"]["globalCount"]);
                    comment.isPlusonedByViewer = (bool)c["plusone"]["isPlusonedByViewer"];
                }
                post.comments.Add(comment);
            }
            if (result["location"] != null)
            {
                post.latitude = (float)result["location"]["latitude"];
                post.longitude = (float)result["location"]["latitude"];
                post.locationTag = (string)result["location"]["locationTag"];
                post.mapUrl = (string)result["location"]["mapUrl"];
                post.mapPageUrl = (string)result["location"]["mapsPageUrl"];
            }

            if (result["embed"] != null)
            {
                if (result["embed"]["emotishareV2"] != null)
                {
                    post.media.url = (string)result["embed"]["emotishareV2"]["url"];
                    post.media.title = (string)result["embed"]["emotishareV2"]["name"];
                    if (result["embed"]["emotishareV2"]["proxiedImage"] != null)
                        post.media.image = (string)result["embed"]["emotishareV2"]["proxiedImage"]["imageUrl"];
                }
                if (result["embed"]["videoObjectV2"] != null)
                {
                    post.media.url = (string)result["embed"]["videoObjectV2"]["url"];
                    if (result["embed"]["videoObjectV2"]["proxiedImage"] != null)
                        post.media.image = (string)result["embed"]["videoObjectV2"]["proxiedImage"]["imageUrl"];
                    post.media.title = (string)result["embed"]["videoObjectV2"]["name"];
                    post.media.description = (string)result["embed"]["videoObjectV2"]["description"];
                }
                if (result["embed"]["webPageV2"] != null)
                {
                    post.media.url = (string)result["embed"]["webPageV2"]["url"];
                    post.media.title = (string)result["embed"]["webPageV2"]["name"];
                    post.media.source = (string)result["embed"]["webPageV2"]["source"];
                    if (result["embed"]["webPageV2"]["proxiedImage"] != null)
                        post.media.image = (string)result["embed"]["webPageV2"]["proxiedImage"]["imageUrl"]; 
                }
            
                if (result["embed"]["postPhotoV2"] != null)
                {                
                    post.media.url = (string)result["embed"]["postPhotoV2"]["url"]; 
                    post.media.title = (string)result["embed"]["postPhotoV2"]["name"]; 
                    post.media.image = (string)result["embed"]["postPhotoV2"]["imageUrl"]; 
                    //post.photo.thumb = (string)result["embed"]["postPhotoV2"]["proxiedImage"]["imageUrl"]; 
                }
                if (result["embed"]["plusPhotoAlbumV2"] != null)
                {
                    /*
                     *  foreach (var item in data[97].Last.First.First[41])
                        {
                            Post.Photo photo = new Post.Photo();
                            photo.desc = item.Last.First.First[3].ToString();
                            photo.albumUrl = item.Last.First.First[0].ToString();
                            if (item.Last.First.First[5].HasValues)
                                photo.thumbImage = _parseLink(item.Last.First.First[5][0].ToString());
                            photo.image = item.Last.First.First[1].ToString();
                            post.photos.Add(photo);
                        }
                        var mainPhoto = (from p in post.photos
                                         where p.albumUrl.Contains(data[97].Last.First.First[26].ToString())
                                         select p).FirstOrDefault();
                        if (mainPhoto != null)
                            post.photo = mainPhoto.thumbImage;
                        else if (data[97].Last.First.First[5].HasValues)
                            post.photo = _parseLink(data[97].Last.First.First[5][0].ToString())
                     */
                    foreach (var item in result["embed"]["plusPhotoAlbumV2"]["associatedMedia"])
                    {
                        Post.Photo photo = new Post.Photo();
                        photo.desc = (string)item["plusPhotoV2"]["description"];
                        photo.image = (string)item["plusPhotoV2"]["imageUrl"];
                        photo.albumUrl = (string)item["plusPhotoV2"]["url"];
                        photo.thumbImage = (string)item["plusPhotoV2"]["proxiedImage"]["imageUrl"];
                        post.photos.Add(photo);
                    }
                }
                if (result["embed"]["plusEventV2"] != null)
                {
                    post.events = Events.parseEvent((JObject)result["embed"]["plusEventV2"]);
                }
            }
            return post;
        }

        public static async Task<PostStream> GetActivities(string userID, string circleID, string squareID, string categoryID)
        {
            PostStream posts = new PostStream();
            posts.posts = new ObservableCollection<Post>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            //client.DefaultRequestHeaders.Add("User-Agent", "com.google.android.apps.plus/411514804 (Linux; U; Android 4.1.2; pl_PL; IdeaTabA1000L-F; Build/JZO54K); G+ SDK");

            string postData = returnRequestPost(userID, circleID, squareID, categoryID);
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(ACTIVITIES_API), content);
            //Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (result["stream"]["update"] != null)
                {
                    foreach (var i in result["stream"]["update"])
                    {
                        posts.posts.Add(_parsePost(i));
                    }
                    posts.pageToken = (string)result["stream"]["continuationToken"];
                }
            }
            return posts;                  
        }

        public static async Task<Post> GetActivity(string activityID)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(ACTIVITY_API), content);
            //Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
                return _parsePost(result["update"]);
            }
            else
            {
                return null;
            }
        }

        public static async Task<PostStream> QueryPost(string queryText, string sort)
        {
            PostStream posts = new PostStream();
            posts.posts = new ObservableCollection<Post>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            //client.DefaultRequestHeaders.Add("User-Agent", "com.google.android.apps.plus/411514804 (Linux; U; Android 4.1.2; pl_PL; IdeaTabA1000L-F; Build/JZO54K); G+ SDK");
            JObject json = new JObject(
                new JProperty("activityRequestData", new JObject(
                   new JProperty("activityFilters", new JObject(
                          new JProperty("maxComments", 0)
//                        new JProperty("updateFilter", new JObject(
//                            new JProperty("includeNamespace", new JArray(
//                                "STREAM",
//                                "EVENT",
//                                "SEARCH",
//                                "PLUSONE",
//                                "PHOTO",
//                                "A2A",
//                                "BIRTHDAY",
//                                "PHOTOS_ADDED_TO_EVENT"
//]

//                    )
                )))),
                new JProperty("searchQuery", new JObject(
                    new JProperty("sort", sort),
                    new JProperty("queryText", queryText),
                    new JProperty("filter", "TACOS")
                ))
            );
            HttpContent content = new StringContent(json.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(SEARCH_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (var i in result["results"]["activityResults"]["stream"]["update"])
                {
                    posts.posts.Add(_parsePost(i));
                }
                posts.pageToken = (string)result["results"]["activityResults"]["stream"]["continuationToken"];
                return posts;
            }
            else
            {
                return null;
            }
        }

    }
}
