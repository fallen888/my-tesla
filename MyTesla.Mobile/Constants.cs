using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MyTesla.Mobile
{
    public static class Constants
    {
        public static string TESLA_CLIENT_ID = "e4a9949fcfa04068f59abb5a658f2bac0a3428e4652315490b659d5ab3f35a9e";
        public static string TESLA_CLIENT_SECRET = "c75f14bbadc8bee3a7594412c31416f8300256d7668ea7e6e7f06727bfb9d220";

        public static Uri TESLA_API_BASEADDRESS = new Uri("https://owner-api.teslamotors.com/");


        public static class PrefKeys
        {
            public static string USER_EMAIL = "USER_EMAIL";
            public static string USER_PASSWORD = "USER_PASSWORD";
            public static string ACCESS_TOKEN = "ACCESS_TOKEN";
            public static string ACCESS_TOKEN_EXPIRATION = "ACCESS_TOKEN_EXPIRATION";

            public static string VEHICLES = "VEHICLES";
            public static string VEHICLE_NAME = "VEHICLE_OPTION_NAME_{0}";
            public static string VEHICLE_VIN = "VEHICLE_VIN_{0}";
            public static string VEHICLE_OPTION_CODES = "VEHICLE_OPTION_CODES_{0}";
            public static string LAST_VEHICLE_CHECK = "LAST_VEHICLE_CHECK";
            public static string VEHICLE_LOCATION_LAT = "VEHICLE_LOCATION_LAT";
            public static string VEHICLE_LOCATION_LONG = "VEHICLE_LOCATION_LONG";

            public static string SETTING_REMINDER_NOTIFICATIONS_ENABLED = "reminderNotificationsEnabled";
            public static string SETTING_CHARGING_LOCATION = "chargingLocation";
            public static string SETTING_DISTANCE_FROM_CHARGING_LOCATION = "distanceFromChargingLocation";
        }
    }
}
