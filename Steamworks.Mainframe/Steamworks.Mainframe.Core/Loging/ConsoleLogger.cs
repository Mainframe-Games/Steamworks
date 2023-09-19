using System;

namespace Steamworks.Mainframe.Core
{
	public class ConsoleLogger : ILogger
	{
		public void Log(object message)
		{
			Console.WriteLine(message);
		}

		public void LogError(object message)
		{
			Console.Error.WriteLine(message);
		}

		public void LogWarning(object message)
		{
			Console.WriteLine(message);
		}
	}
}