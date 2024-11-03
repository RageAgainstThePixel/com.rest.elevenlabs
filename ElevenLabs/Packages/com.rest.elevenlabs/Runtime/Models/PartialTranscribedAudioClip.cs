using System;
using UnityEngine;

namespace ElevenLabs.Models {
	/// <summary>
	/// Represents timing information for a single character in the transcript
	/// </summary>
	[Serializable]
	public class PartialTranscribedAudioClip
	{
		public readonly AudioClip AudioClip;
		public readonly TimestampedTranscriptCharacter[] timestampedTranscriptCharacters;

		public PartialTranscribedAudioClip(AudioClip audioClip, TimestampedTranscriptCharacter[] timestampedTranscriptCharacters)
		{
			this.AudioClip = audioClip;
			this.timestampedTranscriptCharacters = timestampedTranscriptCharacters;
		}
	}
}