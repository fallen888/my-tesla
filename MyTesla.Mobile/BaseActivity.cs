using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Views;
using Android.Support.V7.App;
using V7 = Android.Support.V7.Widget;
using Android.Animation;
using Android.Views.InputMethods;
using Android.Content.PM;
using Android.Graphics;
using System.Net;

namespace MyTesla.Mobile
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class BaseActivity : AppCompatActivity
    {
        protected PrefHelper _prefHelper = null;
        protected TeslaAPI _teslaAPI = null;

        protected View _view = null;
        protected ViewStates _toVisibility = ViewStates.Gone;

        protected string AccessToken
        {
            get
            {
                return _prefHelper.GetPrefString(Constants.PrefKeys.ACCESS_TOKEN);
            }
            set
            {
                _prefHelper.SetPref(Constants.PrefKeys.ACCESS_TOKEN, value);
            }
        }


        protected DateTime AccessTokenExpiration
        {
            get
            {
                return _prefHelper.GetPrefDateTime(Constants.PrefKeys.ACCESS_TOKEN_EXPIRATION);
            }
            set
            {
                _prefHelper.SetPref(Constants.PrefKeys.ACCESS_TOKEN_EXPIRATION, value);
            }
        }


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            this._prefHelper = new PrefHelper(this.ApplicationContext);
            this._teslaAPI = new TeslaAPI(this.AccessToken);
        }

        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Xml.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }


        public override bool OnOptionsItemSelected(IMenuItem item) {
            //return base.OnOptionsItemSelected(item);

            switch (item.ItemId) {
                case Resource.Id.action_menu:
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                    break;
            }

            return true;
        }


        protected void HideKeyboard() {
            RunOnUiThread(() => {
                InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
                inputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
            });
        }


        protected Bitmap GetImageBitmapFromUrl(string url) {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient()) {
                var imageBytes = webClient.DownloadData(url);

                if (imageBytes != null && imageBytes.Length > 0) {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

    }
}
