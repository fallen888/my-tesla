using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace MyTesla
{
	public class Messenger
	{
		private static Action<string> OnCompletion;

		public static void SendText(string number, string message, Action<string> onCompletion)
		{
			OnCompletion = onCompletion;

			string content = "number=" + number + "&message=" + HttpUtility.UrlEncode(message);
			byte[] data = Encoding.UTF8.GetBytes(content);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://textbelt.com/text");
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";

			request.BeginGetRequestStream(result =>
			{
				using (Stream dataStream = request.EndGetRequestStream(result))
				{
					dataStream.Write(data, 0, data.Length);
				}

				request.BeginGetResponse(stringResult =>
				{
					HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(stringResult);
					using (StreamReader reader = new StreamReader(response.GetResponseStream()))
					{
						string serverReturned = reader.ReadToEnd();
						OnCompletion(serverReturned);
					}
				}, null);

			}, null);


		}

	}
}
