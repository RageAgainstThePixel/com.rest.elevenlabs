// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ElevenLabs.Voices
{
    public sealed record SharedVoiceQuery
    {
        public int? PageSize { get; set; } = null;

        public string Category { get; set; } = null;

        public string Gender { get; set; } = null;

        public string Age { get; set; } = null;

        public string Accent { get; set; } = null;

        public string Language { get; set; } = null;

        public string SearchTerms { get; set; } = null;

        public List<string> UseCases { get; set; } = null;

        public List<string> Descriptives { get; set; } = null;

        public bool? Featured { get; set; } = null;

        public bool? ReaderAppEnabled { get; set; } = null;

        public string OwnerId { get; set; } = null;

        public string Sort { get; set; } = null;

        public int? Page { get; set; } = null;

        public Dictionary<string, string> ToQueryParams()
        {
            var parameters = new Dictionary<string, string>();

            if (PageSize.HasValue)
            {
                parameters.Add("page_size", PageSize.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Category))
            {
                parameters.Add("category", Category);
            }

            if (!string.IsNullOrWhiteSpace(Gender))
            {
                parameters.Add("gender", Gender);
            }

            if (!string.IsNullOrWhiteSpace(Age))
            {
                parameters.Add("age", Age);
            }

            if (!string.IsNullOrWhiteSpace(Accent))
            {
                parameters.Add("accent", Accent);
            }

            if (!string.IsNullOrWhiteSpace(Language))
            {
                parameters.Add("language", Language);
            }

            if (!string.IsNullOrWhiteSpace(SearchTerms))
            {
                parameters.Add("search", SearchTerms);
            }

            if (UseCases is { Count: > 0 })
            {
                parameters.Add("use_cases", string.Join(',', UseCases));
            }

            if (Descriptives is { Count: > 0 })
            {
                parameters.Add("descriptives", string.Join(',', Descriptives));
            }

            if (Featured.HasValue)
            {
                parameters.Add("featured", Featured.Value.ToString().ToLower());
            }

            if (ReaderAppEnabled.HasValue)
            {
                parameters.Add("reader_app_enabled", ReaderAppEnabled.Value.ToString().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(OwnerId))
            {
                parameters.Add("owner_id", OwnerId);
            }

            if (!string.IsNullOrWhiteSpace(Sort))
            {
                parameters.Add("sort", Sort);
            }

            if (Page.HasValue)
            {
                parameters.Add("page", Page.Value.ToString());
            }

            return parameters;
        }
    }
}
