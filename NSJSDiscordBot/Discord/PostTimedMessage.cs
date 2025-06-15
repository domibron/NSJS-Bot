using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using NSJSDiscordBot.DataFiles.TimedMessageFileManager;
using NSJSDiscordBot.Util;

namespace NSJSDiscordBot.Discord
{
    public class PostTimedMessage
    {
        // every 10 seconds we want to check the time. so we dont mag dump util.
        public const int WaitTime = 10000;

        public async Task StartSystem()
        {
            PrintToConsole.Print(ConsoleColor.Yellow, $"Activating auto issue updator");


            while (BotCore.NSJSDiscordClient == null)
            {
                PrintToConsole.Print(ConsoleColor.Red, $"No response from Discord bot. waiting for activation!");
                await Task.Delay(5000);
            }

            while (true)
            {
                if (BotCore.NSJSDiscordClient == null)
                {
                    await Task.Delay(WaitTime);
                    continue;
                }


                foreach (var message in TimedMessageFileManager.TimedMessages)
                {
                    //d1 < 0 − If date1 is earlier than date2
                    //d1 = 0 − If date1 is the same as date2
                    //d1 > 0 − If date1 is later than date2
                    if (DateTime.Compare(message.Time, DateTime.Now) <= 0)
                    {
                        //string rsm = StoreData.Messages[i];
                        //DateTime rdt = StoreData.Times[i];
                        //ulong rdc = StoreData.ChannelIDs[i];

                        DiscordChannel discordChannelToPostMessage = await BotCore.NSJSDiscordClient.GetChannelAsync(message.ChannelID); // I want to send the message even if its late.
                        await BotCore.NSJSDiscordClient.SendMessageAsync(discordChannelToPostMessage, message.Message);

                        await TimedMessageFileManager.RemoveTimedMessage(message);

                        NSJSUtil.Print("I sent the message and removed it from the timed message store.", ConsoleColor.DarkYellow);
                    }
                }

                await Task.Delay(WaitTime);
            }
        }
    }
}
