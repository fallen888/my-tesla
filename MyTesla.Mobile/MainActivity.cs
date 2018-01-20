using System;
using System.Net;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using V7 = Android.Support.V7.Widget;
using Android.Graphics;
using Android.Util;
using Android.Content.PM;
using System.Threading.Tasks;

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
                var vehicleContainer = new RelativeLayout(this);
                vehicleContainer.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                vehicleContainer.SetForegroundGravity(GravityFlags.CenterHorizontal);

                // Load image.
                var imageBitmap = GetImageBitmapFromUrl($"https://www.tesla.com/configurator/compositor/?model=m{model}&view=STUD_SIDE&size=1000&options={optionCodes}&bkba_opt=1");
                var image = new ImageView(this);
                image.Id = View.GenerateViewId();
                image.SetImageBitmap(imageBitmap);
                image.SetForegroundGravity(GravityFlags.CenterHorizontal);
                image.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

                // Center image.
                var imageLayoutParams = (RelativeLayout.LayoutParams)image.LayoutParameters;
                imageLayoutParams.AddRule(LayoutRules.CenterHorizontal);
                image.LayoutParameters = imageLayoutParams;

                vehicleContainer.AddView(image);

                // Show name.
                var nameLabel = new TextView(this);
                nameLabel.Text = name;
                nameLabel.Gravity = GravityFlags.CenterHorizontal;
                nameLabel.TextSize = 25;
                nameLabel.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var labelLayoutParams = (RelativeLayout.LayoutParams)nameLabel.LayoutParameters;
                labelLayoutParams.AddRule(LayoutRules.AlignBottom, image.Id);
                labelLayoutParams.BottomMargin = 55;
                nameLabel.LayoutParameters = labelLayoutParams;

                vehicleContainer.AddView(nameLabel);

                RunOnUiThread(() => {
                    _rootContainer.AddView(vehicleContainer);
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
