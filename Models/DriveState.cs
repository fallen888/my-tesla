using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyTesla.Models
{
	public class DriveState
	{
		public object shift_state { get; set; }
		public object speed { get; set; }
		public int? power { get; set; }
		public double? latitude { get; set; }
		public double? longitude { get; set; }
		public int? heading { get; set; }
		public int? gps_as_of { get; set; }
		public long? timestamp { get; set; }
	}
}
