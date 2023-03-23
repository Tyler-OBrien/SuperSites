﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.Configuration.Storage
{
    public interface IStorageConfiguration
    {
        // There's probably a better way to do this, and I am failing to find it.
        [JsonInclude]
        [JsonPropertyName("Type")]
        public string InstanceType { get; }

        // Storage's Unique ID that identifies the storage it is using. Combined with the Instance Type, we identify the backend storage  uniquely, even if there are mutiple configurations for the same storage
        [JsonIgnore]
        public string StorageID { get; }

        public static string Type { get; }
        public List<string> AllowedFileExtensions { get; set; }
        public long FileSizeLimit { get; set; }
    }
}