using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Roles : BaseCommandModule
    {
        [Command("addgroup")]
        [Description("Adds a group to the given user")]
        [Aliases("ag", "addrole", "ar")]
        [RequireOwner()]
        [RequireGuild()]
        public async Task AddGroup(CommandContext context, [Description("the user to add the new group")] DiscordMember user, [Description("the role to add")] DiscordRole role)
        {
            await user.GrantRoleAsync(role);

            var doneMsg = context.RespondAsync("Done.");
            await doneMsg;

            System.Threading.Thread.Sleep(2000);
            await doneMsg.Result.DeleteAsync();
            await context.Message.DeleteAsync();
        }

        [Command("addfriend")]
        [Description("Adds the friend permission for the given user")]
        [Aliases("af", "friend")]
        [RequireOwner()]
        [RequireGuild()]
        public async Task AddGroup(CommandContext context, [Description("the user to add to the friends list")] DiscordMember user)
        {
            await AddGroup(context, user, context.Guild.Roles.Values.First(f => f.Name.Equals("Freund", StringComparison.OrdinalIgnoreCase)));

            //await user.SendMessageAsync($"Hallo {user.DisplayName}, willkommen auf {context.Guild.Name}!");
            //await user.SendMessageAsync($"Du wurdest freigeschaltet und kannst ab sofort alle channel sehen - viel Spaß!");

            var doneMsg = context.RespondAsync("Done.");
            await doneMsg;

            System.Threading.Thread.Sleep(2000);
            await doneMsg.Result.DeleteAsync();
            await context.Message.DeleteAsync();
        }
    }
}
