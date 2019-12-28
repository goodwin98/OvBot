using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace OvBot.Settings
{
    class Settings
    {
        public String BotToken { get; set; }
        public String Prefix { get; set; }

        public DiscordClient discordClient { get; set; }

        private static readonly Settings instance = new Settings();


        private Settings()
        {
            BotToken = "NjU5Njg0MTkyOTg4MTY4MTk2.XgR7MQ.fiavP6I0_ZPZCqlMNAP5jytG6_Q";
            Prefix = "!";
        }

        public static Settings GetInstance()
        {
            return instance;
        }
    }
}
