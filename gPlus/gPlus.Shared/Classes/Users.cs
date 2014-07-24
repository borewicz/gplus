using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace gPlus.Classes
{
    static class Users
    {
        const string HOST = "https://www.googleapis.com";
        const string PROFILE_API = HOST + "/plusi/v2/ozInternal/getsimpleprofile";

        public enum IM
        {
            NetMeeting = 10,
            AIM = 2,
            GTalk = 7,
            ICQ = 8,
            Jabber = 9,
            MSN = 3,
            QQ = 6,
            Skype = 5,
            Yahoo = 4
        }

        public enum Gender
        {
            NoSpecified = 0,
            Male = 1,
            Female = 2,
            Other = 3
        }

        public enum Relationship
        {
            DontSay = 0,
            Single = 2,
            InRelationship = 3,
            Engaged = 4,
            Married = 5,
            Complicated = 6,
            OpenRelationship = 7,
            Widowed = 8,
            DomesticPartnership = 9,
            CivilUnion = 10
        }

        public enum LookingFor
        {
            Networking = 5,
            Relationship = 4,
            Dating = 3,
            Friends = 2
        }
	
        public class User
        {
            public struct HistoryElement
            {
                public string name { get; set; }
                public string title { get; set; }
                public string start { get; set; }
                public string end { get; set; }
                public string description { get; set; }
                public bool current { get; set; }
            }
            public struct Link
            {
                public string link { get; set; }
                public string faviconUrl { get; set; }
                public string label { get; set; }
                public string rel { get; set; }
            }
            public struct Follower { public string id, name, avatar; }
            /*
            public class Contact
            {
                public List<string> telephone = new List<string>();
                public List<string> cellphone = new List<string>();
                public List<string> email = new List<string>();
                public List<string> fax = new List<string>();
                public List<string> pager = new List<string>();
                public List<string> address = new List<string>();
                public Dictionary<string, IM> chat = new Dictionary<string, IM>();
            }
             */
            //basic
            public string userId, profileUrl;
            public string name { get; set; }
            public string given { get; set; }
            public string family { get; set; }

            public string avatarUrl { get; set; }
            public string backgroundUrl { get; set; }

            //history
            public string tagline { get; set; }
            public string intro { get; set; }
            public string braggingRights { get; set; }

            //education
            private List<HistoryElement> _education = new List<HistoryElement>();
            public List<HistoryElement> education { get { return _education; } }

            //some heavy stuff
            public string birthday { get; set; }
            public string gender { get; set; }
            public string relationship { get; set; }
            //private List<LookingFor> _lookingFor = new List<LookingFor>();
            //public List<LookingFor> lookingFor { get { return _lookingFor; } }
            public List<string> lookingFor = new List<string>();
            private List<string> _nicks = new List<string>();
            public List<string> nicks { get { return _nicks; } }

            //fapping
            public string occupation { get; set; }
            public string skills { get; set; }
            private List<HistoryElement> _employment = new List<HistoryElement>();
            public List<HistoryElement> employment { get { return _employment; } }

            public string currentLocation { get; set; }
            public string mapUrl { get; set; }
            private List<string> _olderLocation = new List<string>();
            public List<string> olderLocation { get { return _olderLocation; } }

            //contact
            //public Contact workContact = new Contact();
            //public Contact homeContact = new Contact();
            public class Contact {
                public string value, type, protocol;
            }

            public List<Contact> phone = new List<Contact>();
            public List<Contact> address = new List<Contact>();
            public List<Contact> IM = new List<Contact>();

            //a lot of spam links
            private List<Link> _otherProfiles = new List<Link>();
            private List<Link> _contributeTo = new List<Link>();
            private List<Link> _links = new List<Link>();
            public List<Link> otherProfiles { get { return _otherProfiles; } }
            public List<Link> contributeTo { get { return _contributeTo; } }
            public List<Link> links { get { return _links; } }
        }

        private static User _parseUser(JObject data)
        {
            User user = new User();
            JObject content = (JObject)data["profile"]["content"];
            JObject userData = (JObject)data["profile"]["user"];

            user.userId = (string)data["profile"]["obfuscatedGaiaId"];
            user.name = (string)data["profile"]["displayName"];
            user.given = (string)userData["name"]["given"];
            user.family = (string)userData["name"]["family"];
            user.avatarUrl = Other.parseLink((string)content["photoUrl"]);
            user.gender = (string)userData["gender"]["value"];

            foreach (var nick in (JArray)userData["otherNames"]["name"])
                user.nicks.Add((string)nick["value"]);

            user.occupation = (string)userData["occupation"]["value"];
            user.skills = (string)userData["skills"]["value"];
            user.braggingRights = (string)userData["braggingRights"]["value"];
            user.tagline = (string)content["tagLine"]["value"];
            user.intro = (string)content["introduction"]["value"];

            foreach (var employment in userData["employments"]["employment"])
            {
                User.HistoryElement element = new User.HistoryElement();
                element.name = (string)employment["employer"];
                element.title = (string)employment["title"];
                if (employment["dateInfo"] != null)
                {
                    if (employment["dateInfo"]["end"] != null)
                        element.end = ((int)employment["dateInfo"]["end"]["day"]).ToString() + "-" +
                            ((int)employment["dateInfo"]["end"]["month"]).ToString() + "-" +
                            ((int)employment["dateInfo"]["end"]["year"]).ToString();
                    if (employment["dateInfo"]["start"] != null)
                        element.start = ((int)employment["dateInfo"]["start"]["day"]).ToString() + "-" +
                            ((int)employment["dateInfo"]["start"]["month"]).ToString() + "-" +
                            ((int)employment["dateInfo"]["start"]["year"]).ToString();
                }
                element.description = (string)employment["description"];
                user.employment.Add(element);
            }

            foreach (var school in userData["educations"]["education"])
            {
                User.HistoryElement element = new User.HistoryElement();
                element.name = (string)school["school"];
                element.title = (string)school["majorConcentration"];
                element.description = (string)school["description"];

                if (school["dateInfo"] != null)
                {
                    if (school["dateInfo"]["end"] != null)
                        element.end = ((int)school["dateInfo"]["end"]["day"]).ToString() + "-" +
                            ((int)school["dateInfo"]["end"]["month"]).ToString() + "-" +
                            ((int)school["dateInfo"]["end"]["year"]).ToString();
                    if (school["dateInfo"]["start"] != null)
                        element.start = ((int)school["dateInfo"]["start"]["day"]).ToString() + "-" +
                            ((int)school["dateInfo"]["start"]["month"]).ToString() + "-" +
                            ((int)school["dateInfo"]["start"]["year"]).ToString();
                }
                user.education.Add(element);
                
            }

            if (userData["locations"] != null)
            {
                user.mapUrl = Other.parseLink((string)userData["locations"]["locationMapUrl"]);
                user.currentLocation = (string)userData["locations"]["currentLocation"];
                foreach (var other in userData["locations"]["otherLocation"])
                    user.olderLocation.Add((string)other);
            }

            foreach (var l in content["links"]["link"])
            {
                User.Link link = new User.Link();
                link.link = (string)(l["normalizedUri"].First());
                link.faviconUrl = Other.parseLink((string)l["faviconImgUrl"]);
                link.label = (string)l["label"];
                link.rel = (string)l["rel"];
                //link.type = (string)l[]
                switch ((string)l["type"])
                {
                    case "ME" : user.links.Add(link);
                        break;
                    case "CONTRIBUTOR": user.contributeTo.Add(link);
                        break;
                    case "OTHER": user.otherProfiles.Add(link);
                        break;    
                }
                user.otherProfiles.Add(link);
            }

            user.backgroundUrl = (string)content["scrapbook"]["defaultCoverPhotoUrl"];

            foreach (var item in content["contacts"]["phone"])
            {
                User.Contact contact = new User.Contact();
                contact.value = (string)item["value"];
                contact.type = (string)item["type"];
                user.phone.Add(contact);
            }
            foreach (var item in content["contacts"]["address"])
            {
                User.Contact address = new User.Contact();
                address.value = (string)item["value"];
                user.address.Add(address);
            }
            foreach (var item in content["contacts"]["instantMessage"])
            {
                User.Contact im = new User.Contact();
                im.value = (string)item["value"];
                //contact.type = (string)item["type"];
                im.protocol = (string)item["protocol"];
                user.IM.Add(im);
            }

            user.relationship = (string)userData["relationshipStatus"]["value"];

            foreach (var interest in userData["relationshipInterests"]["interest"])
                user.lookingFor.Add((string)interest["value"]);
            return user;
        }
/*
            //========================================
            if (data[2][22].HasValues) user.relationship = (Relationship)Convert.ToInt32(data[2][22][1].ToString());
            foreach (var element in data[2][23][1])
                user.lookingFor.Add((LookingFor)Convert.ToInt32(element[0].ToString()));
        }
         */

        public static async Task<User> GetSimpleProfile(string userID)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\"ownerId\":\"" + userID + "\"}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(PROFILE_API), content);
            //Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                //string result = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
                return _parseUser(result);
                //return 0;
            }
            else
            {
                return null;
            }
        }
    }
}
