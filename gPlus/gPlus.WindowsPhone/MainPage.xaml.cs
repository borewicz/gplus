﻿using gPlus.Classes;
using gPlus.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
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

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace gPlus
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        public ObservableCollection<Command> commands = new ObservableCollection<Command>();

        public MainPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            commands.Add(new Command("me", Other.info.userID, CommandType.USER));
            commands.Add(new Command("Znajomi".ToLower(), "251e214c09316529", CommandType.CIRCLE));
            commands.Add(new Command("all", null, CommandType.CIRCLE));
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
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //if (Frame.BackStack.Count != 0 && Frame.BackStack.First().SourcePageType == typeof(LoginPage))
            //    Frame.BackStack.Remove(Frame.BackStack.First());
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            //var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            //this.DefaultViewModel["Groups"] = sampleDataGroups;
            //var posts = await Posts.GetActivities(null, null, null);
            //FontIcon icon = new FontIcon();
            //icon.Glyph = (await Notifications.GetNotificationCount()).ToString();
            //notificationAppBarButton.Icon = icon;
            //itemGridView.ItemsSource = posts.posts;
            var squares = await Squares.GetSquares();
            //communitiesGridView.Source = squares;
            communitiesHub.DataContext = squares;
            var circles = await Circles.GetCircles();
            circlesHub.DataContext = circles;
            FontIcon icon = new FontIcon();
            icon.Glyph = (await Notifications.GetNotificationCount()).ToString();
            notificationAppBarButton.Icon = icon;
            favouritesHub.DataContext = commands;
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
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Shows the details of a clicked group in the <see cref="SectionPage"/>.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Details about the click event.</param>
        private void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
            /*
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(SectionPage), groupId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
             */
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Defaults about the click event.</param>
        private void circlesItemClick(object sender, ItemClickEventArgs e)
        {
            var id = ((Circles.Circle)e.ClickedItem).id;
            this.Frame.Navigate(typeof(PostsPage), id);
        }

        private void squaresItemClick(object sender, ItemClickEventArgs e)
        {
            var id = ((Squares.Square)e.ClickedItem).id;
            //this.Frame.Navigate(typeof(PostsPage), "SQUARE:" + id);
            this.Frame.Navigate(typeof(CategoriesPage), id);
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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewPost), null);
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();
            TextBox box = new TextBox()
            {
                //Margin = new Thickness(0, 14, 0, -2)
                //Text = comment.originalTex
                PlaceholderText = "enter query"
            };
            TextBlock block = new TextBlock()
            {
                Text = "Type:"
            };

            RadioButton button1 = new RadioButton()
            {
                Content = "Recent",
                IsChecked = true
            };

            RadioButton button2 = new RadioButton()
            {
                Content = "Best"
            };

            //panel.Children.Add(block);
            panel.Children.Add(box);
            panel.Children.Add(block);
            panel.Children.Add(button1);
            panel.Children.Add(button2);

            var dlg = new ContentDialog()
            {
                Title = "Find posts",
                Content = panel,
                PrimaryButtonText = "search",
                SecondaryButtonText = "cancel"
            };

            var dlgResult = await dlg.ShowAsync();
            
            if (dlgResult == ContentDialogResult.Primary)
            {
                if (button1.IsChecked == true)
                    this.Frame.Navigate(typeof(PostsPage), "SEARCH:RECENT:" + box.Text);
                else
                    this.Frame.Navigate(typeof(PostsPage), "SEARCH:BEST:" + box.Text);
                //int result = await Comments.EditComment(box.Text, comment.commentID, post.postID);
                //if (result == 0)
                //{
                    //post = await Posts.GetActivity(post.postID);
                    //this.DefaultViewModel["Item"] = post;
                //}
            }
        //}
            //var posts = await Posts.QueryPost("XD", "BEST");
            //Debug.WriteLine(posts);
        }

        private void notificationAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NotificationsPage), null);
        }

        private void signOutClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            oAuth.clearCredentials();
        }

        private async void favoutitesItemClick(object sender, ItemClickEventArgs e)
        {
            var command = ((Command)e.ClickedItem);
            switch (command.type)
            {
                case CommandType.USER: Frame.Navigate(typeof(Profile), command.arg);
                    break;
                case CommandType.CIRCLE: Frame.Navigate(typeof(PostsPage), command.arg);
                    break;
                default:
                    MessageDialog msgbox = new MessageDialog("not implemented");
                    await msgbox.ShowAsync();
                    break;
            }
        }

    }
}