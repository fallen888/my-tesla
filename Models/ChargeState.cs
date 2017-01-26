using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyTesla.Models
{
	public class ChargeState
	{
		public string charging_state { get; set; }
		public int? charge_limit_soc { get; set; }
		public int? charge_limit_soc_std { get; set; }
		public int? charge_limit_soc_min { get; set; }
		public int? charge_limit_soc_max { get; set; }
		public bool? charge_to_max_range { get; set; }
		public bool? battery_heater_on { get; set; }
		public bool? not_enough_power_to_heat { get; set; }
		public int? max_range_charge_counter { get; set; }
		public bool? fast_charger_present { get; set; }
		public string fast_charger_type { get; set; }
		public double?  battery_range { get; set; }
		public double?  est_battery_range { get; set; }
		public double?  ideal_battery_range { get; set; }
		public int? battery_level { get; set; }
		public int? usable_battery_level { get; set; }
		public double?  battery_current { get; set; }
		public double?  charge_energy_added { get; set; }
		public double?  charge_miles_added_rated { get; set; }
		public double?  charge_miles_added_ideal { get; set; }
		public int? charger_voltage { get; set; }
		public int? charger_pilot_current { get; set; }
		public int? charger_actual_current { get; set; }
		public int? charger_power { get; set; }
		public double?  time_to_full_charge { get; set; }
		public bool? trip_charging { get; set; }
		public double?  charge_rate { get; set; }
		public bool? charge_port_door_open { get; set; }
		public object scheduled_charging_start_time { get; set; }
		public bool? scheduled_charging_pending { get; set; }
		public object user_charge_enable_request { get; set; }
		public bool? charge_enable_request { get; set; }
		public object charger_phases { get; set; }
		public string charge_port_latch { get; set; }
		public int? charge_current_request { get; set; }
		public int? charge_current_request_max { get; set; }
		public string charge_port_led_color { get; set; }
		public bool? managed_charging_active { get; set; }
		public bool? managed_charging_user_canceled { get; set; }
		public object managed_charging_start_time { get; set; }
		public bool? motorized_charge_port { get; set; }
		public bool? eu_vehicle { get; set; }
		public long? timestamp { get; set; }
	}
}
