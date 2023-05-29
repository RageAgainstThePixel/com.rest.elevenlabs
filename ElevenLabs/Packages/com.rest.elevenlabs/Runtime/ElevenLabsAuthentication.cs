// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    /// <summary>
    /// Represents authentication for ElevenLabs
    /// </summary>
    public sealed class ElevenLabsAuthentication : AbstractAuthentication<ElevenLabsAuthentication, ElevenLabsAuthInfo>
    {
        internal const string CONFIG_FILE = ".elevenlabs";
        private const string ELEVEN_LABS_API_KEY = nameof(ELEVEN_LABS_API_KEY);

        /// <summary>
        /// Allows implicit casting from a string, so that a simple string API key can be provided in place of an instance of Authentication.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        public static implicit operator ElevenLabsAuthentication(string apiKey) => new ElevenLabsAuthentication(apiKey);

        /// <summary>
        /// Instantiates a new Authentication object that will load the default config.
        /// </summary>
        public ElevenLabsAuthentication()
        {
            if (cachedDefault != null)
            {
                return;
            }

            cachedDefault = (LoadFromAsset<ElevenLabsConfiguration>() ??
                             LoadFromDirectory()) ??
                             LoadFromDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) ??
                             LoadFromEnvironment();
            Info = cachedDefault?.Info;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="apiKey"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="apiKey">The API key, required to access the API endpoint.</param>
        public ElevenLabsAuthentication(string apiKey) => Info = new ElevenLabsAuthInfo(apiKey);

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="authInfo"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="authInfo"></param>
        public ElevenLabsAuthentication(ElevenLabsAuthInfo authInfo) => this.Info = authInfo;

        /// <inheritdoc />
        public override ElevenLabsAuthInfo Info { get; }

        private static ElevenLabsAuthentication cachedDefault;

        /// <summary>
        /// The default authentication to use when no other auth is specified.
        /// This can be set manually, or automatically loaded via environment variables or a config file.
        /// <seealso cref="LoadFromEnvironment"/><seealso cref="LoadFromDirectory"/>
        /// </summary>
        public static ElevenLabsAuthentication Default
        {
            get => cachedDefault ?? new ElevenLabsAuthentication();
            internal set => cachedDefault = value;
        }

        [Obsolete("Use ElevenLabsAuthentication.Info.ApiKey")]
        public string ApiKey => Info.ApiKey;

        /// <inheritdoc />
        public override ElevenLabsAuthentication LoadFromAsset<T>()
            => Resources.LoadAll<T>(string.Empty)
                .Where(asset => asset != null)
                .Where(asset => asset is ElevenLabsConfiguration config &&
                                !string.IsNullOrWhiteSpace(config.ApiKey))
                .Select(asset => asset is ElevenLabsConfiguration config
                    ? new ElevenLabsAuthentication(config.ApiKey)
                    : null)
                .FirstOrDefault();

        /// <inheritdoc />
        public override ElevenLabsAuthentication LoadFromEnvironment()
        {
            var apiKey = Environment.GetEnvironmentVariable(ELEVEN_LABS_API_KEY);
            return string.IsNullOrEmpty(apiKey) ? null : new ElevenLabsAuthentication(apiKey);
        }

        /// <inheritdoc />
        /// ReSharper disable once OptionalParameterHierarchyMismatch
        public override ElevenLabsAuthentication LoadFromDirectory(string directory = null, string filename = CONFIG_FILE, bool searchUp = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Environment.CurrentDirectory;
            }

            ElevenLabsAuthInfo tempAuthInfo = null;

            var currentDirectory = new DirectoryInfo(directory);

            while (tempAuthInfo == null && currentDirectory.Parent != null)
            {
                var filePath = Path.Combine(currentDirectory.FullName, filename);

                if (File.Exists(filePath))
                {
                    try
                    {
                        tempAuthInfo = JsonUtility.FromJson<ElevenLabsAuthInfo>(File.ReadAllText(filePath));
                        break;
                    }
                    catch (Exception)
                    {
                        // try to parse the old way for backwards support.
                    }

                    var lines = File.ReadAllLines(filePath);
                    string apiKey = null;

                    foreach (var line in lines)
                    {
                        var parts = line.Split('=', ':');

                        for (var i = 0; i < parts.Length - 1; i++)
                        {
                            var part = parts[i];
                            var nextPart = parts[i + 1];

                            switch (part)
                            {
                                case ELEVEN_LABS_API_KEY:
                                    apiKey = nextPart.Trim();
                                    break;
                            }
                        }
                    }

                    tempAuthInfo = new ElevenLabsAuthInfo(apiKey);
                }

                if (searchUp)
                {
                    currentDirectory = currentDirectory.Parent;
                }
                else
                {
                    break;
                }
            }

            if (tempAuthInfo == null ||
                string.IsNullOrEmpty(tempAuthInfo.ApiKey))
            {
                return null;
            }

            return new ElevenLabsAuthentication(tempAuthInfo);
        }

        [Obsolete("use ElevenLabsAuthentication.Default.LoadFromEnvironment")]
        public static ElevenLabsAuthentication LoadFromEnv() => Default.LoadFromEnvironment();
    }
}
