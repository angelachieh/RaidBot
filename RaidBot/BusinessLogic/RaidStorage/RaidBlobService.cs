﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RaidBot.BusinessLogic.BlobStorage;
using RaidBot.BusinessLogic.RaidStorage.Interfaces;
using RaidBot.Entities;

namespace RaidBot.BusinessLogic.RaidStorage {
   public class RaidBlobService : BlobService, IRaidFileService {
      ServerSettings _serverSettings;

      public RaidBlobService(string guildId, ServerSettings serverSettings) : base(guildId) {
         _serverSettings = serverSettings;
      }
      
      public List<Raid> GetRaidsFromFile() {
         var json = _blockBlob.DownloadText();
         List<Raid> raids = JsonConvert.DeserializeObject<List<Raid>>(json).OrderBy(a => a.Time).ToList();

         var aliveRaids = DeleteExpiredRaids(raids);

         aliveRaids = ChangeOrder(aliveRaids);

         return aliveRaids;
      }

      private List<Raid> ChangeOrder(List<Raid> aliveRaids) {
         return aliveRaids
            .OrderByDescending(a => !a.Day.HasValue)
            .ThenBy(a => a.Day)
            .ThenByDescending(a => !a.Time.HasValue)
            .ThenBy(a => a.Time)
            .ToList();
      }

      private List<Raid> DeleteExpiredRaids(List<Raid> raids) {
         var aliveRaids = raids.Where(a => (DateTime.Now - a.ExpireStart).TotalMinutes < a.Expire.TotalMinutes);
         if (_serverSettings.JoinRaidOnCreate) {
            aliveRaids = aliveRaids.Where(a => a.Users.Count() > 0);
         }
         if (aliveRaids.Count() < raids.Count()) {
            PushRaidsToFile(aliveRaids.ToList());
         }
         return aliveRaids.ToList();
      }

      public void PushRaidsToFile(List<Raid> raids) {
         string json = JsonConvert.SerializeObject(raids);
         _blockBlob.UploadText(json);
      }
   }
}
