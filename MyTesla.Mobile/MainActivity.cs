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


            var myToolbar = (V7.Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(myToolbar);

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
                    _spinner.Visibility = ViewStates.Gone;
                    _loginContainer.Visibility = ViewStates.Visible;
                }
                else {
                    RunOnUiThread(() => {
                        _spinner.Visibility = ViewStates.Visible;
                    });

                    // Got credentials. Go get new access token.
                    await GetAccessToken(email, password);

                    _spinner.Visibility = ViewStates.Gone;
                }
            }
            else {
                // Got valid access token.
                await CheckVehicles();
            }
        }


        protected async Task<bool> CheckVehicles() {
            var vehicleIds = _prefHelper.GetPrefStrings(Constants.PrefKeys.VEHICLES);

            // TODO: Remove debug.
            vehicleIds.Clear();

            if (vehicleIds.Count == 0) {
                //RunOnUiThread(() => {
                RunOnUiThread(() => {
                    _spinner.Visibility = ViewStates.Visible;
                });

                // No vehicles found. Go get em.
                var vehicles = await _teslaAPI.GetVehicles();

                    vehicles.ForEach((vehicle) => {
                        var id = vehicle.id.ToString();
                        _prefHelper.SetPref(String.Format(Constants.PrefKeys.VEHICLE_NAME, id), vehicle.display_name);
                        _prefHelper.SetPref(String.Format(Constants.PrefKeys.VEHICLE_VIN, id), vehicle.vin);
                        _prefHelper.SetPref(String.Format(Constants.PrefKeys.VEHICLE_OPTION_CODES, id), vehicle.option_codes);
                        vehicleIds.Add(id);
                    });

                    _prefHelper.SetPref(Constants.PrefKeys.VEHICLES, vehicleIds);

                    _spinner.Visibility = ViewStates.Gone;

                    ShowVehicles();
                //});
            }
            else {
                ShowVehicles();
            }

            return true;
        }


        protected void ShowVehicles() {
            RunOnUiThread(() => {
                _spinner.Visibility = ViewStates.Visible;
            });

            var vehicleIds = _prefHelper.GetPrefStrings(Constants.PrefKeys.VEHICLES);

            var noVehiclesFound = vehicleIds.Count == 0;

            _yourTeslas.Text = noVehiclesFound ? "No vehicles found." : "Your Tesla" + (vehicleIds.Count > 1 ? "s" : String.Empty);
            _yourTeslas.Visibility = ViewStates.Visible;

            vehicleIds.ForEach((id) => {
                var name = _prefHelper.GetPrefString(String.Format(Constants.PrefKeys.VEHICLE_NAME, id));
                var vin = _prefHelper.GetPrefString(String.Format(Constants.PrefKeys.VEHICLE_VIN, id));
                var model = vin.Substring(3, 1).ToLowerInvariant();
                var optionCodes = _prefHelper.GetPrefString(String.Format(Constants.PrefKeys.VEHICLE_OPTION_CODES, id));

                // Create new container.
                var vehicleContainer = new RelativeLayout(this);
                vehicleContainer.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                //vehicleContainer.SetBackgroundColor(Color.Beige);
                vehicleContainer.SetForegroundGravity(GravityFlags.CenterHorizontal);

                // Load image.
                var imageBitmap = GetImageBitmapFromUrl($"https://www.tesla.com/configurator/compositor/?model=m{model}&view=STUD_SIDE&size=1000&options={optionCodes}&bkba_opt=1");
                var image = new ImageView(this);
                image.Id = View.GenerateViewId();
                image.SetImageBitmap(imageBitmap);
                image.SetForegroundGravity(GravityFlags.CenterHorizontal);
                image.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                //image.SetBackgroundColor(Color.YellowGreen);

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
                //nameLabel.SetBackgroundColor(Color.Pink);
                nameLabel.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                var labelLayoutParams = (RelativeLayout.LayoutParams)nameLabel.LayoutParameters;
                labelLayoutParams.AddRule(LayoutRules.AlignBottom, image.Id);
                labelLayoutParams.BottomMargin = 55;
                nameLabel.LayoutParameters = labelLayoutParams;

                vehicleContainer.AddView(nameLabel);

                _rootContainer.AddView(vehicleContainer);

                _spinner.Visibility = ViewStates.Gone;
            });
        //});
        }


        private Bitmap GetImageBitmapFromUrl(string url) {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient()) {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0) {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }


        protected void RegisterEventHandlers() {
            // Login.
            _loginButton.Click += async delegate {
                HideKeyboard();

                if (_email.Text.Length > 0 && _password.Text.Length > 0) {
                    RunOnUiThread(() => {
                        _spinner.Visibility = ViewStates.Visible;
                    });

                    await GetAccessToken(_email.Text, _password.Text);

                    _spinner.Visibility = ViewStates.Gone;
                }
            };
        }


        protected async Task<bool> GetAccessToken(string email, string password) {
            //RunOnUiThread(() => {
            var accessToken = await _teslaAPI.GetAccessToken(email, password);

                if (accessToken != null) {
                    _prefHelper.SetPref(Constants.PrefKeys.USER_EMAIL, email);
                    _prefHelper.SetPref(Constants.PrefKeys.USER_PASSWORD, password);
                    this.AccessToken = accessToken.Token;
                    this.AccessTokenExpiration = accessToken.ExpirationDate;

                    _loginContainer.Visibility = ViewStates.Gone;
                    _spinner.Visibility = ViewStates.Gone;

                    await CheckVehicles();
                }
                else {
                    _spinner.Visibility = ViewStates.Gone;
                    _loginButton.Visibility = ViewStates.Visible;
                }
            //});

            return true;
        }

    }
}
