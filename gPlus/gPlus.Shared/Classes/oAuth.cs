using gPlus.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace gPlus.Classes
{
    public static class oAuth
    {
        const string REDIRECT_URI = "https://developers.google.com/oauthplayground";
        const string CLIENT_ID = "407408718192.apps.googleusercontent.com";
        const string OAUTH_URI = "https://accounts.google.com/o/oauth2/auth";
        const string SCOPE = "https://www.googleapis.com/auth/plus.me+https://www.googleapis.com/auth/plus.stream.read+https://www.googleapis.com/auth/plus.stream.write+https://www.googleapis.com/auth/plus.circles.write+https://www.googleapis.com/auth/plus.circles.read+https://www.googleapis.com/auth/plus.photos.readwrite";

        static string access_token, username, token, password, stepToken;

        static string GetAuthUri()
        {
            return OAUTH_URI +
               "?redirect_uri=" + REDIRECT_URI +
               "&response_type=code" +
               "&client_id=" + CLIENT_ID +
               "&scope=" + SCOPE +
               "&approval_prompt=force" +
               "&access_type=offline";
        }

        async static Task<string> ConnectToGoogle()
        {
            string code;
            System.Uri StartUri = new Uri(GetAuthUri());
            Debug.WriteLine(StartUri.AbsoluteUri);
            System.Uri EndUri = new Uri(REDIRECT_URI);

            WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                    WebAuthenticationOptions.None,
                                                    StartUri,
                                                    EndUri);

            code = WebAuthenticationResult.ResponseData.ToString().Split('=')[1];
            return code;
        }

        public static void setStepToken(string stepToken)
        {
            oAuth.stepToken = stepToken;
            token = null;
        }

        public static void setCredentials(string username, string password, string token)
        {
            if (username != null || password != null)
            {
                oAuth.username = username;
                oAuth.password = password;
            }
            oAuth.token = token;
        }

        public static void saveCredentials(string username, string password, string token)
        {
            ApplicationData.Current.LocalSettings.Values["token"] = token;
            ApplicationData.Current.LocalSettings.Values["password"] = password;
            ApplicationData.Current.LocalSettings.Values["username"] = username;
            ApplicationData.Current.LocalSettings.Values["stepToken"] = stepToken;
        }

        public static void clearCredentials()
        {
            ApplicationData.Current.LocalSettings.Values["token"] = null;
            ApplicationData.Current.LocalSettings.Values["password"] = null;
            ApplicationData.Current.LocalSettings.Values["username"] = null;
            ApplicationData.Current.LocalSettings.Values["stepToken"] = null;
            token = username = password = access_token = stepToken = null;
        }

        public async static Task<string> GetAccessToken()
        {
            if (access_token != null)
                return access_token;
            //clearCredentials();

            //var settings = ApplicationData.Current.LocalSettings;
            //string token = "";

            //var tokenValue = settings.Values["token"];
            //object tokenValue = null;
            //var username = settings.Values["username"];
            //var password = settings.Values["password"];

            //if ((username == null) || (password == null))
            //{
                //tutaj pytaj o usera i hasło
            //}
            if (oAuth.token == null || oAuth.stepToken != null)
            {
                oAuth.token = await getToken((string)username, (string)password);
                if (oAuth.token.Contains("ContinueSignIn"))
                    return oAuth.token;
                else if (oAuth.token == null)
                //    ApplicationData.Current.LocalSettings.Values["token"] = (string)token;
                //else`
                    return null;
            }
            //saveCredentials(oAuth.username, oAuth.password, oAuth.token);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "GoogleLoginService/1.3");

            string postData = "device_country=pl" +
                "&accountType=HOSTED_OR_GOOGLE" +
                "&Email=" + username +
                "&service=oauth2:" + SCOPE +
                "&source=android" +
                "&app=com.google.android.gms" +
                "&client_sig=38918a453d07199354f8b19af05ec6562ced5788" +
                "&callerPkg=com.google.android.gms" +
                "&callerSig=38918a453d07199354f8b19af05ec6562ced5788" +
                //"&Passwd=" + "Dupa1234";
                "&Token=" + token;
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await client.PostAsync(new Uri("https://android.clients.google.com/auth"), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                string result = await response.Content.ReadAsStringAsync();
                Dictionary<string, string> values =
                result.Split('\n')
                    .Select(x => x.Split('='))
                    .ToDictionary(y => y[0], y => y[1]);
                //return values["Auth"];
                access_token = values["Auth"];
                saveCredentials(username, password, token);
                return access_token;
            }
            else
                return null;
        }

        public static async Task<string> getToken(string username, string password)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "GoogleLoginService/1.3");

            string postData = "accountType=HOSTED_OR_GOOGLE" +
                //"&has_permission=1" + 
               "&add_account=1" +
                //"&EncryptedPasswd=" + encrypt(username, password) + 
               "&service=ac2dm" +
               "&source=android" +
               "&androidId=31e0e7f6a1fa7484" +
                //"&device_country=pl" + 
                //"&operatorCountry=pl" + 
               "&lang=pl";// + 
            if (stepToken == null)
                postData += "&Email=" + username +
               "&Passwd=" + password;
            else
                postData += "&Token=" + stepToken +
                    "&ACCESS_TOKEN=1";
            //"&sdk_version=16";
            //string postData = "{\"activityId\":\"" + activityID + "\"}"; 
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await client.PostAsync(new Uri("https://android.clients.google.com/auth"), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());

            //string token =;
            if (response.IsSuccessStatusCode == true)
            {
                string result = await response.Content.ReadAsStringAsync();
                foreach (var s in result.Split('\n'))
                {
                    var key = s.Split('=');
                    if (key[0] == "Token")
                        return key[1];
                }
                return null;
                /*
                Dictionary<string, string> values =
                    result.Split('\n')
                    .Select(x => x.Split('='))
                    .ToDictionary(y => y[0], y => y[1]);
                //return await _getAuthVar(username, values["Token"]);
                return values["Token"];
                 */
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                string result = await response.Content.ReadAsStringAsync();
                foreach (var x in result.Split('\n'))
                {
                    //Debug.WriteLine(x);
                    if (x.Split(new char[] { '=' }, 2)[0] == "Url")
                        return x.Split(new char[] { '=' }, 2)[1];
                }
                return null;
                /*
                Dictionary<string, string> values =
                    result.Split('\n')
                    .Select(x => x.Split(new char[] { '=' }, 2))
                    .ToDictionary(y => y[0], y => y[1]);
                return values["Url"];
                 */
            }
            else
                return null;
        }


        public async static Task<string> getPlaygroundAccessToken() //DEPR coś tam
        {
            if (access_token != null)
                return access_token;
            var settings = ApplicationData.Current.LocalSettings;

            HttpClient client = new HttpClient();
            JObject result;
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");
            var value = settings.Values["refresh_token"];
            if (value == null)
            {
                JObject o = new JObject();
                o["token_uri"] = "https://accounts.google.com/o/oauth2/token";
                o["code"] = await ConnectToGoogle();

                try
                {
                    HttpContent content = new StringContent(o.ToString());
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage response = await client.PostAsync("https://developers.google.com/oauthplayground/exchangeAuthCode", content);
                    if (response.IsSuccessStatusCode == true)
                    {
                        result = JObject.Parse(await response.Content.ReadAsStringAsync());
                        //Debug.WriteLine();
                        ApplicationData.Current.LocalSettings.Values["refresh_token"] = (string)result["refresh_token"];
                        access_token = (string)result["access_token"];
                        return (string)result["access_token"];
                      
                    }
                    else
                    {
                        return "error";
                    }
                }
                catch
                {
                    return "error";
                }
            }
            else
            {
                JObject o = new JObject();
                o["token_uri"] = "https://accounts.google.com/o/oauth2/token";
                o["refresh_token"] = settings.Values["refresh_token"].ToString();

                try
                {
                    HttpContent content = new StringContent(o.ToString());
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    HttpResponseMessage response = await client.PostAsync("https://developers.google.com/oauthplayground/refreshAccessToken", content);
                    //string result = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode == true)
                    {
                        result = JObject.Parse(await response.Content.ReadAsStringAsync());
                        access_token = (string)result["access_token"];
                        //access_token = (string)result["access_token"];
                        //Debug.WriteLine((string)result["refresh_token"]);
                        if ((string)result["access_token"] == null)
                        {
                            settings.Values["refresh_token"] = null;
                            return await GetAccessToken();
                        }
                        else
                            return (string)result["access_token"];
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
             
        }
    }
}
