using System;
using System.Collections.Generic;
using System.Configuration;
using System.Device.Location;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Timers;

using Newtonsoft.Json;

using MyTesla.Models;


namespace MyTesla
{
	public partial class ReminderService : ServiceBase
	{
		private Timer timer = new Timer();
		private string homeCheckTime = ConfigurationManager.AppSettings["home_check_time"];
		private int homeCheckHour = 0;
		private int homeCheckMinute = 0;
		private string homeCheckAMorPM = null;

		private string TESLA_CLIENT_ID = ConfigurationManager.AppSettings["TESLA_CLIENT_ID"];
		private string TESLA_CLIENT_SECRET = ConfigurationManager.AppSettings["TESLA_CLIENT_SECRET"];
		private string EMAIL = ConfigurationManager.AppSettings["email"];
		private string PASSWORD = ConfigurationManager.AppSettings["password"];
		private string PHONE_NUMBER = ConfigurationManager.AppSettings["phone"];
		private double home_lat = Convert.ToDouble(ConfigurationManager.AppSettings["home_lat"]);
		private double home_long = Convert.ToDouble(ConfigurationManager.AppSettings["home_long"]);
		private int reminder_interval = Convert.ToInt32(ConfigurationManager.AppSettings["reminder_interval"]);

		private Uri baseAddress = new Uri("https://owner-api.teslamotors.com/");

		private string access_token = null;
		private DateTime accessTokenExpirationDate = DateTime.Now.AddMonths(1);

		private DateTime lastReminderSentAt = new DateTime(2000, 1, 1);

		private string AccessToken
		{
			get
			{
				if (access_token == null || DateTime.Now >= accessTokenExpirationDate)
				{
					// Log in.
					using (var client = new HttpClient { BaseAddress = baseAddress })
					{
						using (var content = new StringContent(String.Empty))
						{
							var requestUri = $"oauth/token?grant_type=password&client_id={TESLA_CLIENT_ID}&client_secret={TESLA_CLIENT_SECRET}&email={EMAIL}&password={PASSWORD}";

							using (var response = client.PostAsJsonAsync<object>(requestUri, content).Result)
							{
								string responseData = response.Content.ReadAsStringAsync().Result;
								var model = JsonConvert.DeserializeObject<LoginResponse>(responseData);
								access_token = model.access_token;
								accessTokenExpirationDate = DateTime.Now.AddSeconds(Convert.ToInt32(model.expires_in));
							}
						}
					}
				}

				return access_token;
			}
		}


		public ReminderService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			var timeParts = homeCheckTime.Split(" ".ToCharArray());
			var hourMinParts = timeParts[0].Split(":".ToCharArray());
			homeCheckHour = Convert.ToInt32(hourMinParts[0]);
			homeCheckMinute = Convert.ToInt32(hourMinParts[1]);
			homeCheckAMorPM = timeParts[1];
			
			timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
			//providing the time in miliseconds 
			timer.Interval = 60000; // 1 minute
			timer.AutoReset = true;
			timer.Enabled = true;
			timer.Start();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (DateTime.Now >= (lastReminderSentAt.AddMinutes(reminder_interval)))
			{
				var homeCheckTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, (homeCheckAMorPM == "PM" ? homeCheckHour + 12 : homeCheckHour), homeCheckMinute, 0);

				if (DateTime.Now >= homeCheckTime)
				{
					DoChargingCheck();
				}
			}
		}
		
		protected override void OnContinue()
		{
			base.OnContinue();
			timer.Start();
		}

		protected override void OnPause()
		{
			base.OnPause();
			timer.Stop();
		}

		protected override void OnShutdown()
		{
			base.OnShutdown();
			timer.Stop();
		}

		protected override void OnStop()
		{
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
					if (vehiclesResponse.IsSuccessStatusCode)
					{
						var vehiclesJson = vehiclesResponse.Content.ReadAsStringAsync().Result;
						var vehicles = JsonConvert.DeserializeObject<TeslaResponse<List<Vehicle>>>(vehiclesJson).Content;

						if (vehicles.Count > 0)
						{
							var vehicle = vehicles[0];

							// Get vehicle's drive state.
							using (var driveStateResponse = client.GetAsync($"api/1/vehicles/{vehicle.id}/data_request/drive_state").Result)
							{
								if (driveStateResponse.IsSuccessStatusCode)
								{
									var driveStateJson = driveStateResponse.Content.ReadAsStringAsync().Result;
									var driveState = JsonConvert.DeserializeObject<TeslaResponse<DriveState>>(driveStateJson).Content;

									var sCoord = new GeoCoordinate(home_lat, home_long);
									var eCoord = new GeoCoordinate(driveState.latitude.Value, driveState.longitude.Value);

									var distanceFromHome = sCoord.GetDistanceTo(eCoord);    // in meters.

									// Is vehicle close to home?
									if (distanceFromHome <= 50)
									{
										// Get vehicle's charge state.
										using (var chargeStateResponse = client.GetAsync($"api/1/vehicles/{vehicle.id}/data_request/charge_state").Result)
										{
											if (chargeStateResponse.IsSuccessStatusCode)
											{
												var chargeStateJson = chargeStateResponse.Content.ReadAsStringAsync().Result;
												var chargeState = JsonConvert.DeserializeObject<TeslaResponse<ChargeState>>(chargeStateJson).Content;

												if (chargeState.charging_state == "Disconnected")
												{
													Messenger.SendText(PHONE_NUMBER,
																	   $"{vehicle.display_name}'s battery is at {chargeState.battery_level}% with a range of {chargeState.battery_range} miles. You may want to plug in tonight.",
																	   delegate (string response)
													{
														Console.WriteLine($"Response: {response}");
													});

													lastReminderSentAt = DateTime.Now;
												}
											}
										}

									}
								}
							}

						}
					}
				}
					

			}

		}

	}
}
