using System;

using Android.Content;
using Android.Content.Res;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Android.Widget;


namespace MyTesla.Mobile
{
    public class TimePreference : DialogPreference
    {
        private int lastHour = 0;
        private int lastMinute = 0;
        private TimePicker picker = null;


        public TimePreference(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetPositiveButtonText(Resource.String.time_preference_set);
            SetNegativeButtonText(Resource.String.time_preference_cancel);
        }


        public static int GetHour(string time)
        {
            var pieces = time.Split(":".ToCharArray());
            var hour = Convert.ToInt32(pieces[0]);

            if (time.EndsWith("PM"))
            {
                hour += 12;
            }
            else if (time.EndsWith("AM") && hour == 12)
            {
                hour = 0;
            }

            return hour;
        }


        public static int GetMinute(string time)
        {
            var pieces = time.Split(":".ToCharArray());
            var minutes = Convert.ToInt32(pieces[1].Replace(" AM", String.Empty)
                                                   .Replace(" PM", String.Empty));
            return minutes;
        }


        protected override View OnCreateDialogView()
        {
            picker = new TimePicker(Context);

            return picker;
        }


        protected override void OnBindDialogView(View v)
        {
            base.OnBindDialogView(v);

            picker.CurrentHour = (Java.Lang.Integer)lastHour;
            picker.CurrentMinute = (Java.Lang.Integer)lastMinute;
        }


        protected override void OnDialogClosed(bool positiveResult)
        {
            base.OnDialogClosed(positiveResult);

            if (positiveResult)
            {
                lastHour = (int)picker.CurrentHour;
                lastMinute = (int)picker.CurrentMinute;

                var amPM = "AM";

                if (lastHour > 12)
                {
                    amPM = "PM";
                    lastHour -= 12;
                }
                else if (lastHour == 0)
                {
                    lastHour = 12;
                }

                var time = $"{lastHour}:{lastMinute.ToString("D2")} {amPM}";

                if (CallChangeListener(time))
                {
                    PersistString(time);
                }
            }
        }


        protected override Java.Lang.Object OnGetDefaultValue(TypedArray a, int index)
        {
            return a.GetString(index);
        }


        protected override void OnSetInitialValue(bool restoreValue, Java.Lang.Object defaultValue)
        {
            string time = null;

            if (restoreValue)
            {
                if (defaultValue == null)
                {
                    time = GetPersistedString("00:00");
                }
                else
                {
                    time = GetPersistedString(defaultValue.ToString());
                }
            }
            else
            {
                time = defaultValue.ToString();
            }

            lastHour = GetHour(time);
            lastMinute = GetMinute(time);
        }
    }

}
