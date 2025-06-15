using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using NSJSDiscordBot.DataFiles.TimedMessageFileManager;

namespace NSJSDiscordBot.Discord.SlashCommands
{
    public class NoCatCommands : ApplicationCommandModule
    {
        [SlashCommand("TimedMessage", "Delay a message that is to be sent"), SlashRequireGuild, SlashRequireOwner]
        public async Task TimedMessage(InteractionContext context, [Option("Mesage", "The string message")] string message, [Option("Channel", "The channel to senf the message")] DiscordChannel discordChannel = null, [Option("Time", "Time for when the message is sent")] TimeSpan? time = null, [Option("Year", "The year to send the message")] long year = 0, [Option("Month", "The month to send the message")] long month = 0, [Option("Day", "The day to send the message")] long day = 0)
        {
            if (discordChannel == null)
            {
                discordChannel = context.Channel;
                Console.WriteLine(discordChannel.ToString());
            }
            else
            {
                Console.WriteLine(discordChannel.ToString());
            }

            if (year == 0 || year == null)
            {
                year = DateTime.Now.Year;
                Console.WriteLine(year.ToString());
            }

            if (month == 0 || month == null)
            {
                month = DateTime.Now.Month;
                Console.WriteLine(month.ToString());
            }

            if (day == 0 || day == null)
            {
                //day = DateTime.Now.Day;

                // fix to set the day to the one in advance

                DateTime ldt = DateTime.Now;
                ldt = ldt.AddDays(1);

                day = ldt.Day;

                Console.WriteLine(day.ToString());
            }

            DateTime dt;

            if (time == null)
            {
                dt = new((int)year, (int)month, (int)day, 12, 0, 0);
                Console.WriteLine(dt.ToString());
            }
            else
            {
                dt = new((int)year, (int)month, (int)day, time.Value.Hours, time.Value.Minutes, time.Value.Seconds);
                Console.WriteLine(dt.ToString());
            }

            if (DateTime.Compare(dt, DateTime.Now) > 0)
            {
                await TimedMessageFileManager.AddTimedMessage(message, dt, discordChannel.Id);

                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"message will be sent at {dt} in {discordChannel.Name}").AsEphemeral());

                Console.WriteLine(dt.ToString());
            }
            else
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"the time you want to send the message is in the past! your time {dt}, current time {DateTime.Now}").AsEphemeral());
            }
        }
    }
}
