﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;

namespace RaidBot.Entities {
   [Serializable]
   public class Raid : IEquatable<Raid> {
      public string Name { get; set; }
      public DateTime? Time { get; set; }
      public DateTime? Day { get; set; }
      public DateTime ExpireStart { get; set; }
      public TimeSpan Expire { get; set; }
      public List<User> Users { get; set; }
      public DateTime CreateDateTime { get; set; }
      public int RaidBossId { get; set; }
      public string RaidBossName { get; set; }
      public string RaidBossSpriteUrl { get; set; }


      #region Methods
      public int UserCount
      {
         get
         {
            return Users.Count() + Users.Sum(a => a.GuestsCount);
         }
      }

      public bool Equals(Raid item) {
         return Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase);
      }

      public override string ToString() {
         string time = Time?.ToString("HH:mm");
         string day = Day?.ToString("yyyy'-'MM'-'dd");
         string result = $"{Name} {day} {time} {RaidBossName} (Expires {ToStringExpire()}) ({UserCount} attendee{(UserCount == 1 ? "" : "s")})";
         return Regex.Replace(result, @"\s+", " "); ;
      }

      public string ToStringExpire() {
         var timeLeft = (Expire - (DateTime.Now.AddSeconds(-15) - ExpireStart));
         return timeLeft.Days >= 1 ? timeLeft.ToString("d'd 'h'h 'm'm'") : timeLeft.ToString("h'h 'm'm'");
      }

      public string ToStringUsers() {
         StringBuilder raidWithUsers = new StringBuilder();

         if (Users.Count() == 0) {
            raidWithUsers.Append("No users are attending this raid");
         }
         else {
            raidWithUsers.Append($"Leader: {Users.First().Username} {Users.First().GetGuests} | ");

            foreach (var user in Users.Skip(1)) {
               raidWithUsers.Append($"{user.Username} {user.GetGuests} | ");
            }

            raidWithUsers.Length -= 2;
         }

         return raidWithUsers.ToString();
      }
      #endregion
   }
}
