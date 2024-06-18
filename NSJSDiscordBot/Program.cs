using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;


// [assembly: AssemblyVersion("0.10.0.0")]
// [assembly: AssemblyFileVersion("0.10.0.0")]
// [assembly: AssemblyInformationalVersion("0.10.0.0")]
namespace NSJSDiscordBot
{

	// =============================================================================================================================================================================

	public class Program
	{


		public static void Main(string[] args) // =1=1=1=1=1=1=1=1=11=1=1=1=111111111111111111====1===1=11=1=1=1=1=1=1=1=1=1=1================
		{
			// since we cannot make the entry method asynchronous,
			// let's pass the execution to asynchronous code
			//using (Process p = Process.GetCurrentProcess())
			//    p.PriorityClass = ProcessPriorityClass.High;
			NSJSUtil.Print($"BOOTING NSJS BOT V{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}.{Assembly.GetExecutingAssembly().GetName().Version.Revision}", ConsoleColor.DarkRed);
			var bot = new BotProgram();
			bot.Update();
			bot.RunBotAsync().GetAwaiter().GetResult();
		}


	}
}