using gPlus.Classes;
using gPlus.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace gPlus
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class NewPostPage : Page
    {
        string reshareId;

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


        public NewPostPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            reshareId = e.NavigationParameter as string;
            List<AclItem> items = new List<AclItem>();
            items.Add(new AclItem(null, null, "Public", AclType.Public));
            items.Add(new AclItem(null, null, "Your circles", AclType.YourCircles));
            var circles = await Circles.GetCircles();
            foreach (var c in circles)
                items.Add(new AclItem(c.id, null, c.name, AclType.SpecifiedCircle));
            aclListBox.ItemsSource = items;

            var squares = await Squares.GetSquares();
            foreach (var c in squares)
                items.Add(new AclItem(c.id, null, c.name, AclType.Square));
            squaresListBox.ItemsSource = await Squares.GetSquares();
            ObservableCollection<string> emoticons = new ObservableCollection<string>();
            emoticons.Add("frustrated");
            emoticons.Add("crying");
            emoticons.Add("happy");
            emoticons.Add("angry");
            emoticons.Add("scared");
            emoticons.Add("joy");
            emoticons.Add("kiss");
            emoticons.Add("lol");
            emoticons.Add("meh");
            emoticons.Add("sad");
            emoticons.Add("sick");
            emoticons.Add("silly");
            emoticons.Add("smirk");
            emoticons.Add("wink");
            emoticons.Add("worried");
            emoticons.Add("winter");
            emoticons.Add("new_year");
            emoticons.Add("valentines_day");
            emoticons.Add("st_paddys_day");
            emoticons.Add("spring");
            emoticonsComboBox.ItemsSource = emoticons;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        private void AppBarButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            List<AclItem> items = new List<AclItem>();
            if (toggleSwitch.IsOn)
            {
                items.Add(new AclItem(((Squares.Square)squaresListBox.SelectedItem).id, ((Squares.Square.Category)categoriesListBox.SelectedItem).id, ((Squares.Square)squaresListBox.SelectedItem).name, AclType.Square));
            }
            else
            {
                foreach (AclItem item in aclListBox.SelectedItems)
                {
                    items.Add(item);
                }
            }
            //var id = ((Circles.Circle)aclListBox.SelectedItem).id;
            string emoticon = (string)emoticonsComboBox.SelectedItem;
            //var result = await PostManagement.PostActivity(contentTextBox.Text, items, linkTextBox.Text, emoticon, reshareId, await Location.GetYourLocation());
        }

        private async void squaresListBox_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (squaresListBox.SelectedIndex != -1)
            {
                categoriesListBox.ItemsSource = await Squares.GetCategories(((Squares.Square)squaresListBox.SelectedItem).id);
            }
        }

        private void ToggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (toggleSwitch.IsOn)
            {
                categoriesListBox.IsEnabled = true;
                squaresListBox.IsEnabled = true;
                aclListBox.IsEnabled = false;
            }
            else
            {
                categoriesListBox.IsEnabled = false;
                squaresListBox.IsEnabled = false;
                aclListBox.IsEnabled = true;
            }
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
