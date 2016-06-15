﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TodoListWebApp.Models
{
    // Entity class for todo entries
    public class Todo
    {
        public int ID { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
    }

    // Entity for keeping track of organizations onboarded as customers of the app
    public class Tenant
    {
        public int ID { get; set; }
        public string IssValue { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public bool AdminConsented { get; set; }
    }

    // Entity for keeping track of individual users onboarded as customers of the app
    public class AADUserRecord
    {
        [Key]
        public string ObjectID { get; set; }
        public string UPN { get; set; }
    }

    // Entity for saving tokens for accessing API
    public class TokenCacheEntry
    {
        public int ID { get; set; }
        public string SignedInUser { get; set; }
        public string TokenRequestorUser { get; set; }
        public string ResourceID { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }

    // Entity for displaying user information in UI, not stored in db
    public class AADUserProfile
    {
        [JsonProperty(PropertyName = "odata.metadata")]
        public string odata_metadata { get; set; }
        [JsonProperty(PropertyName = "odata.type")]
        public string odata_type { get; set; }
        public string objectType { get; set; }
        public string objectId { get; set; }
        public bool accountEnabled { get; set; }
        public string city { get; set; }
        public string companyName { get; set; }
        public string country { get; set; }
        public string department { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string immutableId { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mailNickname { get; set; }
        public string postalCode { get; set; }
        public string preferredLanguage { get; set; }
        public string state { get; set; }
        public string streetAddress { get; set; }
        public string surname { get; set; }
        public string telephoneNumber { get; set; }
        public string usageLocation { get; set; }
        public string userPrincipalName { get; set; }
    }
}
