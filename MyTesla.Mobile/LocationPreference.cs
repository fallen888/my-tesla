using Android.Content;
using Android.Content.Res;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Android.Widget;


namespace MyTesla.Mobile
{
    //public class LocationPreference : DialogPreference
    //{
    //    private int mTime;
    //    private int mDialogLayoutResId = Resource.Xml.locationSelector;
    //    private TimePicker picker = null;

    //    public LocationPreference(Context context) : this(context, null) { }

    //    public LocationPreference(Context context, IAttributeSet attrs) : this(context, attrs, Android.Resource.Attribute.DialogPreferenceStyle) { }

    //    public LocationPreference(Context context, IAttributeSet attrs, int defStyleAttr) : this(context, attrs, defStyleAttr, defStyleAttr) { }

    //    public LocationPreference(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
    //        // Do custom stuff here
    //        // ...
    //        // read attributes etc.

    //        DialogLayoutResource = mDialogLayoutResId;
    //    }

    //    public int GetTime() {
    //        return mTime;
    //    }

    //    public void SetTime(int time) {
    //        mTime = time;

    //        // Save to Shared Preferences
    //        PersistInt(time);
    //    }


    //    protected override Java.Lang.Object OnGetDefaultValue(TypedArray a, int index) {
    //        // Default value from attribute. Fallback value is set to 0.
    //        return base.OnGetDefaultValue(a, index);
    //        //return a.GetString(index);
    //    }


    //    protected override void OnSetInitialValue(bool restorePersistedValue, Java.Lang.Object defaultValue) {
    //        //base.OnSetInitialValue(restorePersistedValue, defaultValue);
    //        // Read the value. Use the default value if it is not possible.
    //        SetTime(restorePersistedValue ? GetPersistedInt(mTime) : (int)defaultValue);
    //    }


    //    protected override View OnCreateDialogView() {
    //        picker = new TimePicker(Context);
    //        return picker;
    //    }


    //    protected override void OnBindDialogView(View view) {
    //        base.OnBindDialogView(view);
    //    }

    //    protected override void OnDialogClosed(bool positiveResult) {
    //        base.OnDialogClosed(positiveResult);

    //        if (positiveResult) {
    //            string time = "6:40";

    //            if (CallChangeListener(time)) {
    //                PersistString("6:40");
    //            }
    //        }
    //    }

    //}
}
