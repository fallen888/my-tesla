using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using log4net.Config;


namespace MyTesla
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase.Run(new ReminderService());
		}

	}
}
