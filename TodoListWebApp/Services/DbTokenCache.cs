using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TodoListWebApp.Models;
using TodoListWebApp.Utils;

namespace TodoListWebApp.Services
{
    public class PerWebUserCache
    {
        [Key]
        public int EntryId { get; set; }
        public string webUserUniqueId { get; set; }
        public byte[] cacheBits { get; set; }
        public DateTime LastWrite { get; set; }
    }

    // An EF based implementation of the ADAL token cache and token caching service
    public class DbTokenCache : TokenCache, ITokenCache
    {
        private TodoListWebAppContext _db;
        string User;
        PerWebUserCache Cache;

        // Constructor which accepts the injected db context
        public DbTokenCache(TodoListWebAppContext context)
        {
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            this.BeforeWrite = BeforeWriteNotification;

            _db = context; 
        }

        public TokenCache Init(string user)
        {
            // associate the cache to the current user of the web app
            User = user;

            // look up the entry in the db
            Cache = _db.PerUserCacheList.FirstOrDefault(c => c.webUserUniqueId == User);
            // place the entry in memory
            this.Deserialize((Cache == null) ? null : Cache.cacheBits);

            return this;
        }

        // clean up the db
        public override void Clear()
        {
            base.Clear();
            foreach (var cacheEntry in _db.PerUserCacheList)
                _db.PerUserCacheList.Remove(cacheEntry);
            _db.SaveChanges();
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the db, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (Cache == null)
            {
                // first time access
                Cache = _db.PerUserCacheList.FirstOrDefault(c => c.webUserUniqueId == User);
            }
            else
            {   // retrieve last write from the db
                var status = from e in _db.PerUserCacheList
                             where (e.webUserUniqueId == User)
                             select new
                             {
                                 LastWrite = e.LastWrite
                             };
                // if the in-memory copy is older than the persistent copy
                if (status.First().LastWrite > Cache.LastWrite)
                //// read from from storage, update in-memory copy
                {
                    Cache = _db.PerUserCacheList.FirstOrDefault(c => c.webUserUniqueId == User);
                }
            }
            this.Deserialize((Cache == null) ? null : Cache.cacheBits);
        }
        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (this.HasStateChanged)
            {
                Cache = new PerWebUserCache
                {
                    webUserUniqueId = User,
                    cacheBits = this.Serialize(),
                    LastWrite = DateTime.Now
                };
                //// update the db and the lastwrite                
                _db.Entry(Cache).State = Cache.EntryId == 0 ? EntityState.Added : EntityState.Modified;
                _db.SaveChanges();
                this.HasStateChanged = false;
            }
        }
        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }
}
