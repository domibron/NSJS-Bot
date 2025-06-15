using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using NSJSDiscordBot.Bot_Core;
using NSJSDiscordBot.DataFiles.BotTokensAndKeys;
using NSJSDiscordBot.Util;


namespace NSJSDiscordBot
{


	public class Program
	{


		public static void Main(string[] args)
		{
            /* OLD NSJS CODE, PLANNED FOR REMOVAL
			//// since we cannot make the entry method asynchronous,
			//// let's pass the execution to asynchronous code
			////using (Process p = Process.GetCurrentProcess())
			////    p.PriorityClass = ProcessPriorityClass.High;
			//NSJSUtil.Print($"BOOTING NSJS BOT V{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}.{Assembly.GetExecutingAssembly().GetName().Version.Revision}", ConsoleColor.DarkRed);
			//var bot = new BotProgram();
			//bot.Update();
			//bot.RunBotAsync().GetAwaiter().GetResult();

			//Console.WriteLine("Exited out of program, This should not happen...");
			
			END
			 */

            Util.PrintToConsole.Print("Starting app");
            Util.PrintToConsole.Print("Reading files");



            // this should just go into its own class.
            Util.PrintToConsole.Print("Reading tokens and keys file");

            Task tokenAndKeysTask = TokenAndKeysFileManager.GetReadTokenAndKeys();
            tokenAndKeysTask.Wait();


            //Util.PrintToConsole.Print("Reading discord config file");

            //Task discordConfigTask = DiscordConfigManager.GetReadDiscordConfig();
            //discordConfigTask.Wait();


            //Util.PrintToConsole.Print("Reading github config file");

            //Task githubConfigTask = GithubConfigManager.GetReadDiscordConfig();
            //githubConfigTask.Wait();


            //Util.PrintToConsole.Print("Reading issue messages file");

            //Task IssueMessagesTask = IssueMessagesFileManager.GetReadIssueMessages();
            //IssueMessagesTask.Wait();


            //Util.PrintToConsole.Print("Reading economy file");

            //Task EconomyTask = EconomyFileManager.ReadEconomyFile();
            //EconomyTask.Wait();

            //Util.PrintToConsole.Print("Reading index message file");

            //Task IndexMessageTask = IndexMessageFileManager.ReadIndexMessageFile();
            //IndexMessageTask.Wait();
            // end of comment


            //Util.PrintToConsole.Print("Starting github api and discord bot");
            Util.PrintToConsole.Print("Starting discord bot");

            //GithubService githubService = new GithubService();
            //Task githubServiceTask = githubService.SetupClientAsync();


            BotCore botCore = new BotCore();
            Task discordBot = botCore.InitAndStartBotAsync();


            //AutoUpdateIssueMessages autoUpdateIssueMessages = new AutoUpdateIssueMessages();
            //Task autoUpdateIssueMessagesTask = autoUpdateIssueMessages.StartAutoUpdatingIssues();


            TimeSystem time = new TimeSystem();
            Task updateTime = time.UpdateDeltaTime();
            Task.Run(updateTime.GetAwaiter);

            //Task.WaitAll(githubServiceTask, discordBot, autoUpdateIssueMessagesTask);
            Task.WaitAll(discordBot);

        }


	}
}