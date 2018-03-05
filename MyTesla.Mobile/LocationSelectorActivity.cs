using System;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using static Android.Gms.Maps.GoogleMap;

namespace MyTesla.Mobile
{
    [Activity(Label = "MyTesla.Android Location Selector", NoHistory = true)]
    [IntentFilter(new[] { "OPEN_LOCATION_SELECTOR" }, Categories = new[] { Intent.CategoryDefault })]
    public class LocationSelectorActivity : BaseActivity, IOnMapReadyCallback, ILocationListener
    {
        protected MapFragment _mapFragment = null;
        protected GoogleMap _map = null;
        protected LocationManager _locationManager = null;
        protected Marker _marker = null;
        protected Button _saveButton = null;


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.LocationSelector);

            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

            if (_mapFragment == null) {
                GoogleMapOptions mapOptions = new GoogleMapOptions().InvokeMapType(GoogleMap.MapTypeNormal)
                                                                    .InvokeZoomControlsEnabled(true)
                                                                    .InvokeCompassEnabled(true);

                var fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.map, _mapFragment, "map");
                fragTx.AddToBackStack("LocationSelectorMap");
                fragTx.Commit();
            }

            _mapFragment.GetMapAsync(this);

            _locationManager = GetSystemService(LocationService) as LocationManager;

            _saveButton = (Button)FindViewById(Resource.Id.saveButton);
            _saveButton.BringToFront();

            _saveButton.Click += delegate (object sender, EventArgs e) {
                this.ChargingLocation = _marker.Position;
                this.Finish();
            };
        }


        protected override void OnPause() {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }


        protected override void OnResume() {
            base.OnResume();

            Criteria locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.NoRequirement;

            string locationProvider = _locationManager.GetBestProvider(locationCriteria, true);

            if (!String.IsNullOrEmpty(locationProvider)) {
                _locationManager.RequestLocationUpdates(locationProvider, 2000, 5, this);
            }
        }


        public void OnMapReady(GoogleMap map) {
            _map = map;

            _map.SetInfoWindowAdapter(new MarkerInfoWindowAdapter(this));

            _map.MapClick += delegate (object sender, MapClickEventArgs e) {
                SetMarker(e.Point.Latitude, e.Point.Longitude);
            };

            _map.CameraMoveCanceled += delegate (object sender, EventArgs e) {
                if (_marker != null) {
                    _marker.ShowInfoWindow();
                }
            };

            if (this.ChargingLocation != null)
            {
                FlyToLocation(this.ChargingLocation);
            }
        }


        public void OnLocationChanged(Location location) {
            if (this.ChargingLocation == null) {
                this.ChargingLocation = new LatLng(location.Latitude, location.Longitude);
                FlyToLocation(this.ChargingLocation);
            }
        }


        protected void FlyToLocation(LatLng location)
        {
            SetMarker(location.Latitude, location.Longitude);
            _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(location, 16));
        }


        public void OnProviderDisabled(string provider) {
            //throw new NotImplementedException();
        }

        public void OnProviderEnabled(string provider) {
            //throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras) {
            //throw new NotImplementedException();
        }


        protected void SetMarker(double latitude, double longitude) {
            // Clear all markers.
            _map.Clear();

            // Add marker at specified point.
            var position = new LatLng(latitude, longitude);
            var markerOptions = new MarkerOptions().SetPosition(position)
                                                   .SetTitle("Charging Location")
                                                   .SetSnippet("This is the charging location currently set.\nTap anywhere else on the map\nto set as new location.");

            _marker = _map.AddMarker(markerOptions);

            if (this.ChargingLocation != null) {
                _marker.ShowInfoWindow();
            }

        }
    }
}
