using gPlus.Classes;
using gPlus.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http.Filters;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace gPlus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class _2StepPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        CookieContainer cookieJar = new CookieContainer();
        HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();


        public _2StepPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            string parameter = e.NavigationParameter as string;
            try
            {
                Uri baseUri = new Uri(parameter);
                Windows.Web.Http.HttpRequestMessage httpRequestMessage =
                  new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, baseUri);
                //httpRequestMessage.
                webView.NavigateWithHttpRequestMessage(httpRequestMessage);
            }
            catch (Exception oEx)
            {
                // handle exception
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.Uri.ToString().Contains("https://accounts.google.com/o/oauth2/programmatic_auth?"))
            {
                var cookies = filter.CookieManager.GetCookies(new Uri("https://accounts.google.com/o/android/auth"));
                var cookieContainer = new CookieContainer();
                foreach (var i in cookies)
                {
                    cookieContainer.Add(new Uri("https://accounts.google.com/o/oauth2/programmatic_auth"), new Cookie(i.Name, i.Value));
                }
                var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
                HttpClient client = new HttpClient(handler);
                HttpResponseMessage response = await client.GetAsync(args.Uri.ToString());
                //Debug.WriteLine(await response.Content.ReadAsStringAsync());
                string token = (from h in response.Headers
                                 where h.Key == "Set-Cookie"
                                    from i in h.Value
                                    where i.Contains("oauth_token")
                                    select i).First();

                System.Diagnostics.Debug.WriteLine(token.Split(new char[] { '=', ';' })[1]);
                //oAuth.setCredentials(null, null, "oauth2_" + token.Split(new char[] { '=', ';' })[1]);
                oAuth.setStepToken("oauth2_" + token.Split(new char[] { '=', ';' })[1]);
                Other.info = await Other.getInfo();
                if (Other.info != null)
                    this.Frame.Navigate(typeof(MainPage));
                else
                {
                    var dialog = new MessageDialog("Cannot into. Contact with developer.");
                    await dialog.ShowAsync();
                }
                string dupa = null;
                /*
                if (response.IsSuccessStatusCode == true)
                {
                    foreach (var c in response.Headers)
                    {
                        if (c.Key == "Set-Cookie")
                        {
                            var list = c.Value[0].Split(new char[] { '=', ';' });
                            System.Diagnostics.Debug.WriteLine("oauth2_" + list[1]);
                        }
                    }
                }
                 */
            }
        }
    }
}
