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
using Android.Gms.Maps.Model;

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


        protected LatLng ChargingLocation
        {
            get
            {
                LatLng location = null;

                var locationLat = _prefHelper.GetPrefDouble(Constants.PrefKeys.VEHICLE_LOCATION_LAT);
                var locationLong = _prefHelper.GetPrefDouble(Constants.PrefKeys.VEHICLE_LOCATION_LONG);

                if (locationLat.HasValue && locationLong.HasValue) {
                    location = new LatLng(locationLat.Value, locationLong.Value);
                }

                return location;
            }
            set
            {
                if (value != null) {
                    _prefHelper.SetPref(Constants.PrefKeys.VEHICLE_LOCATION_LAT, value.Latitude);
                    _prefHelper.SetPref(Constants.PrefKeys.VEHICLE_LOCATION_LONG, value.Longitude);
                }
                else {
                    _prefHelper.RemovePref(Constants.PrefKeys.VEHICLE_LOCATION_LAT);
                    _prefHelper.RemovePref(Constants.PrefKeys.VEHICLE_LOCATION_LONG);
                }
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
