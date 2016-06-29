using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Auth0.SDK;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;

namespace AndroidGoogleDrive
{
    public class TokenBasedInitializer : IConfigurableHttpClientInitializer
    {
        private string apiKey;

        public TokenBasedInitializer(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public void Initialize(ConfigurableHttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        }
    }

    [Activity(Label = "AndroidGoogleDrive", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += async delegate
            {
                var auth0 = new Auth0Client("dmitriychernenko.eu.auth0.com", "aqAsxw58VVx3mIbnMieg25xWBXiqcnd2");
                var user = await auth0.LoginAsync(this, "google-oauth2", scope: string.Join(" ", new[] { "openid", DriveService.Scope.Drive, DriveService.Scope.DriveFile }));
                var apiKey = ((JArray)user.Profile["identities"])[0]["access_token"].Value<string>();


                /*// Get tokeninfo for the access token if you want to verify.
                Oauth2Service service = new Oauth2Service(
                    new Google.Apis.Services.BaseClientService.Initializer());
                Oauth2Service.TokeninfoRequest request = service.Tokeninfo();
                request.AccessToken = apiKey;

                Tokeninfo info = request.Execute();
*/



                var init = new BaseClientService.Initializer
                {
                    HttpClientInitializer = new TokenBasedInitializer(apiKey)
                };

                //This also works (doesn’t throw an error anyway)
                DriveService oservice = new DriveService(init);
                //This fails – complaining about invalid token or the like.  The same code sample using 
                //their “file system” approach to authentication in a plain .NET application lists out 
                //the files on my drive.
                FilesResource.ListRequest listRequest = oservice.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";
                IList<Google.Apis.Drive.v3.Data.File> files = null;

                try
                {
                    files = listRequest.Execute().Files;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                var fNames = string.Join(",", listRequest.Execute().Files.Select(f => f.Name.ToString()));
                button.Text = fNames;
            };
        }
    }
}

