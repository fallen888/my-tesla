using System;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using static Android.Gms.Maps.GoogleMap;

namespace MyTesla.Mobile
{
    //<activity android:name=".LocationSelectorActivity">
    //  <intent-filter>
    //    <action android:name="android.intent.action.VIEW" />
    //    <category android:name="android.intent.category.DEFAULT" />
    //  </intent-filter>
    //</activity>

    [Activity(Label = "MyTesla.Android Location Selector")]
    [IntentFilter(new[] { "OPEN_LOCATION_SELECTOR" },
                  Categories = new[] { Intent.CategoryDefault })]
    public class LocationSelectorActivity : BaseActivity, IOnMapReadyCallback
    {
        protected MapFragment _mapFragment = null;
        protected GoogleMap _map = null;
        

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
                fragTx.Commit();
            }

            _mapFragment.GetMapAsync(this);

            //if (_map != null) {
            //    _map.UiSettings.ZoomControlsEnabled = true;
            //    _map.UiSettings.CompassEnabled = true;
            //}

            //_mapFragment.tap
        }


        public void OnMapReady(GoogleMap map) {
            _map = map;

            _map.MapClick += delegate (object sender, MapClickEventArgs e) {
                // Clear all markers.
                _map.Clear();

                // Add marker at specified point.
                var markerOptions = new MarkerOptions().SetPosition(e.Point)
                                                       .SetTitle("Selected charging location");
                _map.AddMarker(markerOptions);
            };
        }
    }
}
