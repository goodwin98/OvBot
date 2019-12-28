using System;
using System.Collections.Generic;
using System.Text;

namespace OvBot.OwStats.DataBase
{
    class UsersBattleTag
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public String BattleTag { get; set; }
    }
}
