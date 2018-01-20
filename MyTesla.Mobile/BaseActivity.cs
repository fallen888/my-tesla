using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Views;
using Android.Support.V7.App;
using V7 = Android.Support.V7.Widget;
using Android.Animation;
using Android.Views.InputMethods;
using Android.Content.PM;

namespace MyTesla.Mobile
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class BaseActivity : AppCompatActivity, Animator.IAnimatorListener
    {
        protected PrefHelper _prefHelper = null;
        protected TeslaAPI _teslaAPI = null;

        protected View _view = null;
        protected ViewStates _toVisibility = ViewStates.Gone;

        protected string AccessToken
        {
            get
            {
                return _prefHelper.GetPrefString(Constants.PrefKeys.ACCESS_TOKEN);
            }
            set
            {
                _prefHelper.SetPref(Constants.PrefKeys.ACCESS_TOKEN, value);
            }
        }


        protected DateTime AccessTokenExpiration
        {
            get
            {
                DateTime expiration;
                string expirationString = _prefHelper.GetPrefString(Constants.PrefKeys.ACCESS_TOKEN_EXPIRATION);

                if (!String.IsNullOrEmpty(expirationString)) {
                    expiration = DateTime.Parse(expirationString);
                }
                else {
                    expiration = DateTime.MinValue;
                }

                return expiration;
            }
            set
            {
                _prefHelper.SetPref(Constants.PrefKeys.ACCESS_TOKEN_EXPIRATION, value.ToString());
            }
        }


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            this._prefHelper = new PrefHelper(this.ApplicationContext);
            this._teslaAPI = new TeslaAPI(this.AccessToken);
        }

        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Xml.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }


        public override bool OnOptionsItemSelected(IMenuItem item) {
            //return base.OnOptionsItemSelected(item);

            switch (item.ItemId) {
                case Resource.Id.action_menu:
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                    break;
            }

            return true;
        }


        /**
         * @param view         View to animate
         * @param toVisibility Visibility at the end of animation
         * @param toAlpha      Alpha at the end of animation
         * @param duration     Animation duration in ms
         */
        public void AnimateView(View view, ViewStates toVisibility, int duration) {
            this._view = view;
            this._toVisibility = toVisibility;

            bool show = (toVisibility == ViewStates.Visible);

            if (show) {
                view.Alpha = 0;
            }

            view.Visibility = toVisibility;
            view.BringToFront();

            var animator = view.Animate();
            animator.SetDuration(duration);
            animator.Alpha(show ? 1 : 0);
            animator.SetListener(this);
        }


        public void OnAnimationCancel(Animator animation) {
            //throw new NotImplementedException();
        }

        public void OnAnimationEnd(Animator animation) {
            this._view.Visibility = this._toVisibility;
        }

        public void OnAnimationRepeat(Animator animation) {
            //throw new NotImplementedException();
        }

        public void OnAnimationStart(Animator animation) {
            //throw new NotImplementedException();
        }


        protected void HideKeyboard() {
            InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            inputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }
    }
}
