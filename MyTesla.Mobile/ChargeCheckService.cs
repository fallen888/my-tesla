using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


namespace MyTesla.Mobile {

    [Service(Name = "MyTesla.Android.ChargeCheckService",
             Process = "kalilabs.mytesla.notifier_process",
             Exported = true)]
    //[Service(IsolatedProcess = true)]
    public class ChargeCheckService : Service {
        const int NOTIFICATION_ID = 6663;
        static readonly string TAG = "X:" + typeof(ChargeCheckService).Name;
#if DEBUG
        static readonly int _timerWait = 10000;     // Check every 10 seconds.
#else
        static readonly int _timerWait = 600000;     // Check every 10 minutes.
#endif

        Timer timer;
        bool isStarted = false;
        private IDictionary<string, DateTime> trackedErrors = null;


        public override void OnCreate() {
            base.OnCreate();
        }


        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId) {
            //return base.OnStartCommand(intent, flags, startId);

            if (!isStarted) {
                timer = new Timer(HandleTimerCallback, "state", 0, _timerWait);
                isStarted = true;
            }

            return StartCommandResult.Sticky;
        }


        public override IBinder OnBind(Intent intent) {
            //throw new NotImplementedException();
            return null;
        }


        public override void OnDestroy() {
            timer.Dispose();
            timer = null;
            isStarted = false;
    
            base.OnDestroy();
        }


        void HandleTimerCallback(object state) {
            // Code omitted for clarity - here is where the service would do something.

            // Work has finished, now dispatch anotification to let the user know.
            Notification.Builder notificationBuilder = new Notification.Builder(this).SetSmallIcon(Resource.Drawable.ic_lightbulb_outline_white_48dp)
                                                                                     .SetContentTitle(Resources.GetString(Resource.String.notification_title))
                                                                                     .SetContentText(Resources.GetString(Resource.String.notification_content));

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);

            notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
        }


        protected void CleanupOldErrors() {
            var errorKeys = new string[trackedErrors.Keys.Count];
            trackedErrors.Keys.CopyTo(errorKeys, 0);

            foreach (var key in errorKeys) {
                // If error was last tracked 6 or more hours ago, remove it.
                if (DateTime.Now.Subtract(trackedErrors[key]).Hours >= 6) {
                    trackedErrors.Remove(key);
                }
            }
        }

    }

}
