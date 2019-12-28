using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Net;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using Serilog;


namespace OvBot
{
    class MyCommands
    {
        readonly OwStats.OwStats owStats = new OwStats.OwStats();
        [Command("ping")] // let's define this method as a command
        [Description("Просто пинг")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("pong")] // alternative names for the command
        public async Task Ping(CommandContext ctx) // this command takes no arguments
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // let's make the message a bit more colourful
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

            // respond with ping
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
        }


        [Command("показать"), Description("показать чью-то статистику")]
        public async Task OvStats(CommandContext ctx, [Description("батлтег для показа")]String tag)
        {
            await owStats.DisplayStat(ctx, tag);
        }

        [Command("показать_моё"), Description("показать свою статистику")]
        public async Task OvStats(CommandContext ctx)
        {
            await owStats.DisplayStat(ctx);
        }

        [Command("добавить"), Description("Добавление батлтега")]
        public async Task OwAdd(CommandContext ctx, [Description("батлтег для добавления")] String tag)
        {
            try
            {
                await owStats.AddBattleTag(ctx, tag);
            } catch (Exception e)
            {
                Log.Error(e, "with OwAdd");
            }
        }

        [Command("удалить"), Description("Удаление связанного батлтега")]
        public async Task OwDelete(CommandContext ctx)
        {
            try
            {
                await owStats.DelBattleTag(ctx);
            }
            catch (Exception e)
            {
                Log.Error(e, "with OwDelete");
            }
        }

        [Command("update")]
        [Hidden]
        [Require​Owner]
        [Require​User​Permissions(Permissions.BanMembers)]
        public async Task UpdateRoles(CommandContext ctx)
        {
            await OwStats.OwStats.UpdateAllRoles();
        }

        [Command("pepe"), Aliases("feelsbadman"), Description("Feels bad, man.")]
        [Hidden]
        [Require​Owner]
        [Require​User​Permissions(Permissions.BanMembers)]
        public async Task Pepe(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            // wrap it into an embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "Pepe",
                ImageUrl = "http://i.imgur.com/44SoSqS.jpg"
            };
            await ctx.RespondAsync(embed: embed);
        }
        public static void Every6Hourevent(object state)
        {
            Log.Information("Start every 6 hour event");
            OwStats.OwStats.UpdateAllRoles();
            Log.Information("End of every 6 hour event");
        }
    }
}
