using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Public : BaseCommandModule
    {
        [Command("ping")]
        [Description("checks the bot ping to the discord server")]
        [Aliases("pong")]
        [RequireDirectMessage()]
        public async Task Ping(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(context.Client, ":ping_pong:");
            var message = await context.RespondAsync($"{emoji} Pong! Ping: {context.Client.Ping}ms");
        }

        [Command("time")]
        [Description("see the current bot time")]
        [Aliases("date")]
        [RequireDirectMessage()]
        public async Task Time(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(context.Client, ":timer:");
            var message = await context.RespondAsync($"{emoji} Servertime: {DateTime.Now} (UTC: {DateTime.UtcNow})");
        }


        [Command("whoami")]
        [Description("see the current bot time")]
        [Aliases("w")]
        [RequireDirectMessage()]
        public async Task WhoAmI(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(context.Client, ":information_source:");

            var author = context.Message.Author;

            var message = await context.RespondAsync($"{emoji} {author.Username} {author.Id} {author.IsBot} {author.IsSystem}");
        }
    }
}
