using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Preferences;
using Android.Content.PM;


namespace MyTesla.Mobile
{
    [Activity(Label = "MyTesla.Android Settings", Icon = "@drawable/ic_settings_white_48dp", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SettingsActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            FragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, new MyPreferenceFragment(this))
                                              .Commit();
        }


        public class MyPreferenceFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
        {
            protected SettingsActivity activityContext = null;

            public MyPreferenceFragment(SettingsActivity activityContext) : base() {
                this.activityContext = activityContext;
            }


            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(Resource.Xml.preferences);
            }


            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key) {
                var pref = FindPreference(key);

                if (pref is ListPreference) {
                    pref.Summary = (pref as ListPreference).Entry;
                }

                if (key == Constants.PrefKeys.SETTING_START_TIME)
                {
                    pref.Summary = $"Monitoring starts at {this.activityContext.StartTime}";
                }
                else if (key == Constants.PrefKeys.SETTING_END_TIME)
                {
                    pref.Summary = $"Monitoring ends at {this.activityContext.EndTime}";
                }
            }


            public override void OnResume() {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                var locationPref = FindPreference(Constants.PrefKeys.SETTING_CHARGING_LOCATION);
                var distancePref = FindPreference(Constants.PrefKeys.SETTING_DISTANCE_FROM_CHARGING_LOCATION);
                var frequencyPref = FindPreference(Constants.PrefKeys.SETTING_CHECK_FREQUENCY);
                var startTimePref = FindPreference(Constants.PrefKeys.SETTING_START_TIME);
                var endTimePref = FindPreference(Constants.PrefKeys.SETTING_END_TIME);

                var location = this.activityContext.ChargingLocation;

                if (location != null) {
                    locationPref.Summary = $"{location.Latitude},{location.Longitude}";
                }
                else {
                    locationPref.Summary = "Location not set";
                }

                distancePref.Summary = $"{this.activityContext.DistanceFromChargingLocation} meters";
                frequencyPref.Summary = $"Check every {this.activityContext.CheckFrequency} hours";
                startTimePref.Summary = $"Monitoring starts at {this.activityContext.StartTime}";
                endTimePref.Summary = $"Monitoring ends at {this.activityContext.EndTime}";
            }


            public override void OnPause() {
                PreferenceManager.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                base.OnPause();
            }

        }

    }
}
