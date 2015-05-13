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
using Windows.Devices.Geolocation;

namespace gPlus.Classes
{
    public static class Location
    {
        public class Place
        {
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string locationTag { get; set; }
            public string bestAddress { get; set; }
            public string clusterId { get; set; }
        }

        const string HOST = "https://www.googleapis.com";
        const string PLACES_API = HOST + "/plusi/v2/ozInternal/snaptoplace";

        public static async Task<Place> GetYourLocation()
        {
            Geolocator locator = new Geolocator();
            locator.DesiredAccuracy = PositionAccuracy.High;
            var location = await locator.GetGeopositionAsync().AsTask();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\"latitudeE7\":" + String.Format("{0:0.0000000}", location.Coordinate.Point.Position.Latitude).Replace(".", null) + ",\"longitudeE7\":" + String.Format("{0:0.0000000}", location.Coordinate.Point.Position.Longitude).Replace(".", null) + ",\"precisionMeters\":46.39099884033203}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(PLACES_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                Place place = new Place();
                place.latitude = (string)result["preciseLocation"]["location"]["latitudeE7"];
                place.longitude = (string)result["preciseLocation"]["location"]["longitudeE7"];
                place.locationTag = (string)result["preciseLocation"]["location"]["locationTag"];
                return place;
            }
            else
                return null;
        }

        public static async Task<ObservableCollection<Place>> GetPlaces()
        {
            ObservableCollection<Place> places = new ObservableCollection<Place>();
            Geolocator locator = new Geolocator();
            locator.DesiredAccuracy = PositionAccuracy.High;
            var location = await locator.GetGeopositionAsync().AsTask();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse("Bearer " + await oAuth.GetAccessToken());
            client.DefaultRequestHeaders.Add("User-Agent", "google-oauth-playground");

            string postData = "{\"latitudeE7\":" + String.Format("{0:0.0000000}", location.Coordinate.Point.Position.Latitude).Replace(".", null) + ",\"longitudeE7\":" + String.Format("{0:0.0000000}", location.Coordinate.Point.Position.Longitude).Replace(".", null) + ",\"precisionMeters\":46.39099884033203}";
            HttpContent content = new StringContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(new Uri(PLACES_API), content);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode == true)
            {
                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (var p in result["localPlace"])
                {
                    Place place = new Place();
                    place.bestAddress = (string)p["location"]["bestAddress"];
                    place.clusterId = (string)p["location"]["clusterId"];
                    place.latitude = (string)p["location"]["latitudeE7"];
                    place.longitude = (string)p["location"]["longitudeE7"];
                    place.locationTag = (string)p["location"]["locationTag"];
                    places.Add(place);
                }
                return places;
            }
            else
                return null;
        }
    }
}
