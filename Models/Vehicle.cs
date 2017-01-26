using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyTesla.Models
{
	public class Vehicle
	{
		public long id { get; set; }
		public int vehicle_id { get; set; }
		public string vin { get; set; }
		public string display_name { get; set; }
		public string option_codes { get; set; }
		public object color { get; set; }
		public List<string> tokens { get; set; }
		public string state { get; set; }
		public object in_service { get; set; }
		public string id_s { get; set; }
		public bool? remote_start_enabled { get; set; }
		public bool? calendar_enabled { get; set; }
		public bool? notifications_enabled { get; set; }
		public object backseat_token { get; set; }
		public object backseat_token_updated_at { get; set; }
	}
}
