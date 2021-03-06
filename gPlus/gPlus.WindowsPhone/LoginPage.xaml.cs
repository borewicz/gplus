﻿using gPlus.Classes;
using gPlus.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace gPlus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        ShareOperation operation = null;
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                try
                {
                    operation = (ShareOperation)e.Parameter;
                }
                catch
                {
                    operation = null;
                }
            }
            var username = ApplicationData.Current.LocalSettings.Values["username"];
            var password = ApplicationData.Current.LocalSettings.Values["password"];
            var token = ApplicationData.Current.LocalSettings.Values["token"];

            if (token != null)
                login((string)username, (string)password, (string)token);

            if ((username != null) && (password != null))
            {
                usernameTextBox.Text = (string)username;
                passwordBox.Password = (string)password;
            }

        }

        private void AppBarButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            login(usernameTextBox.Text, passwordBox.Password, null);
        }

        private async void login(string username, string password, string token)
        {
            oAuth.setCredentials(username, password, token);
            string result = await oAuth.GetAccessToken();
            if (result != null)
            {
                if (result.Contains("Continue"))
                {
                    this.Frame.Navigate(typeof(_2StepPage), result);
                }
                else
                {
                    Other.info = await Other.getInfo();
                    if (Other.info != null)
                    {
                        if (operation == null)
                            this.Frame.Navigate(typeof(MainPage));
                        else
                            this.Frame.Navigate(typeof(NewPost), operation);
                    }
                    /*
                    else
                    {
                        var dialog = new MessageDialog("Cannot into. Contact with developer.");
                        await dialog.ShowAsync();
                    }
                     */
                }
            }
            else
            {
                var dialog = new MessageDialog("Cannot into. Contact with developer.");
                await dialog.ShowAsync();
            }
        }
    }
}
