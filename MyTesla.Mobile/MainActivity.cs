using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

using V7 = Android.Support.V7.Widget;


namespace MyTesla.Mobile
{
    [Activity(Label = "MyTesla.Android", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : BaseActivity
    {
        private LinearLayout _rootContainer = null;
        private LinearLayout _loginContainer = null;
        private EditText _email = null;
        private EditText _password = null;
        private Button _loginButton = null;
        private FrameLayout _spinner = null;
        private TextView _yourTeslas = null;


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            RunOnUiThread(() => {
                var myToolbar = (V7.Toolbar)FindViewById(Resource.Id.toolbar);
                SetSupportActionBar(myToolbar);
            });

            /*
            Intent serviceIntent = new Intent(this, typeof(ChargeCheckService));
            var component = StartService(serviceIntent);

            if (component == null) {
            }
            */

            InitControls();
            RegisterEventHandlers();
            InitApp();
        }

        protected void InitControls() {
            // Register.
            _rootContainer = (LinearLayout)FindViewById(Resource.Id.rootContainer);
            _loginContainer = (LinearLayout)FindViewById(Resource.Id.loginContainer);
            _email = (EditText)FindViewById(Resource.Id.email);
            _password = (EditText)FindViewById(Resource.Id.password);
            _loginButton = (Button)FindViewById(Resource.Id.loginButton);
            _spinner = (FrameLayout)FindViewById(Resource.Id.progress_overlay);
            _yourTeslas = (TextView)FindViewById(Resource.Id.yourTeslas);

            _spinner.BringToFront();
        }


        protected async void InitApp() {
            // Validate Access Token.
            if (String.IsNullOrEmpty(this.AccessToken) || this.AccessTokenExpiration <= DateTime.Now) {
                // No valid access token.
                var email = _prefHelper.GetPrefString(Constants.PrefKeys.USER_EMAIL);
                var password = _prefHelper.GetPrefString(Constants.PrefKeys.USER_PASSWORD);

                // Check user credentials.
                if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password)) {
                    // Show login prompt.
                    ShowSpinner(false);

                    RunOnUiThread(() => {
                        _loginContainer.Visibility = ViewStates.Visible;
                    });
                }
                else {
                    ShowSpinner(true);
                    
                    await Task.Run(async () => {
                        // Got credentials. Go get new access token.
                        await GetAccessToken(email, password);
                    });
                }
            }
            else {
                // Got valid access token.
                await CheckVehicles();
                CheckNotificationSettings();
            }
        }


        protected void CheckNotificationSettings()
        {
            var isNotificationsEnabled =_prefHelper.GetPrefBoolean(Constants.PrefKeys.SETTING_REMINDER_NOTIFICATIONS_ENABLED);
            var isPromptNeeded = !isNotificationsEnabled || this.ChargingLocation == null;

            if (isPromptNeeded)
            {
                var alert = new AlertDialog.Builder(this);

                if (!isNotificationsEnabled)
                {
                    alert.SetTitle("Notifications Disabled");
                    alert.SetMessage("Notifications are currently disabled. Would you like to enable them and configure other settings now?");
                }
                else if (this.ChargingLocation == null)
                {
                    alert.SetTitle("Charging Location Not Set");
                    alert.SetMessage("Charging location has not been set. Would you like to set it and configure other settings now?");
                }

                alert.SetPositiveButton("Yes", (senderAlert, args) =>
                {
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                });

                alert.SetNegativeButton("No", (senderAlert, args) =>
                {
                    //Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });

                using (var dialog = alert.Create())
                {
                    RunOnUiThread(() =>
                    {
                        dialog.Show();
                    });
                }
            }
        }


        protected async Task<bool> CheckVehicles() {
            var vehicleIds = _prefHelper.GetPrefStrings(Constants.PrefKeys.VEHICLES);

            if (vehicleIds.Count > 0) {
                var lastVehicleCheck = _prefHelper.GetPrefDateTime(Constants.PrefKeys.LAST_VEHICLE_CHECK);

                if (lastVehicleCheck < DateTime.Now.AddDays(-1)) {
                    // Last check was more than a day ago, so clear the list to force a new API GET.
                    vehicleIds.Clear();
                }
            }

            if (vehicleIds.Count == 0) {
                ShowSpinner(true);

                await Task.Run(async () => {

                    // No vehicles found. Go get em.
                    var vehicles = await _teslaAPI.GetVehicles();

                    vehicles.ForEach((vehicle) => {
                        var id = vehicle.id.ToString();
                        _prefHelper.SetPref(String.Format(Constants.PrefKeys.VEHICLE_VIN, id), vehicle.vin);
                        _prefHelper.SetPref(String.Format(Constants.PrefKeys.VEHICLE_NAME, id), vehicle.display_name);
                        _prefHelper.SetPref(String.Format(Constants.PrefKeys.VEHICLE_OPTION_CODES, id), vehicle.option_codes);
                        vehicleIds.Add(id);
                    });

                    _prefHelper.SetPref(Constants.PrefKeys.VEHICLES, vehicleIds);
                });

                ShowSpinner(false);
                ShowVehicles();
            }
            else {
                ShowVehicles();
            }

            _prefHelper.SetPref(Constants.PrefKeys.LAST_VEHICLE_CHECK, DateTime.Now.ToString());
            return true;
        }


        protected void ShowVehicles() {
            ShowSpinner(true);

            var vehicleIds = _prefHelper.GetPrefStrings(Constants.PrefKeys.VEHICLES);

            //vehicleIds.Add(vehicleIds[0]);

            RunOnUiThread(() => {
                _yourTeslas.Text = (vehicleIds.Count == 0) ? "No vehicles found." : "Your Tesla" + (vehicleIds.Count > 1 ? "s" : String.Empty);
                _yourTeslas.Visibility = ViewStates.Visible;
            });

            vehicleIds.ForEach((id) => {
                var name = _prefHelper.GetPrefString(String.Format(Constants.PrefKeys.VEHICLE_NAME, id));
                var vin = _prefHelper.GetPrefString(String.Format(Constants.PrefKeys.VEHICLE_VIN, id));
                var model = vin.Substring(3, 1).ToLowerInvariant();
                var optionCodes = _prefHelper.GetPrefString(String.Format(Constants.PrefKeys.VEHICLE_OPTION_CODES, id));

                // Create new container.
                var vehicleContainer = new LinearLayout(this);
                vehicleContainer.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                vehicleContainer.Orientation = Orientation.Vertical;
                vehicleContainer.SetForegroundGravity(GravityFlags.CenterHorizontal);
                vehicleContainer.SetPadding(0, 0, 0, 150);

                // Load image.
                var image = new ImageView(this);

                using (var imageBitmap = GetImageBitmapFromUrl($"https://www.tesla.com/configurator/compositor/?model=m{model}&view=STUD_SIDE&size=1000&options={optionCodes}&bkba_opt=1"))
                using (var croppedBitmap = Bitmap.CreateBitmap(imageBitmap, 0, (int)(imageBitmap.Height * 0.18), imageBitmap.Width, (int)(imageBitmap.Height * 0.55)))
                {
                    image.SetImageBitmap(croppedBitmap);
                }

                //image.SetBackgroundColor(Color.LightPink);
                image.SetForegroundGravity(GravityFlags.CenterHorizontal);
                image.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = GravityFlags.CenterHorizontal
                };

                // Show name.
                var nameLabel = new TextView(this);
                nameLabel.Text = name;
                nameLabel.Gravity = GravityFlags.CenterHorizontal;
                nameLabel.TextSize = 23;
                nameLabel.SetTextColor(Color.Black);
                nameLabel.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = GravityFlags.CenterHorizontal
                };

                // Show status spinner until charge state is retrieved.
                var statusSpinner = new ProgressBar(this, null, 0, Resource.Style.Widget_AppCompat_ProgressBar);
                statusSpinner.SetForegroundGravity(GravityFlags.Center);
                statusSpinner.Indeterminate = true;
                statusSpinner.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = GravityFlags.CenterHorizontal,
                    Height = 50,
                    Width = 50
                };

                vehicleContainer.AddView(nameLabel);
                vehicleContainer.AddView(image);
                vehicleContainer.AddView(statusSpinner);

                RunOnUiThread(() => {
                    _rootContainer.AddView(vehicleContainer);
                });

                Task.Run(async () => {
                    // Get current charge state.
                    var chargeState = await _teslaAPI.GetChargeState(Convert.ToInt64(id));

                    var chargeStateLabel = new TextView(this)
                    {
                        Text = $"Status: {chargeState.charging_state}{System.Environment.NewLine}{chargeState.battery_level}% charged with a range of {chargeState.battery_range} miles.",
                        Gravity = GravityFlags.CenterHorizontal,
                        TextSize = 16
                    };

                    chargeStateLabel.SetTextColor(Color.DarkGray);
                    chargeStateLabel.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                    RunOnUiThread(() => {
                        statusSpinner.Visibility = ViewStates.Gone;
                        vehicleContainer.AddView(chargeStateLabel);
                    });
                });

            });

            ShowSpinner(false);
        }


        protected void RegisterEventHandlers() {
            // Login.
            _loginButton.Click += async delegate {
                await Task.Run(async () =>
                {
                    HideKeyboard();

                    if (_email.Text.Length > 0 && _password.Text.Length > 0) {
                        await GetAccessToken(_email.Text, _password.Text);
                    }
                });
            };
        }


        protected async Task<bool> GetAccessToken(string email, string password) {
            ShowSpinner(true);
            
            var accessToken = await _teslaAPI.GetAccessToken(email, password);

            if (accessToken != null) {
                _prefHelper.SetPref(Constants.PrefKeys.USER_EMAIL, email);
                _prefHelper.SetPref(Constants.PrefKeys.USER_PASSWORD, password);
                this.AccessToken = accessToken.Token;
                this.AccessTokenExpiration = accessToken.ExpirationDate;

                RunOnUiThread(() => {
                    _loginContainer.Visibility = ViewStates.Gone;
                });

                ShowSpinner(false);

                await CheckVehicles();
                CheckNotificationSettings();
            }
            else {
                ShowSpinner(false);
            }

            return true;
        }


        protected void ShowSpinner(bool show) {
            RunOnUiThread(() => {
                _spinner.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
            });
        }

    }
}
