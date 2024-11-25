# com.rest.elevenlabs

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4) [![openupm](https://img.shields.io/npm/v/com.rest.elevenlabs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rest.elevenlabs/) [![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.rest.elevenlabs)](https://openupm.com/packages/com.rest.elevenlabs/)

A non-official [ElevenLabs](https://elevenlabs.io/?from=partnerbrown9849) voice synthesis RESTful client for the [Unity](https://unity.com/) Game Engine.

Based on [ElevenLabs-DotNet](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet)

I am not affiliated with ElevenLabs and an account with api access is required.

***All copyrights, trademarks, logos, and assets are the property of their respective owners.***

## Installing

Requires Unity 2021.3 LTS or higher.

The recommended installation method is though the unity package manager and [OpenUPM](https://openupm.com/packages/com.rest.elevenlabs).

### Via Unity Package Manager and OpenUPM

- Open your Unity project settings
- Select the `Package Manager`
![scoped-registries](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/package-manager-scopes.png)
- Add the OpenUPM package registry:
  - Name: `OpenUPM`
  - URL: `https://package.openupm.com`
  - Scope(s):
    - `com.rest.elevenlabs`
    - `com.utilities`
- Open the Unity Package Manager window
- Change the Registry from Unity to `My Registries`
- Add the `ElevenLabs` package

### Via Unity Package Manager and Git url

- Open your Unity Package Manager
- Add package from git url: `https://github.com/RageAgainstThePixel/com.rest.elevenlabs.git#upm`
  > Note: this repo has dependencies on other repositories! You are responsible for adding these on your own.
  - [com.utilities.async](https://github.com/RageAgainstThePixel/com.utilities.async)
  - [com.utilities.extensions](https://github.com/RageAgainstThePixel/com.utilities.extensions)
  - [com.utilities.audio](https://github.com/RageAgainstThePixel/com.utilities.audio)
  - [com.utilities.encoder.ogg](https://github.com/RageAgainstThePixel/com.utilities.encoder.ogg)
  - [com.utilities.encoder.wav](https://github.com/RageAgainstThePixel/com.utilities.encoder.wav)
  - [com.utilities.rest](https://github.com/RageAgainstThePixel/com.utilities.rest)

---

## Documentation

### Table of Contents

- [Authentication](#authentication)
- [API Proxy](#api-proxy)
- [Editor Dashboard](#editor-dashboard)
  - [Speech Synthesis Dashboard](#speech-synthesis-dashboard)
  - [Voice Lab Dashboard](#voice-lab-dashboard)
    - [Voice Designer Dashboard](#voice-designer-dashboard)
    - [Voice Cloning Dashboard](#voice-cloning-dashboard)
  - [History](#history)
- [Text to Speech](#text-to-speech)
  - [Stream Text To Speech](#stream-text-to-speech)
- [Voices](#voices)
  - [Get Shared Voices](#get-shared-voices) :new:
  - [Get All Voices](#get-all-voices)
  - [Get Default Voice Settings](#get-default-voice-settings)
  - [Get Voice](#get-voice)
  - [Edit Voice Settings](#edit-voice-settings)
  - [Add Voice](#add-voice)
  - [Edit Voice](#edit-voice)
  - [Delete Voice](#delete-voice)
  - [Samples](#samples)
    - [Download Voice Sample](#download-voice-sample)
    - [Delete Voice Sample](#delete-voice-sample)
- [Dubbing](#dubbing) :new:
  - [Dub](#dub) :new:
  - [Get Dubbing Metadata](#get-dubbing-metadata) :new:
  - [Get Transcript for Dub](#get-transcript-for-dub) :new:
  - [Get dubbed file](#get-dubbed-file) :new:
  - [Delete Dubbing Project](#delete-dubbing-project) :new:
- [SFX Generation](#sfx-generation) :new:
- [History](#history)
  - [Get History](#get-history)
  - [Get History Item](#get-history-item)
  - [Download History Audio](#download-history-audio)
  - [Download History Items](#download-history-items)
  - [Delete History Item](#delete-history-item)
- [User](#user)
  - [Get User Info](#get-user-info)
  - [Get Subscription Info](#get-subscription-info)

### Authentication

There are 4 ways to provide your API keys, in order of precedence:

:warning: We recommended using the environment variables to load the API key instead of having it hard coded in your source. It is not recommended use this method in production, but only for accepting user credentials, local testing and quick start scenarios.

1. [Pass keys directly with constructor](#pass-keys-directly-with-constructor) :warning:
2. [Unity Scriptable Object](#unity-scriptable-object) :warning:
3. [Load key from configuration file](#load-key-from-configuration-file)
4. [Use System Environment Variables](#use-system-environment-variables)

#### Pass keys directly with constructor

```csharp
var api = new ElevenLabsClient("yourApiKey");
```

Or create a `ElevenLabsAuthentication` object manually

```csharp
var api = new ElevenLabsClient(new ElevenLabsAuthentication("yourApiKey"));
```

#### Unity Scriptable Object

You can save the key directly into a scriptable object that is located in the `Assets/Resources` folder.

You can create a new one by using the context menu of the project pane and creating a new `ElevenLabsConfiguration` scriptable object.

![Create new ElevenLabsConfiguration](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/create-scriptable-object.png)

#### Load key from configuration file

Attempts to load api keys from a configuration file, by default `.elevenlabs` in the current directory, optionally traversing up the directory tree or in the user's home directory.

To create a configuration file, create a new text file named `.elevenlabs` and containing the line:

##### Json format

```json
{
  "apiKey": "yourApiKey",
}
```

You can also load the file directly with known path by calling a static method in Authentication:

```csharp
var api = new ElevenLabsClient(new ElevenLabsAuthentication().LoadFromDirectory("your/path/to/.elevenlabs"));;
```

#### Use System Environment Variables

Use your system's environment variables specify an api key to use.

- Use `ELEVEN_LABS_API_KEY` for your api key.

```csharp
var api = new ElevenLabsClient(new ElevenLabsAuthentication().LoadFromEnvironment());
```

### [API Proxy](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet/main/ElevenLabs-DotNet-Proxy/README.md)

[![NuGet version (ElevenLabs-DotNet-Proxy)](https://img.shields.io/nuget/v/ElevenLabs-DotNet-Proxy.svg?label=ElevenLabs-DotNet-Proxy&logo=nuget)](https://www.nuget.org/packages/ElevenLabs-DotNet-Proxy/)

Using either the [ElevenLabs-DotNet](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet) or [com.rest.elevenlabs](https://github.com/RageAgainstThePixel/com.rest.elevenlabs) packages directly in your front-end app may expose your API keys and other sensitive information. To mitigate this risk, it is recommended to set up an intermediate API that makes requests to ElevenLabs on behalf of your front-end app. This library can be utilized for both front-end and intermediary host configurations, ensuring secure communication with the ElevenLabs API.

#### Front End Example

In the front end example, you will need to securely authenticate your users using your preferred OAuth provider. Once the user is authenticated, exchange your custom auth token with your API key on the backend.

Follow these steps:

1. Setup a new project using either the [ElevenLabs-DotNet](https://github.com/RageAgainstThePixel/ElevenLabs-DotNet) or [com.rest.elevenlabs](https://github.com/RageAgainstThePixel/com.rest.elevenlabs) packages.
2. Authenticate users with your OAuth provider.
3. After successful authentication, create a new `ElevenLabsAuthentication` object and pass in the custom token.
4. Create a new `ElevenLabsSettings` object and specify the domain where your intermediate API is located.
5. Pass your new `auth` and `settings` objects to the `ElevenLabsClient` constructor when you create the client instance.

Here's an example of how to set up the front end:

```csharp
var authToken = await LoginAsync();
var auth = new ElevenLabsAuthentication(authToken);
var settings = new ElevenLabsSettings(domain: "api.your-custom-domain.com");
var api = new ElevenLabsClient(auth, settings);
```

This setup allows your front end application to securely communicate with your backend that will be using the ElevenLabs-DotNet-Proxy, which then forwards requests to the ElevenLabs API. This ensures that your ElevenLabs API keys and other sensitive information remain secure throughout the process.

#### Back End Example

In this example, we demonstrate how to set up and use `ElevenLabsProxyStartup` in a new ASP.NET Core web app. The proxy server will handle authentication and forward requests to the ElevenLabs API, ensuring that your API keys and other sensitive information remain secure.

1. Create a new [ASP.NET Core minimal web API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-6.0) project.
2. Add the ElevenLabs-DotNet nuget package to your project.
    - Powershell install: `Install-Package ElevenLabs-DotNet-Proxy`
    - Dotnet install: `dotnet add package ElevenLabs-DotNet-Proxy`
    - Manually editing .csproj: `<PackageReference Include="ElevenLabs-DotNet-Proxy" />`
3. Create a new class that inherits from `AbstractAuthenticationFilter` and override the `ValidateAuthenticationAsync` method. This will implement the `IAuthenticationFilter` that you will use to check user session token against your internal server.
4. In `Program.cs`, create a new proxy web application by calling `ElevenLabsProxyStartup.CreateDefaultHost` method, passing your custom `AuthenticationFilter` as a type argument.
5. Create `ElevenLabsAuthentication` and `ElevenLabsClientSettings` as you would normally with your API keys, org id, or Azure settings.

```csharp
public partial class Program
{
    private class AuthenticationFilter : AbstractAuthenticationFilter
    {
        public override async Task ValidateAuthenticationAsync(IHeaderDictionary request)
        {
            await Task.CompletedTask; // remote resource call

            // You will need to implement your own class to properly test
            // custom issued tokens you've setup for your end users.
            if (!request["xi-api-key"].ToString().Contains(TestUserToken))
            {
                throw new AuthenticationException("User is not authorized");
            }
        }
    }

    public static void Main(string[] args)
    {
        var client = new ElevenLabsClient();
        var proxy = ElevenLabsProxyStartup.CreateDefaultHost<AuthenticationFilter>(args, client);
        proxy.Run();
    }
}
```

Once you have set up your proxy server, your end users can now make authenticated requests to your proxy api instead of directly to the ElevenLabs API. The proxy server will handle authentication and forward requests to the ElevenLabs API, ensuring that your API keys and other sensitive information remain secure.

### Editor Dashboard

You can perform all of the same actions from the ElevenLabs website, in the Editor using the ElevenLabs Dashboard!

`Window/Dashboards/ElevenLabs`

![dashboard](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/dashboard.png)

#### Speech Synthesis Dashboard

Just like in the ElevenLabs website, you can synthesize new audio clips using available voices. This tool makes it even more handy as the clips are automatically downloaded and imported into your project, ready for you to use!

![Speech Synthesis](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-synthesis-editor.png)

#### Voice Lab Dashboard

Just like in the ElevenLabs website, you can manage all your voices directly in the editor.

![Voice Lab](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-lab-editor.png)

##### Voice Designer Dashboard

Selecting `Create New Voice` will display a popup where you can design entirely new voices using ElevenLabs generative models.

![Voice Designer](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-design-editor.png)

##### Voice Cloning Dashboard

Additionally, you're also able to clone a voice from sample recordings.

![Voice Cloning](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/voice-clone-editor.png)

#### History Editor Dashboard

You also have access to the full list of all your generated samples, ready for download.

![History](ElevenLabs/Packages/com.rest.elevenlabs/Documentation~/images/history-editor.png)

### [Text to Speech](https://docs.elevenlabs.io/api-reference/text-to-speech)

Convert text to speech.

```csharp
var api = new ElevenLabsClient();
var text = "The quick brown fox jumps over the lazy dog.";
var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
var request = new TextToSpeechRequest(voice, text);
var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(request);
audioSource.PlayOneShot(voiceClip.AudioClip);
```

> Note: if you want to save the voice clip into your project, you will need to copy it from the cached path into the specified location in your project:

```csharp
voiceClip.CopyIntoProject(editorDownloadDirectory);
```

#### [Stream Text To Speech](https://docs.elevenlabs.io/api-reference/text-to-speech-stream)

Stream text to speech.

```csharp
var api = new ElevenLabsClient();
var text = "The quick brown fox jumps over the lazy dog.";
var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
var partialClips = new Queue<VoiceClip>();
var request = new TextToSpeechRequest(voice, message, model: Model.EnglishTurboV2, outputFormat: OutputFormat.PCM_44100);
var voiceClip = await api.TextToSpeechEndpoint.StreamTextToSpeechAsync(request, partialClip =>
{
    // Note: check demo scene for best practices
    // on how to handle playback with OnAudioFilterRead
    partialClips.Enqueue(partialClip);
});
```

### [Voices](https://docs.elevenlabs.io/api-reference/voices)

Access to voices created either by the user or ElevenLabs.

#### Get Shared Voices

Gets a list of shared voices in the public voice library.

```csharp
var api = new ElevenLabsClient();
var results = await ElevenLabsClient.SharedVoicesEndpoint.GetSharedVoicesAsync();
foreach (var voice in results.Voices)
{
    Debug.Log($"{voice.OwnerId} | {voice.VoiceId} | {voice.Date} | {voice.Name}");
}
```

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

#### [Samples](https://docs.elevenlabs.io/api-reference/samples)

Access to your samples, created by you when cloning voices.

##### Download Voice Sample

```csharp
var api = new ElevenLabsClient();
var voiceClip = await api.VoicesEndpoint.DownloadVoiceSampleAsync(voice, sample);
```

##### Delete Voice Sample

```csharp
var api = new ElevenLabsClient();
var success = await api.VoicesEndpoint.DeleteVoiceSampleAsync(voiceId, sampleId);
Debug.Log($"Was successful? {success}");
```

### [Dubbing](https://elevenlabs.io/docs/api-reference/create-dub)

#### Dub

Dubs provided audio or video file into given language.

```csharp
var api = new ElevenLabsClient();
// from URI
var request = new DubbingRequest(new Uri("https://youtu.be/Zo5-rhYOlNk"), "ja", "en", 1, true);
// from file
var request = new DubbingRequest(filePath, "es", "en", 1);
var metadata = await api.DubbingEndpoint.DubAsync(request, progress: new Progress<DubbingProjectMetadata>(metadata =>
{
    switch (metadata.Status)
    {
        case "dubbing":
            Debug.Log($"Dubbing for {metadata.DubbingId} in progress... Expected Duration: {metadata.ExpectedDurationSeconds:0.00} seconds");
            break;
        case "dubbed":
            Debug.Log($"Dubbing for {metadata.DubbingId} complete in {metadata.TimeCompleted.TotalSeconds:0.00} seconds!");
            break;
        default:
            Debug.Log($"Status: {metadata.Status}");
            break;
    }
}));
```

#### Get Dubbing Metadata

Returns metadata about a dubbing project, including whether it’s still in progress or not.

```csharp
var api = new ElevenLabsClient();
var metadata = api.await GetDubbingProjectMetadataAsync("dubbing-id");
```

#### Get Dubbed File

Returns downloaded dubbed file path.

> [!IMPORTANT]
> Videos will be returned in MP4 format and audio only dubs will be returned in MP3.

```csharp
var dubbedClipPath = await ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(metadata.DubbingId, request.TargetLanguage);
var dubbedClip = await Rest.DownloadAudioClipAsync($"file://{dubbedClipPath}", AudioType.MPEG);
audioSource.PlayOneShot(dubbedClip);
```

#### Get Transcript for Dub

Returns transcript for the dub in the desired format.

```csharp
var srcFile = new FileInfo(audioPath);
var transcriptPath = new FileInfo($"{srcFile.FullName}.dubbed.{request.TargetLanguage}.srt");
var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(metadata.DubbingId, request.TargetLanguage);
await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
```

#### Delete Dubbing Project

Deletes a dubbing project.

```csharp
var api = new ElevenLabsClient();
await api.DubbingEndpoint.DeleteDubbingProjectAsync("dubbing-id");
```

### SFX Generation

API that converts text into sounds & uses the most advanced AI audio model ever.

```csharp
var api = new ElevenLabsClient();
var request = new SoundGenerationRequest("Star Wars Light Saber parry");
var clip = await api.SoundGenerationEndpoint.GenerateSoundAsync(request);
```

### [History](https://docs.elevenlabs.io/api-reference/history)

Access to your previously synthesized audio clips including its metadata.

#### Get History

Get metadata about all your generated audio.

```csharp
var api = new ElevenLabsClient();
var historyItems = await api.HistoryEndpoint.GetHistoryAsync();

foreach (var item in historyItems.OrderBy(historyItem => historyItem.Date))
{
    Debug.Log($"{item.State} {item.Date} | {item.Id} | {item.Text.Length} | {item.Text}");
}
```

#### Get History Item

Get information about a specific item.

```csharp
var api = new ElevenLabsClient();
var historyItem = await api.HistoryEndpoint.GetHistoryItemAsync(voiceClip.Id);
```

#### Download History Audio

```csharp
var api = new ElevenLabsClient();
var voiceClip = await api.HistoryEndpoint.DownloadHistoryAudioAsync(historyItem);
```

#### Download History Items

Downloads the last 100 history items, or the collection of specified items.

```csharp
var api = new ElevenLabsClient();
var voiceClips = await api.HistoryEndpoint.DownloadHistoryItemsAsync();
```

> Note: to copy the clips directly into your project you can additionally call:

```csharp
VoiceClipUtilities.CopyIntoProject(editorDownloadDirectory, downloadItems.ToArray());
```

#### Delete History Item

```csharp
var api = new ElevenLabsClient();
var success = await api.HistoryEndpoint.DeleteHistoryItemAsync(historyItem);
Debug.Log($"Was successful? {success}");
```

### [User](https://docs.elevenlabs.io/api-reference/user)

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
