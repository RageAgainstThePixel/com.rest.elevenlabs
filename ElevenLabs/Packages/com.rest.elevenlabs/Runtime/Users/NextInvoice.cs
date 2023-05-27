// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace ElevenLabs.User
{
    [Preserve]
    public sealed class NextInvoice
    {
        [Preserve]
        [JsonConstructor]
        public NextInvoice(
            [JsonProperty("amount_due_cents")] double amountDueCents,
            [JsonProperty("next_payment_attempt_unix")] int nextPaymentAttemptUnix)
        {
            AmountDueCents = amountDueCents;
            NextPaymentAttemptUnix = nextPaymentAttemptUnix;
        }

        [Preserve]
        [JsonProperty("amount_due_cents")]
        public double AmountDueCents { get; }

        [Preserve]
        [JsonProperty("next_payment_attempt_unix")]
        public int NextPaymentAttemptUnix { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime NextPaymentAttempt => DateTimeOffset.FromUnixTimeSeconds(NextPaymentAttemptUnix).DateTime;
    }
}
