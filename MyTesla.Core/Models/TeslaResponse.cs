using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace MyTesla.Core.Models
{
	public class TeslaResponse<T> where T : class
	{
		[JsonProperty("response")]
		public T Content { get; set; }
	}
}
