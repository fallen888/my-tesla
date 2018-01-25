﻿using System;
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
                                              .AddToBackStack(null)
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
                var pref = FindPreference(key) as ListPreference;

                if (pref != null) {
                    pref.Summary = pref.Entry;
                }
            }


            public override void OnResume() {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                var pref = FindPreference("chargingLocation");

                var location = this.activityContext.ChargingLocation;

                if (location != null) {
                    pref.Summary = $"{location.Latitude},{location.Longitude}";
                }
                else {
                    pref.Summary = "Location not set";
                }
            }


            public override void OnPause() {
                PreferenceManager.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                base.OnPause();
            }

        }

    }
}
