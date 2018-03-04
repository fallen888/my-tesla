using System;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using static Android.Gms.Maps.GoogleMap;


namespace MyTesla.Mobile
{
    public class MarkerInfoWindowAdapter : Java.Lang.Object, IInfoWindowAdapter
    {
        Context _context = null;

        public MarkerInfoWindowAdapter(Context context) : base() {
            this._context = context;
        }


        public View GetInfoContents(Marker marker) {
            LinearLayout info = new LinearLayout(_context);
            info.Orientation = Orientation.Vertical;

            TextView title = new TextView(_context);
            title.SetTextColor(Android.Graphics.Color.Black);
            title.Gravity = GravityFlags.Center;
            title.SetTypeface(null, TypefaceStyle.Bold);
            title.SetText(marker.Title, TextView.BufferType.Normal);

            TextView snippet = new TextView(_context);
            snippet.SetTextColor(Android.Graphics.Color.Gray);
            snippet.SetText(marker.Snippet, TextView.BufferType.Normal);

            info.AddView(title);
            info.AddView(snippet);

            return info;
        }


        public View GetInfoWindow(Marker marker) {
            return null;
        }

    }
}