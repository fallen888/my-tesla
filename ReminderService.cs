using System;
using System.Collections.Generic;
using System.Configuration;
using System.Device.Location;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Timers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

using MyTesla.Models;


namespace MyTesla
{
	public partial class ReminderService : ServiceBase
	{
		private ILog fileLogger = null;
		private ILog smtpLogger = null;
		private Timer timer = new Timer();
		private string homeCheckTime = ConfigurationManager.AppSettings["home_check_time"];
		private int homeCheckHour = 0;
		private int homeCheckMinute = 0;
		private string homeCheckAMorPM = null;

		private string TESLA_CLIENT_ID = ConfigurationManager.AppSettings["TESLA_CLIENT_ID"];
		private string TESLA_CLIENT_SECRET = ConfigurationManager.AppSettings["TESLA_CLIENT_SECRET"];
		private string EMAIL = ConfigurationManager.AppSettings["email"];
		private string PASSWORD = ConfigurationManager.AppSettings["password"];
		private double home_lat = Convert.ToDouble(ConfigurationManager.AppSettings["home_lat"]);
		private double home_long = Convert.ToDouble(ConfigurationManager.AppSettings["home_long"]);
		private int reminder_interval = Convert.ToInt32(ConfigurationManager.AppSettings["reminder_interval"]);

		private Uri baseAddress = new Uri("https://owner-api.teslamotors.com/");

		private string access_token = null;
		private DateTime accessTokenExpirationDate = DateTime.Now.AddMonths(1);

        private DateTime lastReminderSentAt = new DateTime(2000, 1, 1);

        private IDictionary<string, DateTime> trackedErrors = null;


		private string AccessToken
		{
			get
			{
				if (access_token == null || DateTime.Now >= accessTokenExpirationDate)
				{
					// Log in.
					fileLogger.Info("Retrieving access token...");

					using (var client = new HttpClient { BaseAddress = baseAddress })
					{
						using (var content = new StringContent(String.Empty))
						{
							var requestUri = $"oauth/token?grant_type=password&client_id={TESLA_CLIENT_ID}&client_secret={TESLA_CLIENT_SECRET}&email={EMAIL}&password={PASSWORD}";

							using (var response = client.PostAsJsonAsync<object>(requestUri, content).Result)
							{
								string responseData = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
								var model = JsonConvert.DeserializeObject<LoginResponse>(responseData);
								access_token = model.access_token;
								accessTokenExpirationDate = DateTime.Now.AddSeconds(Convert.ToInt32(model.expires_in));

								fileLogger.Info("New access token acquired.");
							}
						}
					}
				}

				return access_token;
			}
		}


		public ReminderService()
		{
			fileLogger = LogManager.GetLogger("FileAppender");
			smtpLogger = LogManager.GetLogger("SmtpAppender");

			InitializeComponent();
            
            trackedErrors = new Dictionary<string, DateTime>();

            //OnStart(new string[0]);
        }


		protected override void OnStart(string[] args)
		{
			try
			{
				var timeParts = homeCheckTime.Split(" ".ToCharArray());
				var hourMinParts = timeParts[0].Split(":".ToCharArray());
				homeCheckHour = Convert.ToInt32(hourMinParts[0]);
				homeCheckMinute = Convert.ToInt32(hourMinParts[1]);
				homeCheckAMorPM = timeParts[1];

                trackedErrors.Clear();

                timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
				timer.Interval = 600000; // 10 minutes
				timer.AutoReset = true;
				timer.Enabled = true;
				timer.Start();

				fileLogger.Info("Service started.");

                //Timer_Elapsed(null, null);
            }
			catch (Exception ex)
			{
				fileLogger.Error(ex.ToString());
                SmtpLogError(ex.ToString());
			}
		}


		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
                CleanupOldErrors();

                if (DateTime.Now >= (lastReminderSentAt.AddMinutes(reminder_interval)))
				{
					var homeCheckTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, (homeCheckAMorPM == "PM" ? homeCheckHour + 12 : homeCheckHour), homeCheckMinute, 0);

					if (DateTime.Now >= homeCheckTime)
					{
						fileLogger.Info("Starting charging check...");
						DoChargingCheck();
						fileLogger.Info("Charging check complete.");
					}
				}
			}
			catch (Exception ex)
			{
				fileLogger.Error(ex.ToString());
                SmtpLogError(ex.ToString());
            }
		}


		protected override void OnContinue()
		{
			base.OnContinue();
			timer.Start();
			fileLogger.Info("Service continued.");
		}


		protected override void OnPause()
		{
			base.OnPause();
			timer.Stop();
			fileLogger.Info("Service paused.");
		}


		protected override void OnShutdown()
		{
			base.OnShutdown();
			timer.Stop();
		}


		protected override void OnStop()
		{
			fileLogger.Info("Service stopped.");
		}


		private void DoChargingCheck()
		{
			using (var client = new HttpClient { BaseAddress = baseAddress })
			{
				// Set access token in auth header.
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.AccessToken);

				// Get list of vehicles.
				using (var vehiclesResponse = client.GetAsync("api/1/vehicles").Result)
				{
					var vehiclesJson = vehiclesResponse.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
					var vehicles = JsonConvert.DeserializeObject<TeslaResponse<List<Vehicle>>>(vehiclesJson).Content;

					if (vehicles.Count > 0)
					{
						fileLogger.Info("Found a vehicle.");

						var vehicle = vehicles[0];

						if (vehicle.in_service.HasValue && vehicle.in_service.Value)
						{
							fileLogger.Info("Vehicle is currently in service.");
							return;
						}

						// Get vehicle's drive state.
						using (var driveStateResponse = client.GetAsync($"api/1/vehicles/{vehicle.id}/data_request/drive_state").Result)
						{
							if (!ValidateApiReponse(driveStateResponse, "GET drive_state")) return;

							var driveStateJson = driveStateResponse.Content.ReadAsStringAsync().Result;
							var driveState = JsonConvert.DeserializeObject<TeslaResponse<DriveState>>(driveStateJson).Content;

							var sCoord = new GeoCoordinate(home_lat, home_long);
							var eCoord = new GeoCoordinate(driveState.latitude.Value, driveState.longitude.Value);

							var distanceFromHome = sCoord.GetDistanceTo(eCoord);    // in meters.

							// Is vehicle close to home?
							if (distanceFromHome <= 50)
							{
								fileLogger.Info("Vehicle is at home.");

								// Get vehicle's charge state.
								using (var chargeStateResponse = client.GetAsync($"api/1/vehicles/{vehicle.id}/data_request/charge_state").Result)
								{
									if (!ValidateApiReponse(chargeStateResponse, "GET charge_state")) return;

									var chargeStateJson = chargeStateResponse.Content.ReadAsStringAsync().Result;
									var chargeState = JsonConvert.DeserializeObject<TeslaResponse<ChargeState>>(chargeStateJson).Content;

									// Send reminder if not connected to charger.
									if (chargeState.charging_state == "Disconnected")
									{
										// Log/send alert.
										var message = $"{vehicle.display_name}'s battery is at {chargeState.battery_level}% with a range of {chargeState.battery_range} miles. You may want to plug in tonight.";

										fileLogger.Warn(message);
										smtpLogger.Warn(message);

										lastReminderSentAt = DateTime.Now;
									}
									else
									{
										fileLogger.Info("Vehicle is plugged in.");
									}
								}

							}
							else
							{
								fileLogger.Info("Vehicle is not at home.");
							}

						}

					}

				}
					

			}

		}


		protected bool ValidateApiReponse(HttpResponseMessage response, string apiCallDescription)
		{
			if (!response.IsSuccessStatusCode)
			{
				var errorMessage = $"Failed to {apiCallDescription}. \nDetails: " + response.Content.ReadAsStringAsync().Result;

                fileLogger.Error(errorMessage);
                SmtpLogError(errorMessage);

                return false;
			}

			return true;
		}


        protected void SmtpLogError(string errorMessage) {
            if (!trackedErrors.ContainsKey(errorMessage)) {
                // First error instance.
                // Send email notification.
                smtpLogger.Error(errorMessage);

                // Track new error.
                trackedErrors.Add(errorMessage, DateTime.Now);
            }
            else {
                // Error instance exists.
                // Check how recently notified about it.
                var lastNotified = trackedErrors[errorMessage];

                if (DateTime.Now.Subtract(lastNotified).Minutes >= 30) {
                    // It's been half an hour since last notification was sent. Send another.
                    smtpLogger.Error(errorMessage);

                    // Update last sent.
                    trackedErrors[errorMessage] = DateTime.Now;
                }
            }
        }


        protected void CleanupOldErrors() {
            var errorKeys = new string[0];
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
