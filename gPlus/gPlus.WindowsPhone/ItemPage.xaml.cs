using gPlus.Common;
using gPlus.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace gPlus
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        Posts.Post post;

        public ItemPage()
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
        /// Gets the view model for this <see cref="Page"/>. This can be changed to a strongly typed view model.
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
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            //var item = await SampleDataSource.GetItemAsync((string)e.NavigationParameter);
            string parameter = e.NavigationParameter as string;
            post = await Posts.GetActivity(parameter);
            this.DefaultViewModel["Item"] = post;
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

        private async void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            int result = await PostManagement.PlusOne(post.postID, !post.isPlusonedByViewer);
            if (result == 0)
                post.isPlusonedByViewer = true;
            else
                //tu error
                (sender as AppBarToggleButton).IsChecked = false;
        }

        private async void AppBarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            int result = await PostManagement.PlusOne(post.postID, !post.isPlusonedByViewer);
            if (result == 0)
                post.isPlusonedByViewer = false;
            else
                //tu error
                (sender as AppBarToggleButton).IsChecked = true;
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            int result = await Comments.PostComment(commentTextBox.Text, post.postID, Other.info.userID);
            if (result == 0)
            {
                post = await Posts.GetActivity(post.postID);
                this.DefaultViewModel["Item"] = post;
            }
            
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewPost), post.postID);
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);  
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            //int result;
            if (item != null)
            {
                Posts.Post.Comment comment = item.DataContext as Posts.Post.Comment;

                if (comment != null)
                {
                    /*
                     *                                             <MenuFlyoutItem Text="+1" Tag="+1" 
                        DataContext="{Binding}" Click="MenuFlyoutItem_Click" />
                                            <MenuFlyoutItem Text="reply" Tag="reply" 
                        DataContext="{Binding}" Click="MenuFlyoutItem_Click"/>
                                            <MenuFlyoutItem Text="edit" Tag="edit" 
                        DataContext="{Binding}" Click="MenuFlyoutItem_Click" />
                                            <MenuFlyoutItem Text="remove" Tag="remove" 
                        DataContext="{Binding}" Click="MenuFlyoutItem_Click"/>
                     */
                    if (item.Tag.ToString() == "+1")
                    {
                        int result = await Comments.PlusOne(comment.commentID, !comment.isPlusonedByViewer);
                        if (result == 0)
                        {
                            post = await Posts.GetActivity(post.postID);
                            this.DefaultViewModel["Item"] = post;     
                            //to potem się recznie zrobi
                        }
                    }
                    else if (item.Tag.ToString() == "reply")
                        commentTextBox.Text += "@" + comment.userID;                      
                    else if (item.Tag.ToString() == "edit")
                    {
                        StackPanel panel = new StackPanel();
                        TextBox box = new TextBox()
                        {
                            //Margin = new Thickness(0, 14, 0, -2)
                            Text = comment.originalText
                        };
                        TextBlock block = new TextBlock()
                        {
                            Text = "Tap OK to continue."
                        };

                        panel.Children.Add(block);
                        panel.Children.Add(box);
                        var dlg = new ContentDialog()
                        {
                            Title = "Edit comment",
                            Content = panel,
                            PrimaryButtonText = "ok",
                            SecondaryButtonText = "cancel"
                        };

                        var dlgResult = await dlg.ShowAsync();
                        if (dlgResult == ContentDialogResult.Primary)
                        {
                            int result = await Comments.EditComment(box.Text, comment.commentID, post.postID);
                            if (result == 0)
                            {
                                post = await Posts.GetActivity(post.postID);
                                this.DefaultViewModel["Item"] = post;
                            }
                        }
                    }
                    else if (item.Tag.ToString() == "remove")
                    {
                        var dialog = new MessageDialog("", "");
                        var dlg = new ContentDialog()
                        {
                            Title = "Warning",
                            Content = "Do you really want to delete this comment?",
                            PrimaryButtonText = "yes",
                            SecondaryButtonText = "no"
                        };
                        var dlgResult = await dlg.ShowAsync();
                        if (dlgResult == ContentDialogResult.Primary)
                        {
                            int result = await Comments.DeleteComment(comment.commentID);
                            if (result == 0)
                            {
                                //post = await Posts.GetActivity(post.postID); //
                                post.comments.Remove(comment);
                                this.DefaultViewModel["Item"] = post;
                            }
                        }
                    }
                    //else img.Stretch = Stretch.Uniform;
                }
            }
        }

        private async void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            StackPanel panel = new StackPanel();
            TextBox box = new TextBox()
            {
                //Margin = new Thickness(0, 14, 0, -2)
                Text = post.content
            };
            TextBlock block = new TextBlock()
            {
                Text = "Tap OK to continue.",
                TextWrapping = TextWrapping.Wrap
            };

            panel.Children.Add(block);
            panel.Children.Add(box);
            var dlg = new ContentDialog()
            {
                Title = "Edit post",
                Content = panel,
                PrimaryButtonText = "ok",
                SecondaryButtonText = "cancel"
            };

            var dlgResult = await dlg.ShowAsync();
            if (dlgResult == ContentDialogResult.Primary)
            {
                int result = await PostManagement.EditActivity(post.postID, box.Text);
                if (result == 0)
                {
                    post = await Posts.GetActivity(post.postID);
                    this.DefaultViewModel["Item"] = post;
                }
            }
        }

        private async void AppBarButton_Click_3(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("", "");
            var dlg = new ContentDialog()
            {
                Title = "Warning",
                Content = "Do you really want to delete this post?",
                PrimaryButtonText = "yes",
                SecondaryButtonText = "no"
            };
            var dlgResult = await dlg.ShowAsync();
            if (dlgResult == ContentDialogResult.Primary)
            {
                int result = await PostManagement.DeleteActivity(post.postID);
                if (result == 0)
                {
                    //post = await Posts.GetActivity(post.postID); //
                    this.Frame.GoBack();
                }
            }
        }

        private async void AppBarButton_Click_4(object sender, RoutedEventArgs e)
        {
            int result = await PostManagement.ReportAbuse(post.postID);
            if (result == 0)
            {
                MessageDialog dlg = new MessageDialog("Post successfully reported", "Success!");
                await dlg.ShowAsync();
                //post = await Posts.GetActivity(post.postID); //
            }
        }

    }
}