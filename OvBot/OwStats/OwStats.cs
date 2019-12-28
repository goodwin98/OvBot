using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OvBot.OwStats.DataBase;

namespace OvBot.OwStats
{
    class OwStats
    {
        public async Task DisplayStat(CommandContext ctx)
        {
            using Context db = new Context();
            var result = db.Users.Where(u => u.UserId == ctx.Member.Id);
            if (result.Count() != 0)
            {
                DisplayStat(ctx, result.First().BattleTag);
            } else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Description = $"{ctx.Member.Mention} нет связанного battletag'a"
                };
                await ctx.RespondAsync(embed: embed);
            }
        }
        public async Task<OwStat> DisplayStat(CommandContext ctx, String tag)
        {
            await ctx.TriggerTypingAsync();
            var embed = new DiscordEmbedBuilder
            {
                Description = "Зпрос отправлен, ждем ответа..."
            };
            var mess = await ctx.RespondAsync(embed: embed);
            OwStat owStat;
            try
            {
                owStat = OwApiModel.getFromService("eu", tag, "pc").Result.GetOwStat();
                await ctx.TriggerTypingAsync();
                embed = new DiscordEmbedBuilder
                {
                    Title = "Статистика игрока " + owStat.Name
                };

                String textScore;
                if (owStat.Priv)
                {
                    textScore = "Закрытый профиль.\n\nВнимание! У вас закрыт профиль! Откройте его в настройках игры(Настройки -> Общение)";
                }
                else
                {
                    textScore = "‣ " + GetEmodjiByRank(ctx.Client, owStat.TankLevelShort) + "Танк: " + (owStat.TankLevel != 0 ? owStat.TankLevel.ToString() : "Неизвестно") + "\n";
                    textScore += "‣ " + GetEmodjiByRank(ctx.Client, owStat.DamageLevelShort) + "Урон: " + (owStat.DamageLevel != 0 ? owStat.DamageLevel.ToString() : "Неизвестно") + "\n";
                    textScore += "‣ " + GetEmodjiByRank(ctx.Client, owStat.SupportLevelShort) + "Поддержка: " + (owStat.SupportLevel != 0 ? owStat.SupportLevel.ToString() : "Неизвестно") + "\n";
                }
                embed.AddField(owStat.Name, textScore);
                await mess.ModifyAsync(embed: embed);
                return owStat;
            } catch(Exception)
            {
                embed = new DiscordEmbedBuilder
                {
                    Description = "Неправильный BattleTag, либо какая-то ошибка с сетью. Проверьте правильность написания или попробуйте позднее."
                };
                await mess.ModifyAsync(embed: embed);
                return null;
            }
        }
        private DiscordEmoji GetEmodjiByRank(DiscordClient client, Rank rank)
        {
            if (rank == Rank.Bronze)
            {
                return DiscordEmoji.FromGuildEmote(client, 656419045410734101);
            }
            else if (rank == Rank.Silver)
            {
                return DiscordEmoji.FromGuildEmote(client, 656419439721578516);
            }
            else if (rank == Rank.Gold)
            {
                return DiscordEmoji.FromGuildEmote(client, 656419046329155604);
            }
            else if (rank == Rank.Platinum)
            {
                return DiscordEmoji.FromGuildEmote(client, 656419046362841108);
            }
            else if (rank == Rank.Diamond)
            {
                return DiscordEmoji.FromGuildEmote(client, 656419046610436109);
            }
            else if (rank == Rank.Master)
            {
                return DiscordEmoji.FromGuildEmote(client, 656419044592844835);
            }
            else if (rank == Rank.Grandmaster)
            {
                return DiscordEmoji.FromGuildEmote(client, 656419044571873281);
            }
            return DiscordEmoji.FromName(client, ":grey_question:");
        }

        private static async Task ClearDiscordUserRoles(DiscordGuild guild, DiscordMember member)
        {
            IReadOnlyList<DiscordRole> guildDiscordRoles = guild.Roles;
            IEnumerable<DiscordRole> oldDiscordRoles = member.Roles;
            List<DiscordRole> newDiscordRoles = new List<DiscordRole>();
            foreach (DiscordRole discordRole in oldDiscordRoles)
            {
                if ((discordRole.Name.Contains("Support") || discordRole.Name.Contains("Tank") || discordRole.Name.Contains("Damage")) &&
                    Enum.GetNames(typeof(Rank)).Any(discordRole.Name.Contains))
                {
                    continue;
                }
                newDiscordRoles.Add(discordRole);
            }
            await member.ReplaceRolesAsync(newDiscordRoles);
        }
        private static async Task UpdateDiscordUserRoles(OwStat owStat, DiscordGuild guild, DiscordMember member)
        {
            if (owStat != null)
            {
                IReadOnlyList<DiscordRole> guildDiscordRoles = guild.Roles;
                IEnumerable<DiscordRole> oldDiscordRoles = member.Roles;
                List<DiscordRole> newDiscordRoles = new List<DiscordRole>();
                foreach (DiscordRole discordRole in oldDiscordRoles)
                {
                    if ((discordRole.Name.Contains("Support") || discordRole.Name.Contains("Tank") || discordRole.Name.Contains("Damage")) &&
                        Enum.GetNames(typeof(Rank)).Any(discordRole.Name.Contains))
                    {
                        continue;
                    }
                    newDiscordRoles.Add(discordRole);
                }
                foreach (DiscordRole discordRole in guildDiscordRoles)
                {
                    if (owStat.SupportLevel != 0)
                    {
                        if (discordRole.Name.Contains("Support") && discordRole.Name.Contains(Enum.GetName(typeof(Rank), owStat.SupportLevelShort)))
                        {
                            newDiscordRoles.Add(discordRole);
                        }
                    }
                    if (owStat.TankLevel != 0)
                    {
                        if (discordRole.Name.Contains("Tank") && discordRole.Name.Contains(Enum.GetName(typeof(Rank), owStat.TankLevelShort)))
                        {
                            newDiscordRoles.Add(discordRole);
                        }
                    }
                    if (owStat.DamageLevel != 0)
                    {
                        if (discordRole.Name.Contains("Damage") && discordRole.Name.Contains(Enum.GetName(typeof(Rank), owStat.DamageLevelShort)))
                        {
                            newDiscordRoles.Add(discordRole);
                        }
                    }
                }
                await member.ReplaceRolesAsync(newDiscordRoles);
            }
        }

        public async Task DelBattleTag(CommandContext ctx)
        {
            using Context db = new Context();
            var result = db.Users.Where(u => u.UserId == ctx.Member.Id);
            if (result.Count() != 0)
            {
                Log.Debug("Deleting tag for user {User}", ctx.Member.DisplayName);
                var tag = result.FirstOrDefault().BattleTag;
                db.Users.Remove(result.FirstOrDefault());
                try
                {
                    db.SaveChanges();
                    await ctx.TriggerTypingAsync();
                    var embed = new DiscordEmbedBuilder
                    {
                        Description = $"BattleTag {tag} удалён"
                    };
                    var mess = await ctx.RespondAsync(embed: embed);
                    Log.Debug("Clear role for user {User}", ctx.Member.DisplayName);
                    await ClearDiscordUserRoles(ctx.Guild, ctx.Member);
                }
                catch (DbUpdateException e)
                {
                    Log.Error(e, "Error with deleting user's battletag from db. UserId: {id}",ctx.Member.Id);
                    await ctx.TriggerTypingAsync();
                    var embed = new DiscordEmbedBuilder
                    {
                        Description = "Произошла ошибка при удалении battleteg. Попробуйте позже."
                    };
                    var mess = await ctx.RespondAsync(embed: embed);
                }
            }
        }
        public async Task AddBattleTag(CommandContext ctx, String tag)
        {
            using Context db = new Context();
            tag = tag.Replace("#", "-");
            var result = db.Users.Where(u => u.BattleTag == tag);
            if (result.Count() != 0)
            {
                await ctx.TriggerTypingAsync();
                var embed = new DiscordEmbedBuilder
                {
                    Description = "Этот battletag уже привязан к пользователю " + ctx.Client.GetUserAsync(result.First().UserId).Result.Mention
                };
                var mess = await ctx.RespondAsync(embed: embed);
                return;
            }
            result = db.Users.Where(u => u.UserId == ctx.Member.Id);
            if (result.Count() != 0)
            {
                await ctx.TriggerTypingAsync();
                var embed = new DiscordEmbedBuilder
                {
                    Description = ctx.Member.Mention + " к вам уже привязан BattleTag " + result.First().BattleTag
                };
                var mess = await ctx.RespondAsync(embed: embed);
                return;
            }
           
            OwStat owStat = await DisplayStat(ctx, tag);

            await UpdateDiscordUserRoles(owStat, ctx.Guild, ctx.Member);
            UsersBattleTag battleTag = new UsersBattleTag { UserId = ctx.Member.Id, BattleTag = tag };
            db.Users.Add(battleTag);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                Log.Error(e, "Error with saving battletag {tag} to db", tag);
            }
        }
        public static async Task UpdateAllRoles()
        {
            using Context db = new Context();
            Settings.Settings settings = Settings.Settings.GetInstance();
            Log.Information("Update all roles");
            foreach(KeyValuePair<ulong,DiscordGuild> entry in settings.discordClient.Guilds)
            {
                Log.Information("Update roles for members guild {Id} ", entry.Key);
                foreach(DiscordMember discordMember in (await entry.Value.GetAllMembersAsync()).Where(u=> !u.IsBot))
                {
                    try
                    {
                        var resultDb = db.Users.Where(u => u.UserId == discordMember.Id);
                        if(resultDb.Count() != 0)
                        {
                            OwStat owStat;
                            owStat = (await OwApiModel.getFromService("eu",resultDb.First().BattleTag, "pc")).GetOwStat();
                            if (owStat.Priv)
                            {
                                Log.Debug("Clear role for user {User}", discordMember.DisplayName);
                                await ClearDiscordUserRoles(entry.Value, discordMember);
                            }
                            Log.Debug("Update roles for user {User}", discordMember.DisplayName);
                            await UpdateDiscordUserRoles(owStat, entry.Value, discordMember);
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error with update role for user.\n User: {UserId}, Guild: {GuildId}", discordMember.DisplayName, entry.Key);
                        continue;
                    }
                }
                Log.Debug("End of list guild. Sum {S} members.", (await entry.Value.GetAllMembersAsync()).Where(u => !u.IsBot).Count());
            }
        }
    }
}
