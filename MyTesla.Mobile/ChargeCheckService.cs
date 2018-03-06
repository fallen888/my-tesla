using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;

namespace MyTesla.Mobile {

    [Service(Name = "MyTesla.Android.ChargeCheckService",
             Process = "kalilabs.mytesla.notifier_process",
             Exported = true)]
    public class ChargeCheckService : Service
    {
        const int NOTIFICATION_ID = 6663;
        //static readonly string TAG = "X:" + typeof(ChargeCheckService).Name;
        static readonly string TAG = "********";

        Timer timer;
        bool isStarted = false;
        private IDictionary<string, DateTime> trackedErrors = null;


        #region --- Properties ---
        PrefHelper _prefHelper = null;
        protected PrefHelper PrefHelper
        {
            get
            {
                if (_prefHelper == null)
                {
                    _prefHelper = new PrefHelper(this.ApplicationContext);
                }

                return _prefHelper;
            }
        }

        protected TeslaAPI _teslaAPI = null;
        protected TeslaAPI TeslaAPI
        {
            get
            {
                if (_teslaAPI == null)
                {
                    _teslaAPI = new TeslaAPI(this.AccessToken);
                }

                return _teslaAPI;
            }
        }

        protected string AccessToken
        {
            get
            {
                return PrefHelper.GetPrefString(Constants.PrefKeys.ACCESS_TOKEN);
            }
            set
            {
                PrefHelper.SetPref(Constants.PrefKeys.ACCESS_TOKEN, value);
            }
        }

        protected DateTime AccessTokenExpiration
        {
            get
            {
                return PrefHelper.GetPrefDateTime(Constants.PrefKeys.ACCESS_TOKEN_EXPIRATION);
            }
            set
            {
                PrefHelper.SetPref(Constants.PrefKeys.ACCESS_TOKEN_EXPIRATION, value);
            }
        }

        protected int CheckFrequency
        {
            get
            {
                var frequencyString = PrefHelper.GetPrefString(Constants.PrefKeys.SETTING_CHECK_FREQUENCY);
                var frequency = Convert.ToInt32(frequencyString) * 1000 * 60;   // in hours.
                return frequency;
            }
        }

        protected DateTime MonitorStartTime
        {
            get
            {
                var startTimeString = PrefHelper.GetPrefString(Constants.PrefKeys.SETTING_START_TIME);
                var startTime = GetDateTime(startTimeString);
                return startTime;
            }
        }

        protected DateTime MonitorEndTime
        {
            get
            {
                var endTimeString = PrefHelper.GetPrefString(Constants.PrefKeys.SETTING_END_TIME);
                var endTime = GetDateTime(endTimeString);
                return endTime;

            }
        }

        protected bool IsWithinMonitorTimeframe
        {
            get
            {
                return (DateTime.Now >= MonitorStartTime && DateTime.Now <= MonitorEndTime);
            }
        }

        protected LatLng ChargingLocation
        {
            get
            {
                LatLng location = null;

                var locationLat = PrefHelper.GetPrefDouble(Constants.PrefKeys.VEHICLE_LOCATION_LAT);
                var locationLong = PrefHelper.GetPrefDouble(Constants.PrefKeys.VEHICLE_LOCATION_LONG);

                if (locationLat.HasValue && locationLong.HasValue)
                {
                    location = new LatLng(locationLat.Value, locationLong.Value);
                }

                return location;
            }
            set
            {
                if (value != null)
                {
                    PrefHelper.SetPref(Constants.PrefKeys.VEHICLE_LOCATION_LAT, value.Latitude);
                    PrefHelper.SetPref(Constants.PrefKeys.VEHICLE_LOCATION_LONG, value.Longitude);
                }
                else
                {
                    PrefHelper.RemovePref(Constants.PrefKeys.VEHICLE_LOCATION_LAT);
                    PrefHelper.RemovePref(Constants.PrefKeys.VEHICLE_LOCATION_LONG);
                }
            }
        }


        protected int DistanceFromChargingLocation
        {
            get
            {
                var distanceString = PrefHelper.GetPrefString(Constants.PrefKeys.SETTING_DISTANCE_FROM_CHARGING_LOCATION);
                var distance = Convert.ToInt32(distanceString);
                return distance;
            }
        }
        #endregion


        public override void OnCreate() {
            base.OnCreate();
        }


        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId) {
            //return base.OnStartCommand(intent, flags, startId);

            Log.Debug(TAG, "Enter: OnStartCommand()");

            if (!isStarted) {
                Log.Debug(TAG, $"CheckFrequency: {this.CheckFrequency}");

                timer = new Timer(HandleTimerCallback, "state", 0, this.CheckFrequency);
                isStarted = true;
            }

            Log.Debug(TAG, "Exit: OnStartCommand()");

            return StartCommandResult.Sticky;
        }


        public override IBinder OnBind(Intent intent) {
            //throw new NotImplementedException();
            return null;
        }


        public override void OnDestroy() {
            timer.Dispose();
            timer = null;
            isStarted = false;
    
            base.OnDestroy();
        }


        protected void HandleTimerCallback(object state)
        {
            Log.Debug(TAG, "Enter: HandleTimerCallback()");

            if (this.IsWithinMonitorTimeframe)
            {
                ValidateAccessToken();

                var vehicleIds = PrefHelper.GetPrefStrings(Constants.PrefKeys.VEHICLES);

                vehicleIds.ForEach((id) =>
                {
                    var vehicleId = Convert.ToInt64(id);
                    var vehicleName = PrefHelper.GetPrefString(String.Format(Constants.PrefKeys.VEHICLE_NAME, id));

                    Log.Debug(TAG, $"vehicleId: {vehicleId}");
                    Log.Debug(TAG, $"vehicleName: {vehicleName}");

                    var isAtChargingLocation = IsAtChargingLocation(vehicleId);

                    if (isAtChargingLocation)
                    {
                        Log.Debug(TAG, "GetChargeState...");
                        var chargeState = _teslaAPI.GetChargeState(vehicleId).Result;

                        Log.Debug(TAG, $"charging_state: {chargeState.charging_state}");

                        //if (chargeState.charging_state == "Disconnected")
                        //{
                            ShowNotification(vehicleName, chargeState.charging_state, chargeState.battery_level.Value, chargeState.battery_range.Value);
                        //}
                    }
                });
            }

            Log.Debug(TAG, "Exit: HandleTimerCallback()");
        }


        protected bool IsAtChargingLocation(long vehicleId)
        {
            Log.Debug(TAG, $"Enter: IsAtChargingLocation({vehicleId})");


            Log.Debug(TAG, "GetDriveState...");
            var driveState = TeslaAPI.GetDriveState(vehicleId).Result;

            var chargingLocation = new Location("Charging Location")
            {
                Latitude = this.ChargingLocation.Latitude,
                Longitude = this.ChargingLocation.Longitude
            };

            var vehicleLocation = new Location("Vehicle Location")
            {
                Latitude = driveState.latitude.Value,
                Longitude = driveState.longitude.Value
            };

            var currentDistance = chargingLocation.DistanceTo(vehicleLocation);    // in meters.


            Log.Debug(TAG, $"currentDistance: {currentDistance}");

            Log.Debug(TAG, $"Exit: IsAtChargingLocation({vehicleId})");

            return (currentDistance <= this.DistanceFromChargingLocation);
        }


        protected DateTime GetDateTime(string value)
        {
            var timespan = DateTime.ParseExact(value, "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
            var time = DateTime.Now.Date.Add(timespan);

            return time;
        }


        protected void ValidateAccessToken()
        {
            Log.Debug(TAG, "Enter: ValidateAccessToken()");
            Log.Debug(TAG, $"AccessToken: {this.AccessToken}");
            Log.Debug(TAG, $"AccessTokenExpiration: {this.AccessTokenExpiration}");

            if (String.IsNullOrEmpty(this.AccessToken) || this.AccessTokenExpiration <= DateTime.Now)
            {
                // No valid access token.
                var email = PrefHelper.GetPrefString(Constants.PrefKeys.USER_EMAIL);
                var password = PrefHelper.GetPrefString(Constants.PrefKeys.USER_PASSWORD);

                Log.Debug(TAG, "GetAccessToken...");
                var accessToken = TeslaAPI.GetAccessToken(email, password).Result;


                if (accessToken != null)
                {
                    PrefHelper.SetPref(Constants.PrefKeys.USER_EMAIL, email);
                    PrefHelper.SetPref(Constants.PrefKeys.USER_PASSWORD, password);

                    this.AccessToken = accessToken.Token;
                    this.AccessTokenExpiration = accessToken.ExpirationDate;

                    Log.Debug(TAG, $"AccessToken: {this.AccessToken}");
                    Log.Debug(TAG, $"AccessTokenExpiration: {this.AccessTokenExpiration}");
                }
            }

            Log.Debug(TAG, "Exit: ValidateAccessToken()");
        }


        protected void ShowNotification(string vehicleName, string charginState, int batteryLevel, double batteryRange)
        {
            string title = null;
            string body = null;

            if (charginState == "Disconnected")
            {
                title = $"Time to Charge Up for {vehicleName}";
                body = $"{vehicleName}'s battery is at {batteryLevel}% with a range of {batteryRange} miles. You may want to plug in.";
            }
            //else if (charginState == "Complete")
            //{
            //    title = $"{vehicleName} is Charged";
            //    body = $"{vehicleName}'s battery is at {batteryLevel}% with a range of {batteryRange} miles.";
            //}

            if (title != null)
            {
                Notification.Builder notificationBuilder = new Notification.Builder(this).SetSmallIcon(Resource.Drawable.ic_lightbulb_outline_white_48dp)
                                                                                         .SetContentTitle(title)
                                                                                         .SetContentText(body);

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);

                notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
            }
        }


        protected void CleanupOldErrors() {
            var errorKeys = new string[trackedErrors.Keys.Count];
            trackedErrors.Keys.CopyTo(errorKeys, 0);

            foreach (var key in errorKeys) {
                // If error was last tracked 6 or more hours ago, remove it.
                if (DateTime.Now.Subtract(trackedErrors[key]).Hours >= 6) {
                    trackedErrors.Remove(key);
                }
            }
        }

    }

}
