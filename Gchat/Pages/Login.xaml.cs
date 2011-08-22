﻿using System;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using Gchat.Utilities;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows;

namespace Gchat.Pages {
    public partial class Login : PhoneApplicationPage {
        private readonly IsolatedStorageSettings settings;

        // Constructor
        public Login() {
            InitializeComponent();

            settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("username")) {
                Username.Text = settings["username"] as string;
            }

            if (settings.Contains("password")) {
                var passBytes = ProtectedData.Unprotect(settings["password"] as byte[], null);
                Password.Password = Encoding.UTF8.GetString(passBytes, 0, passBytes.Length);
            }
        }

        private void Login_Click(object sender, EventArgs e) {
            if (settings.Contains("username") && ((string) settings["username"]) == Username.Text &&
                (settings.Contains("auth") || (settings.Contains("token") && settings.Contains("rootUrl")))) {
                NavigationService.GoBack();
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            Username.IsEnabled = false;
            Password.IsEnabled = false;
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

            settings["username"] = Username.Text;
            settings["password"] = ProtectedData.Protect(Encoding.UTF8.GetBytes(Password.Password), null);
            settings.Save();

            GoogleTalkHelper.GoogleLogin(
                Username.Text,
                Password.Password,
                token =>
                Dispatcher.BeginInvoke(() => {
                    settings["auth"] =
                        ProtectedData.Protect(
                            Encoding.UTF8.GetBytes(token), null
                        );
                    settings.Save();

                    NavigationService.GoBack();
                }),
                error =>
                Dispatcher.BeginInvoke(() => {
                    MessageBox.Show("Authentication error: " + error);

                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressBar.IsIndeterminate = false;
                    Username.IsEnabled = true;
                    Password.IsEnabled = true;
                    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                })
            );
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            throw new QuitException();
        }

        private void Password_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == System.Windows.Input.Key.Enter) {
                Focus();
                Login_Click(sender, e);
            }
        }
    }
}