using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;


namespace MyTesla.Mobile
{
    public class PrefHelper
    {
        protected ISharedPreferences _sharedPrefs = null;

        public PrefHelper(Context context) {
            this._sharedPrefs = PreferenceManager.GetDefaultSharedPreferences(context);
        }


        public string GetPrefString(String key) {
            return _sharedPrefs == null ? null : _sharedPrefs.GetString(key, null);
        }


        public List<string> GetPrefStrings(String key) {
            return _sharedPrefs == null ? new List<string>() : _sharedPrefs.GetStringSet(key, new List<string>()).ToList();
        }


        public int GetPrefInt(String key) {
            return _sharedPrefs == null ? 0 : _sharedPrefs.GetInt(key, 0);
        }


        public bool GetPrefBoolean(String key) {
            return _sharedPrefs == null ? false : _sharedPrefs.GetBoolean(key, false);
        }


        public void SetPref(String key, String value) {
            var editor = _sharedPrefs.Edit();
            editor.PutString(key, value);
            editor.Apply();
        }

        public void SetPref(String key, ICollection<String> value) {
            var editor = _sharedPrefs.Edit();
            editor.PutStringSet(key, value);
            editor.Apply();
        }


        public void SetPref(String key, int value) {
            var editor = _sharedPrefs.Edit();
            editor.PutInt(key, value);
            editor.Apply();
        }


        public void SetPref(String key, bool value) {
            var editor = _sharedPrefs.Edit();
            editor.PutBoolean(key, value);
            editor.Apply();
        }
    }
}
