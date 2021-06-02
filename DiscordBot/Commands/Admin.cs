using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Admin : BaseCommandModule
    {
        [Command("shutdown")]
        [Description("stops the bot")]
        [RequireOwner()]
        public async Task Shutdown(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var shutdownMsg = context.RespondAsync($"i am going to shutdown - bye {context.User.Mention}");
            shutdownMsg.Wait();
            System.Threading.Thread.Sleep(1000);
            await context.Message.DeleteAsync();
            await shutdownMsg.Result.DeleteAsync();
            await context.Client.DisconnectAsync();
            Environment.Exit(0);
        }

        [Command("version")]
        [Description("shows the current bot version number")]
        [RequireOwner()]
        public async Task Version(CommandContext context)
        {
            await context.TriggerTypingAsync();
            await context.RespondAsync($"Version: {Environment.Version.ToString()}");
        }

        [Command("play")]
        [Description("changes the information what the bot is playing")]
        [Aliases("activity")]
        [RequireOwner()]
        [RequireGuild()]
        public async Task Play(CommandContext context,[Description("name of the game the bot should play")] params string[] game)
        {
            await context.TriggerTypingAsync();

            await context.Client.UpdateStatusAsync(new DiscordActivity(string.Join(" ", game)));

            var doneMsg = context.RespondAsync("Done");
            await doneMsg;

            System.Threading.Thread.Sleep(2000);
            await context.Channel.DeleteMessagesAsync(new List<DiscordMessage>() { doneMsg.Result, context.Message });
        }
    }
}
