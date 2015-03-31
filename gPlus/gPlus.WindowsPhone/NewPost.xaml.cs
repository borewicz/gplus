using gPlus.Classes;
using gPlus.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace gPlus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewPost : Page, IFileOpenPickerContinuable
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        string reshareId, selectedEmoticon, link, imageContent;
        Location.Place place;
        ObservableCollection<string> emoticons;
        List<AclItem> items, selectedItems;
        ShareOperation operation;
        //string imageContent;

        public NewPost()
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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //Other.info
            reshareId = e.NavigationParameter as string;
            items = new List<AclItem>();

            items.Add(new AclItem(null, null, "Public", AclType.Public));
            items.Add(new AclItem(null, null, "Your circles", AclType.YourCircles));
            var circles = await Circles.GetCircles();
            foreach (var c in circles)
                items.Add(new AclItem(c.id, null, c.name, AclType.SpecifiedCircle));
            //aclListBox.ItemsSource = items;
            var squares = await Squares.GetSquares();
            foreach (var c in squares)
                items.Add(new AclItem(c.id, null, c.name, AclType.Square));
            //squaresListBox.ItemsSource = await Squares.GetSquares();
            aclItemFlyout.ItemsSource = items;
            authorAvatar.Source = new BitmapImage(new Uri(Other.info.avatar));
            authorName.Text = Other.info.name;
            emoticons = new ObservableCollection<string>();
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
            if (reshareId != null || e.NavigationParameter != null)
            {
                /*
                 * Line 1: this extracts the data we want from the shared data bundle (ShareOperation.Data) 
                 * sent to our app. In this example we knew we could only receive a hyperlink, so we just went 
                 * and extracted a hyperlink. You can alternately use ShareOperation.Data.Contains() to determine 
                 * if the data you want is present (e.g. if your app can receive different data types).
                 */
                moodButton.IsEnabled = linkButton.IsEnabled = cameraButton.IsEnabled = locationCheckBox.IsEnabled = false;
                operation = (ShareOperation)e.NavigationParameter;
                if (operation.Data.Contains(StandardDataFormats.WebLink))
                    link = operation.Data.GetWebLinkAsync().GetResults().ToString();
            }
            //emoticonsComboBox.ItemsSource = emoticons;
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

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            locationCheckBox.Content = "Locating...";
            place = await Location.GetYourLocation();
            locationCheckBox.Content = place.locationTag;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            place = null;
            locationCheckBox.Content = "Include location";
        }

        private void OnItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            selectedEmoticon = (string)sender.SelectedItem;
            cameraButton.IsChecked = false;
            linkButton.IsChecked = false;
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var picker = new ListPickerFlyout();
            picker.ItemsSource = emoticons;
            picker.SelectionMode = ListPickerFlyoutSelectionMode.Single;
            //picker.ItemTemplate = (DataTemplate)Resources["PickerTemplate"];
            picker.ItemsPicked += OnItemsPicked;
            picker.ShowAt(this);
        }

        private void AppBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedEmoticon = null;
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var result = await PostManagement.PostActivity(contentTextBox.Text, selectedItems, link, selectedEmoticon, reshareId, place, imageContent);
            if (result == 0)
            {
                if (operation != null)
                    operation.ReportCompleted();
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            }
            else
            {
                var messageDialog = new MessageDialog("Cannot into. Contact with developer.");
                await messageDialog.ShowAsync();
            }
        }

        private void ListPickerFlyout_ItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            selectedItems = new List<AclItem>();
            foreach (AclItem item in sender.SelectedItems)
            {
                selectedItems.Add(item);
            }
        }

        private void AppBarToggleButton_Checked_2(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail,
            };

            // Filter to include a sample subset of file types.
            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");
            PickImage(openPicker);
        }

        private void PickImage(FileOpenPicker openPicker)
        {
            openPicker.PickSingleFileAndContinue();
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            var file = args.Files.FirstOrDefault();
            Debug.WriteLine("super");
            if (file == null)
                return;
            imageContent = await Other.StorageFileToBase64(file);
            moodButton.IsChecked = false;
            linkButton.IsChecked = false;
        }
        
        private async void AppBarToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();

            InputScope scope = new InputScope();
            InputScopeName name = new InputScopeName();
            name.NameValue = InputScopeNameValue.Url;
            scope.Names.Add(name);
            TextBox box = new TextBox()
            {
                //Margin = new Thickness(0, 14, 0, -2)
                InputScope = scope,
                Text = "http://"
            };
            TextBlock block = new TextBlock()
            {
                Text = "Tap OK to continue."
            };

            panel.Children.Add(block);
            panel.Children.Add(box);
            var dlg = new ContentDialog()
            {
                Title = "Link",
                Content = panel,
                PrimaryButtonText = "ok",
                SecondaryButtonText = "cancel"
            };

            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                link = box.Text;
                cameraButton.IsChecked = false;
                moodButton.IsChecked = false;
            }
            else
            {
                link = null;
                (sender as AppBarToggleButton).IsChecked = false;
            }
            Debug.WriteLine(result == ContentDialogResult.Primary ? "YES" : "No");
        }

        private void AppBarToggleButton_Unchecked_1(object sender, RoutedEventArgs e)
        {
            link = null;
        }

    }
}
