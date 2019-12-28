using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace OvBot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;
        private static System.Threading.Timer timer;
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("log.txt",
                fileSizeLimitBytes:50*1024*1024,
                retainedFileCountLimit:9,
                rollOnFileSizeLimit: true)
            .CreateLogger();
            Log.Information("Starting up!");

            using (OvBot.OwStats.DataBase.Context db = new OvBot.OwStats.DataBase.Context() )
            {
                db.Database.EnsureCreated();
                OvBot.OwStats.DataBase.UsersBattleTag battleTag = new OvBot.OwStats.DataBase.UsersBattleTag { UserId = 223528667874197504, BattleTag = "YxoProgress-2327" };
                db.Users.Add(battleTag);
                try
                {
                    db.SaveChanges();
                    Log.Information("Added first line in BattleTag DataBase");
                } catch(DbUpdateException)
                {
                    
                }
            }
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            Settings.Settings s = Settings.Settings.GetInstance();
            discord = new DiscordClient(new DiscordConfiguration
            {
                UseInternalLogHandler = false,
                LogLevel = LogLevel.Debug,
                Token = s.BotToken,
                TokenType = TokenType.Bot
            });
            Settings.Settings.GetInstance().discordClient = discord;
            discord.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = s.Prefix
            });
            commands.RegisterCommands<MyCommands>();
            
            TimerCallback timeCB = new TimerCallback(MyCommands.Every6Hourevent);

            timer = new Timer(timeCB, null, 20000, 6 * 3600 * 1000);

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
        private static void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            if(e.Level == LogLevel.Debug)
            {
                Log.Debug($"[{e.Application}] {e.Message}");
            } else if(e.Level == LogLevel.Error)
            {
                Log.Error($"[{e.Application}] {e.Message}");
            } else if (e.Level == LogLevel.Critical)
            {
                Log.Fatal($"[{e.Application}] {e.Message}");
            } else if (e.Level == LogLevel.Info)
            {
                Log.Information($"[{e.Application}] {e.Message}");
            } else if (e.Level == LogLevel.Warning)
            {
                Log.Warning($"[{e.Application}] {e.Message}");
            }
        }

    }
}
