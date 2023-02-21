// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;

namespace ElevenLabs.User
{
    public sealed class NextInvoice
    {
        [JsonConstructor]
        public NextInvoice(
            [JsonProperty("amount_due_cents")] int amountDueCents,
            [JsonProperty("next_payment_attempt_unix")] int nextPaymentAttemptUnix)
        {
            AmountDueCents = amountDueCents;
            NextPaymentAttemptUnix = nextPaymentAttemptUnix;
        }

        [JsonProperty("amount_due_cents")]
        public int AmountDueCents { get; }

        [JsonProperty("next_payment_attempt_unix")]
        public int NextPaymentAttemptUnix { get; }

        [JsonIgnore]
        public DateTime NextPaymentAttempt => DateTimeOffset.FromUnixTimeSeconds(NextPaymentAttemptUnix).DateTime;
    }
}
