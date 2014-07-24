using gPlus.Classes;
using gPlus.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace gPlus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var posts = await Posts.GetActivities(null, null, null);
            FontIcon icon = new FontIcon();
            icon.Glyph = (await Notifications.GetNotificationCount()).ToString();
            notificationAppBarButton.Icon = icon;
            itemGridView.ItemsSource = posts.posts;
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NotificationsPage));
            //var result = await Notifications.GetNotifications();
        }

        private void AppBarButton_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            //var result = await Circles.GetCircles();
            //var result2 = await Squares.GetSquares();
            this.Frame.Navigate(typeof(SquaresPage));
        }

        private void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((Posts.Post)e.ClickedItem).postID;
            this.Frame.Navigate(typeof(PostPage), itemId);
        }

        private void AppBarButton_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CirclesPage));
        }

        private void AppBarButton_Tapped_3(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewPostPage));

        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
