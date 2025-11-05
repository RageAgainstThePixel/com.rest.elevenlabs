// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    [Preserve]
    public sealed record PronunciationDictionary
    {
        [JsonConstructor]
        internal PronunciationDictionary(
            [JsonProperty("id")] string id,
            [JsonProperty("latest_version_id")] string latestVersionId,
            [JsonProperty("latest_version_rules_num")] int latestVersionRulesNum,
            [JsonProperty("name")] string name,
            [JsonProperty("permission_on_resource")] string permissionOnResource,
            [JsonProperty("created_by")] string createdBy,
            [JsonProperty("creation_time_unix")] long creationTimeUnix,
            [JsonProperty("archived_time_unix")] long? archivedTimeUnix,
            [JsonProperty("description")] string description)
        {
            Id = id;
            LatestVersionId = latestVersionId;
            LatestVersionRulesNum = latestVersionRulesNum;
            Name = name;
            PermissionOnResource = permissionOnResource;
            CreatedBy = createdBy;
            CreationTimeUnix = creationTimeUnix;
            ArchivedTimeUnix = archivedTimeUnix;
            Description = description;
        }

        [Preserve]
        [JsonProperty("id")]
        public string Id { get; }

        [Preserve]
        [JsonProperty("latest_version_id")]
        public string LatestVersionId { get; }

        [Preserve]
        [JsonProperty("latest_version_rules_num")]
        public int LatestVersionRulesNum { get; }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("permission_on_resource")]
        public string PermissionOnResource { get; }

        [Preserve]
        [JsonProperty("created_by")]
        public string CreatedBy { get; }

        [Preserve]
        [JsonProperty("creation_time_unix")]
        public long CreationTimeUnix { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime CreationTime
            => DateTimeOffset.FromUnixTimeSeconds(CreationTimeUnix).DateTime;

        [Preserve]
        [JsonProperty("archived_time_unix")]
        public long? ArchivedTimeUnix { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime? ArchivedTime
            => ArchivedTimeUnix.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(ArchivedTimeUnix.Value).DateTime
                : null;

        [Preserve]
        [JsonProperty("description")]
        public string Description { get; }
    }
}
