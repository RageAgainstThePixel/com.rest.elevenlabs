// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using Utilities.WebRequestRest;

namespace ElevenLabs.Voices
{
    public sealed class VoicesV2Endpoint : ElevenLabsBaseEndPoint
    {
        public VoicesV2Endpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "voices";

        protected override string ApiVersion => "v2";

        public async Task<VoiceList> GetVoicesAsync(VoiceQuery query = null, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl(queryParameters: query?.ToQueryParams()), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<VoiceList>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }
    }

    /// <summary>
    /// Represents a container for a collection of voices with pagination metadata.
    /// </summary>
    [Preserve]
    public sealed class VoiceList
    {
        [Preserve]
        [JsonConstructor]
        internal VoiceList(
            [JsonProperty("voices")] List<Voice> voices,
            [JsonProperty("has_more")] bool hasMore,
            [JsonProperty("total_count")] int totalCount,
            [JsonProperty("next_page_token")] string nextPageToken)
        {
            Voices = voices;
            HasMore = hasMore;
            TotalCount = totalCount;
            NextPageToken = nextPageToken;
        }

        [Preserve]
        [JsonProperty("voices")]
        public IReadOnlyList<Voice> Voices { get; }

        [Preserve]
        [JsonProperty("has_more")]
        public bool HasMore { get; }

        [Preserve]
        [JsonProperty("total_count")]
        public int TotalCount { get; }

        [Preserve]
        [JsonProperty("next_page_token")]
        public string NextPageToken { get; }
    }

    public enum SortDirections
    {
        /// <summary>
        /// Sort by the creation time (Unix timestamp). May not be available for older voices.
        /// </summary>
        [EnumMember(Value = "created_at_unix")]
        CreatedAtUnix,
        /// <summary>
        /// Sort by the name of the voice.
        /// </summary>
        [EnumMember(Value = "name")]
        Name
    }

    /// <summary>
    /// Type of the voice to filter by.
    /// </summary>
    public enum VoiceTypes
    {
        /// <summary>
        /// Personal voice.
        /// </summary>
        [EnumMember(Value = "personal")]
        Personal,
        /// <summary>
        /// Community voice.
        /// </summary>
        [EnumMember(Value = "community")]
        Community,
        /// <summary>
        /// Default voice.
        /// </summary>
        [EnumMember(Value = "default")]
        Default,
        /// <summary>
        /// Workspace voice.
        /// </summary>
        [EnumMember(Value = "workspace")]
        Workspace,
        /// <summary>
        /// Non-default voice (all but 'default').
        /// </summary>
        [EnumMember(Value = "non-default")]
        NonDefault
    }
    /// <summary>
    /// Category of the voice to filter by.
    /// </summary>
    public enum CategoryTypes
    {
        /// <summary>
        /// Premade voice.
        /// </summary>
        [EnumMember(Value = "premade")]
        Premade,
        /// <summary>
        /// Cloned voice.
        /// </summary>
        [EnumMember(Value = "cloned")]
        Cloned,
        /// <summary>
        /// Generated voice.
        /// </summary>
        [EnumMember(Value = "generated")]
        Generated,
        /// <summary>
        /// Professional voice.
        /// </summary>
        [EnumMember(Value = "professional")]
        Professional
    }
    /// <summary>
    /// State of the voice’s fine-tuning to filter by. Applicable only to professional voices clones.
    /// </summary>
    public enum FineTuningStateTypes
    {
        /// <summary>
        /// Draft state.
        /// </summary>
        [EnumMember(Value = "draft")]
        Draft,
        /// <summary>
        /// Not verified state.
        /// </summary>
        [EnumMember(Value = "not_verified")]
        NotVerified,
        /// <summary>
        /// Not started state.
        /// </summary>
        [EnumMember(Value = "not_started")]
        NotStarted,
        /// <summary>
        /// Queued state.
        /// </summary>
        [EnumMember(Value = "queued")]
        Queued,
        /// <summary>
        /// Fine-tuning in progress.
        /// </summary>
        [EnumMember(Value = "fine_tuning")]
        FineTuning,
        /// <summary>
        /// Fine-tuned state.
        /// </summary>
        [EnumMember(Value = "fine_tuned")]
        FineTuned,
        /// <summary>
        /// Failed state.
        /// </summary>
        [EnumMember(Value = "failed")]
        Failed,
        /// <summary>
        /// Delayed state.
        /// </summary>
        [EnumMember(Value = "delayed")]
        Delayed
    }

    /// <summary>
    /// Represents a container for query parameters used by the VoicesV2Endpoint.
    /// </summary>
    public sealed record VoiceQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceQuery"/> class with optional parameters.
        /// </summary>
        public VoiceQuery(
            string nextPageToken = null,
            int? pageSize = null,
            string search = null,
            string sort = null,
            SortDirections? sortDirection = null,
            VoiceTypes? voiceType = null,
            CategoryTypes? category = null,
            FineTuningStateTypes? fineTuningState = null,
            string collectionId = null,
            bool? includeTotalCount = null,
            IEnumerable<string> voiceIds = null)
        {
            NextPageToken = nextPageToken;
            PageSize = pageSize;
            Search = search;
            Sort = sort;
            SortDirection = sortDirection;
            VoiceType = voiceType;
            Category = category;
            FineTuningState = fineTuningState;
            CollectionId = collectionId;
            IncludeTotalCount = includeTotalCount;
            VoiceIds = voiceIds?.ToList();
        }

        /// <summary>
        /// Optional. The next page token to use for pagination. Returned from the previous request.
        /// </summary>
        public string NextPageToken { get; private set; }

        /// <summary>
        /// Optional. How many voices to return at maximum. Can not exceed 100, defaults to 10. Page 0 may include more voices due to default voices being included.
        /// </summary>
        public int? PageSize { get; }

        /// <summary>
        /// Optional. Search term to filter voices by. Searches in name, description, labels, category.
        /// </summary>
        public string Search { get; }

        /// <summary>
        /// Optional. Which field to sort by, one of ‘created_at_unix’ or ‘name’. ‘created_at_unix’ may not be available for older voices.
        /// </summary>
        public string Sort { get; }

        /// <summary>
        /// Optional. Which direction to sort the voices in. 'asc' or 'desc'.
        /// </summary>
        public SortDirections? SortDirection { get; }

        /// <summary>
        /// Optional. Type of the voice to filter by. One of ‘personal’, ‘community’, ‘default’, ‘workspace’, ‘non-default’. ‘non-default’ is equal to all but ‘default’.
        /// </summary>
        public VoiceTypes? VoiceType { get; }

        /// <summary>
        /// Optional. Category of the voice to filter by. One of 'premade', 'cloned', 'generated', 'professional'.
        /// </summary>
        public CategoryTypes? Category { get; }

        /// <summary>
        /// Optional. State of the voice’s fine-tuning to filter by. Applicable only to professional voices clones. One of ‘draft’, ‘not_verified’, ‘not_started’, ‘queued’, ‘fine_tuning’, ‘fine_tuned’, ‘failed’, ‘delayed’.
        /// </summary>
        public FineTuningStateTypes? FineTuningState { get; }

        /// <summary>
        /// Optional. Collection ID to filter voices by.
        /// </summary>
        public string CollectionId { get; }

        /// <summary>
        /// Optional. Whether to include the total count of voices found in the response. Incurs a performance cost. Defaults to true.
        /// </summary>
        public bool? IncludeTotalCount { get; }

        /// <summary>
        /// Optional. Voice IDs to lookup by. Maximum 100 voice IDs.
        /// </summary>
        public IReadOnlyList<string> VoiceIds { get; }

        public VoiceQuery WithNextPageToken(string nextPageToken) => this with { NextPageToken = nextPageToken };

        public static implicit operator Dictionary<string, string>(VoiceQuery query) => query?.ToQueryParams();

        /// <summary>
        /// Converts the current query object to a dictionary of HTTP query parameters.
        /// </summary>
        public Dictionary<string, string> ToQueryParams()
        {
            var parameters = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(NextPageToken))
            {
                parameters.Add("next_page_token", NextPageToken);
            }

            if (PageSize.HasValue)
            {
                parameters.Add("page_size", PageSize.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Search))
            {
                parameters.Add("search", Search);
            }

            if (!string.IsNullOrWhiteSpace(Sort))
            {
                parameters.Add("sort", Sort);
            }

            if (SortDirection.HasValue)
            {
                parameters.Add("sort_direction", SortDirection.Value.ToString());
            }

            if (VoiceType.HasValue)
            {
                parameters.Add("voice_type", VoiceType.Value.ToString());
            }

            if (Category.HasValue)
            {
                parameters.Add("category", Category.Value.ToString());
            }

            if (FineTuningState.HasValue)
            {
                parameters.Add("fine_tuning_state", FineTuningState.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(CollectionId))
            {
                parameters.Add("collection_id", CollectionId);
            }

            if (IncludeTotalCount.HasValue)
            {
                parameters.Add("include_total_count", IncludeTotalCount.Value.ToString().ToLower());
            }

            if (VoiceIds is { Count: > 0 })
            {
                parameters.Add("voice_ids", string.Join(",", VoiceIds));
            }

            return parameters;
        }
    }
}
