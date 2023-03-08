# com.rest.elevenlabs

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4)
[![openupm](https://img.shields.io/npm/v/com.rest.elevenlabs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rest.elevenlabs/)

A non-official [Eleven Labs](https://elevenlabs.io) voice synthesis RESTful client for the [Unity](https://unity.com/) Game Engine.

## Installing

### Via Unity Package Manager and OpenUPM

- Open your Unity project settings
- Select the `Package Manager`
![scoped-registries](images/package-manager-scopes.png)
- Add the OpenUPM package registry:
  - `Name: OpenUPM`
  - `URL: https://package.openupm.com`
  - `Scope(s):`
    - `com.rest.elevenlabs`
- Open the Unity Package Manager window
- Change the Registry from Unity to `My Registries`
- Add the `ElevenLabs` package

### Via Unity Package Manager and Git url

- Open your Unity Package Manager
- Add package from git url: `https://github.com/RageAgainstThePixel/com.rest.elevenlabs.git#upm`

---

## Documentation

### Table of Contents

- [Text to Speech](#text-to-speech)
- [Voices](#voices)
  - [Samples](#samples)
- [History](#history)
- [User](#user)

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

Access to voices created either by the user or eleven labs.

#### [Samples](https://api.elevenlabs.io/docs#/samples)

Access to your samples, created by you when cloning voices.

### [History](https://api.elevenlabs.io/docs#/history)

Access to your previously synthesized audio clips including its metadata.

### [User](https://api.elevenlabs.io/docs#/user)

Access to your user Information and subscription status.
