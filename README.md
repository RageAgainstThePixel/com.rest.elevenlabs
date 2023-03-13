# com.rest.elevenlabs

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4)
[![openupm](https://img.shields.io/npm/v/com.rest.elevenlabs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rest.elevenlabs/)

A non-official [ElevenLabs](https://elevenlabs.io) voice synthesis RESTful client for the [Unity](https://unity.com/) Game Engine.

I am not affiliated with ElevenLabs and an account with api access is required.

***All copyrights, trademarks, logos, and assets are the property of their respective owners.***

## Installing

### Via Unity Package Manager and OpenUPM

- Open your Unity project settings
- Select the `Package Manager`
![scoped-registries](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/package-manager-scopes.png)
- Add the OpenUPM package registry:
  - `Name: OpenUPM`
  - `URL: https://package.openupm.com`
  - `Scope(s):`
    - `com.rest.elevenlabs`
- Open the Unity Package Manager window
- Change the Registry from Unity to `My Registries`
- Add the `Elevenlabs` package

### Via Unity Package Manager and Git url

- Open your Unity Package Manager
- Add package from git url: `https://github.com/RageAgainstThePixel/com.rest.elevenlabs.git#upm`
  > Note: this repo has dependencies on other repositories! You are responsible for adding these on your own.
  - [com.utilities.async](https://github.com/RageAgainstThePixel/com.utilities.async)
  - [com.utilities.rest](https://github.com/RageAgainstThePixel/com.utilities.rest)
  - [com.utilities.audio](https://github.com/RageAgainstThePixel/com.utilities.audio)

---

## Documentation

### Table of Contents

- [Editor Dashboard](#editor-dashboard)
  - [Speech Synthesis](#speech-synthesis)
  - [Voice Lab](#voice-lab)
    - [Voice Designer](#voice-designer)
    - [Voice Cloning](#voice-cloning)
  - [History](#history)
- [Text to Speech](#text-to-speech)
- [Voices](#voices)
  - [Get All Voices](#get-all-voices)
  - [Get Default Voice Settings](#get-default-voice-settings)
  - [Get Voice](#get-voice)
  - [Edit Voice Settings](#edit-voice-settings)
  - [Add Voice](#add-voice)
  - [Edit Voice](#edit-voice)
  - [Delete Voice](#delete-voice)
  - [Samples](#samples)
    - [Get Voice Sample](#get-voice-sample)
    - [Delete Voice Sample](#delete-voice-sample)
- [History](#history)
  - [Get History](#get-history)
  - [Get History Audio](#get-history-audio)
  - [Download All History](#download-all-history)
  - [Delete History Item](#delete-history-item)
- [User](#user)
  - [Get User Info](#get-user-info)
  - [Get Subscription Info](#get-subscription-info)

### Editor Dashboard

You can perform all of the same actions from the ElevenLabs website, in the Editor using the ElevenLabs Dashboard!
![dashboard](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/dashboard.png)

#### Speech Synthesis

Just like in the ElevenLabs website, you can synthesize new audio clips using available voices. This tool makes it even more handy as the clips are automatically downloaded and imported into your project, ready for you to use!

![Speech Synthesis](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-synthesis-editor.png)

#### Voice Lab

Just like in the ElevenLabs website, you can manage all your voices directly in the editor.

![Voice Lab](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-lab-editor.png)

##### Voice Designer

Selecting `Create New Voice` will display a popup where you can design entirely new voices using ElevenLabs generative models.

![Voice Designer](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-design-editor.png)

##### Voice Cloning

Additionally, you're also able to clone a voice from sample recordings.

![Voice Cloning](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-clone-editor.png)

#### History

You also have access to the full list of all your generated samples, ready for download.

![History](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/history-editor.png)

### [Text to Speech](https://api.elevenlabs.io/docs#/text-to-speech)

Convert text to speech.

```csharp
var api = new ElevenLabsClient();
var text = "The quick brown fox jumps over the lazy dog.";
var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, voice, defaultVoiceSettings);
Debug.Log(clipPath);
```

### [Voices](https://api.elevenlabs.io/docs#/voices)

Access to voices created either by the user or ElevenLabs.

#### Get All Voices

Gets a list of all available voices.

```csharp
var api = new ElevenLabsClient();
var allVoices = await api.VoicesEndpoint.GetAllVoicesAsync();

foreach (var voice in allVoices)
{
    Debug.Log($"{voice.Id} | {voice.Name} | similarity boost: {voice.Settings?.SimilarityBoost} | stability: {voice.Settings?.Stability}");
}
```

#### Get Default Voice Settings

Gets the global default voice settings.

```csharp
var api = new ElevenLabsClient();
var result = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
Debug.Log($"stability: {result.Stability} | similarity boost: {result.SimilarityBoost}");
```

#### Get Voice

```csharp
var api = new ElevenLabsClient();
var voice = await api.VoicesEndpoint.GetVoiceAsync("voiceId");
Debug.Log($"{voice.Id} | {voice.Name} | {voice.PreviewUrl}");
```

#### Edit Voice Settings

Edit the settings for a specific voice.

```csharp
var api = new ElevenLabsClient();
var success = await api.VoicesEndpoint.EditVoiceSettingsAsync(voice, new VoiceSettings(0.7f, 0.7f));
Debug.Log($"Was successful? {success}");
```

#### Add Voice

```csharp
var api = new ElevenLabsClient();
var labels = new Dictionary<string, string>
{
    { "accent", "american" }
};
var audioSamplePaths = new List<string>();
var voice = await api.VoicesEndpoint.AddVoiceAsync("Voice Name", audioSamplePaths, labels);
```

#### Edit Voice

```csharp
var api = new ElevenLabsClient();
var labels = new Dictionary<string, string>
{
    { "age", "young" }
};
var audioSamplePaths = new List<string>();
var success = await api.VoicesEndpoint.EditVoiceAsync(voice, audioSamplePaths, labels);
Debug.Log($"Was successful? {success}");
```

#### Delete Voice

```csharp
var api = new ElevenLabsClient();
var success = await api.VoicesEndpoint.DeleteVoiceAsync(voiceId);
Debug.Log($"Was successful? {success}");
```

#### [Samples](https://api.elevenlabs.io/docs#/samples)

Access to your samples, created by you when cloning voices.

##### Get Voice Sample

```csharp
var api = new ElevenLabsClient();
var audioClip = await api.VoicesEndpoint.GetVoiceSampleAsync(voiceId, sampleId);
```

##### Delete Voice Sample

```csharp
var api = new ElevenLabsClient();
var success = await api.VoicesEndpoint.DeleteVoiceSampleAsync(voiceId, sampleId);
Debug.Log($"Was successful? {success}");
```

### [History](https://api.elevenlabs.io/docs#/history)

Access to your previously synthesized audio clips including its metadata.

#### Get History

```csharp
var api = new ElevenLabsClient();
var historyItems = await api.HistoryEndpoint.GetHistoryAsync();

foreach (var historyItem in historyItems.OrderBy(historyItem => historyItem.Date))
{
    Debug.Log($"{historyItem.State} {historyItem.Date} | {historyItem.Id} | {historyItem.Text.Length} | {historyItem.Text}");
}
```

#### Get History Audio

```csharp
var api = new ElevenLabsClient();
var audioClip = await api.HistoryEndpoint.GetHistoryAudioAsync(historyItem);
```

#### Download All History

```csharp
var api = new ElevenLabsClient();
var success = await api.HistoryEndpoint.DownloadHistoryItemsAsync();
```

#### Delete History Item

```csharp
var api = new ElevenLabsClient();
var result = await api.HistoryEndpoint.DeleteHistoryItemAsync(historyItem);
Debug.Log($"Was successful? {success}");
```

### [User](https://api.elevenlabs.io/docs#/user)

Access to your user Information and subscription status.

#### Get User Info

Gets information about your user account with ElevenLabs.

```csharp
var api = new ElevenLabsClient();
var userInfo = await api.UserEndpoint.GetUserInfoAsync();
```

#### Get Subscription Info

Gets information about your subscription with ElevenLabs.

```csharp
var api = new ElevenLabsClient();
var subscriptionInfo = await api.UserEndpoint.GetSubscriptionInfoAsync();
```
