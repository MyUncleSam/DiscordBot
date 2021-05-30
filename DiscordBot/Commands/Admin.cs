using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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
    }
}
