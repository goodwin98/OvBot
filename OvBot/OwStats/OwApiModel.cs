using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OvBot.OwStats
{
    class OwApiModel
    {
        static readonly HttpClient client = new HttpClient();

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("private")]
        public bool Private { get; set; }
        [JsonProperty("ratings")]
        public Rating[] Ratings { get; set; }

        public static async Task<OwApiModel> getFromService(String region, String battleTag, String platform)
        {
            battleTag = battleTag.Replace('#', '-');
            //var data = (await client.GetAsync($"https://ow-api.com/v1/stats/{platform}/{region}/{battleTag}/profile")).EnsureSuccessStatusCode();
            //OwApiModel owApiModel = JsonConvert.DeserializeObject<OwApiModel>(
             //   await data.Content.ReadAsStringAsync());
            using WebClient wc = new WebClient();
            var json = await wc.DownloadStringTaskAsync($"https://ow-api.com/v1/stats/{platform}/{region}/{battleTag}/profile");
            OwApiModel owApiModel = JsonConvert.DeserializeObject<OwApiModel>(json);
            return owApiModel;
        }
        public OwStat GetOwStat()
        {
            OwStat owStat = new OwStat
            {
                Name = Name
            };
            if(Private)
            {
                owStat.Priv = true;
                return owStat;
            }
            if (Ratings != null)
            {
                foreach (Rating rating in Ratings)
                {
                    var shortLevel = Rank.Nothing;
                    if (rating.RankIcon.OriginalString.Contains("Bronze"))
                    {
                        shortLevel = Rank.Bronze;
                    }
                    else if (rating.RankIcon.OriginalString.Contains("Silver"))
                    {
                        shortLevel = Rank.Silver;
                    }
                    else if (rating.RankIcon.OriginalString.Contains("Gold"))
                    {
                        shortLevel = Rank.Gold;
                    }
                    else if (rating.RankIcon.OriginalString.Contains("Platinum"))
                    {
                        shortLevel = Rank.Platinum;
                    }
                    else if (rating.RankIcon.OriginalString.Contains("Diamond"))
                    {
                        shortLevel = Rank.Diamond;
                    }
                    else if (rating.RankIcon.OriginalString.Contains("Master"))
                    {
                        shortLevel = Rank.Master;
                    }
                    else if (rating.RankIcon.OriginalString.Contains("Grandmaster"))
                    {
                        shortLevel = Rank.Grandmaster;
                    }
                    if (rating.Role.Equals("tank"))
                    {
                        owStat.TankLevel = (int)rating.Level;
                        owStat.TankLevelShort = shortLevel;
                    }
                    else if (rating.Role.Equals("damage"))
                    {
                        owStat.DamageLevel = (int)rating.Level;
                        owStat.DamageLevelShort = shortLevel;
                    }
                    else if (rating.Role.Equals("support"))
                    {
                        owStat.SupportLevel = (int)rating.Level;
                        owStat.SupportLevelShort = shortLevel;
                    }
                }
            }
            return owStat;
        }
    }
    public partial class Rating
    {
        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("roleIcon")]
        public Uri RoleIcon { get; set; }

        [JsonProperty("rankIcon")]
        public Uri RankIcon { get; set; }
    }

    public class OwStat
    {
        public String Name { get; set; }
        public int TankLevel { get; set; }
        public int DamageLevel { get; set; }
        public int SupportLevel { get; set; }
        public Rank TankLevelShort { get; set; }
        public Rank DamageLevelShort { get; set; }
        public Rank SupportLevelShort { get; set; }
        public bool Priv { get; set; }
    }
   
    public enum Rank
    {
        Nothing = 0,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        Grandmaster
    }
}
