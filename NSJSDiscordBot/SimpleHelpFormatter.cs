using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Builders;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Executors;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;

namespace NSJSDiscordBot
{
    // help formatters can alter the look of default help command,
    // this particular one replaces the embed with a simple text message.
    [Obsolete("Not using old command system", false)]
    public class SimpleHelpFormatter : BaseHelpFormatter
    {
        private DiscordEmbedBuilder MessageBuilder { get; }

        private StringBuilder StringBuilder { get; }

        public SimpleHelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.MessageBuilder = new DiscordEmbedBuilder();
        }

        // this method is called first, it sets the command
        public override BaseHelpFormatter WithCommand(Command command)
        {
            string placeHolder;

            if (command is CommandGroup) placeHolder = $"\n\nThis group has a standalone command.";
            else placeHolder = "";

            this.MessageBuilder.Title = $"Command: **{command.Name}**";

            this.MessageBuilder.Description = ($"\n\nDescription: {command.Description} {placeHolder} \n\nAliases: {string.Join(", ", command.Aliases)}");

            //this.MessageBuilder.Append("Command: ")
            //   .AppendLine(Formatter.Bold(command.Name))
            //   .AppendLine();


            //this.MessageBuilder.Append("Description: ")
            //    .AppendLine(command.Description)
            //    .AppendLine();

            //if (command is CommandGroup)
            //    this.MessageBuilder.AppendLine("This group has a standalone command.").AppendLine();

            //this.MessageBuilder.Append("Aliases: ")
            //    .AppendLine(string.Join(", ", command.Aliases))
            //    .AppendLine();


            foreach (var overload in command.Overloads)
            {
                if (overload.Arguments.Count == 0)
                {
                    continue;
                }

                this.MessageBuilder.Description += ($"\n\n[Overload {overload.Priority}] Arguments: { string.Join(", ", overload.Arguments.Select(xarg => $"{xarg.Name} ({xarg.Type.Name})"))}");

                //this.StringBuilder.Append($"[Overload {overload.Priority}] Arguments: ")
                //.AppendLine(string.Join(", ", overload.Arguments.Select(xarg => $"{xarg.Name} ({xarg.Type.Name})")))
                //.AppendLine();
            }

            return this;
        }

        // this method is called second, it sets the current group's subcommands
        // if no group is being processed or current command is not a group, it 
        // won't be called
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            this.MessageBuilder.Title = "Commands";
            this.MessageBuilder.Description = string.Join(", ", subcommands.Select(xc => xc.Name));

            //this.StringBuilder.Append("Subcommands: ")
            //    .AppendLine(string.Join(", ", subcommands.Select(xc => xc.Name)))
            //    .AppendLine();

            return this;
        }

        // this is called as the last method, this should produce the final 
        // message, and return it
        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage("", this.MessageBuilder);
        }
    }
}