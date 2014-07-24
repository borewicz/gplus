using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using gPlus.Classes;
using gPlus.Common;

// The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229

namespace gPlus
{
    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class PostPage : Page
    {
        Posts.Post post;
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


        public PostPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            post = await Posts.GetActivity(e.NavigationParameter as string);
            this.DefaultViewModel["Group"] = post;
            this.DefaultViewModel["Items"] = post.comments;
            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
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

        private async void plusOneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int result = await PostManagement.PlusOne(post.postID, !post.isPlusonedByViewer);
            if (result == 0)
            {
                post.isPlusonedByViewer = !post.isPlusonedByViewer;
                //plusOneButton.IsPressed = !plusOneButton.IsPressed;
            }
        }

        private void AppBarButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewPostPage), post.postID);
        }

        private async void AppBarButton_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            int result = await Comments.PostComment(commentsTextBox.Text, post.postID, post.userID);
        }

        private async void AppBarButton_Tapped_2(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            bool isChecked = ((bool)(sender as AppBarToggleButton).IsChecked);
        	int result = await Comments.PlusOne((string)(sender as AppBarToggleButton).Tag, isChecked);

        }

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Users.User result = await Users.GetSimpleProfile("111399666922347055972");
        }
    }
}
