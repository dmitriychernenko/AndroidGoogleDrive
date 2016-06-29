using System;
using System.Linq;
using Android.App;
using Android.Widget;
using Android.OS;
using Auth0.SDK;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;

namespace AndroidGoogleDrive
{
    [Activity(Label = "AndroidGoogleDrive", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var button = FindViewById<Button>(Resource.Id.MyButton);
            var listView = FindViewById<ListView>(Resource.Id.FileListView);

            button.Click += async delegate
            {
                try
                {
                    // Clear list view
                    listView.Adapter = null;

                    // Create Auth0 client
                    var auth0Client = new Auth0Client("dmitriychernenko.eu.auth0.com", "aqAsxw58VVx3mIbnMieg25xWBXiqcnd2");

                    // Sign in and get user object
                    var user = await auth0Client.LoginAsync(this, "google-oauth2");

                    // Get access token to talk to Google API
                    var accessToken = ((JArray) user.Profile["identities"])[0]["access_token"].Value<string>();

                    // Create base client service with custom HttpClientInitializer, which uses access token instead of client credentials or static api key
                    var init = new BaseClientService.Initializer
                    {
                        // IMPORTANT: AccessTokenHttpClientInitializer is custom class, which injects Bearer Authorization header into http requests
                        HttpClientInitializer = new AccessTokenHttpClientInitializer(accessToken)
                    };

                    // Retrieve files and populate the list view
                    using (var driveService = new DriveService(init))
                    {
                        var listRequest = driveService.Files.List();
                        var files = listRequest.Execute().Files;
                        listView.Adapter = new ArrayAdapter<string>(this, Resource.Layout.simple_list_item,
                            files?.Select(f => f?.Name ?? "[Untitled]").ToArray());
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            };
        }
    }
}

