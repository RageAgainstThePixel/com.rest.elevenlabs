// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Authentication;
using UnityEditor;
using UnityEngine;

namespace ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_00_Authentication
    {
        [SetUp]
        public void Setup()
        {
            var authJson = new ElevenLabsAuthInfo("key-test12");
            var authText = JsonUtility.ToJson(authJson, true);
            File.WriteAllText(".elevenlabs", authText);
        }

        [Test]
        public void Test_01_GetAuthFromEnv()
        {
            var auth = ElevenLabsAuthentication.Default.LoadFromEnvironment();
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.IsNotEmpty(auth.Info.ApiKey);
        }

        [Test]
        public void Test_02_GetAuthFromFile()
        {
            var auth = ElevenLabsAuthentication.Default.LoadFromDirectory();
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-test12", auth.Info.ApiKey);
        }

        [Test]
        public void Test_03_GetAuthFromNonExistentFile()
        {
            var auth = ElevenLabsAuthentication.Default.LoadFromDirectory(filename: "bad.config");
            Assert.IsNull(auth);
        }


        [Test]
        public void Test_04_GetAuthFromConfiguration()
        {
            var config = ScriptableObject.CreateInstance<ElevenLabsConfiguration>();
            config.ApiKey = "key-test12";
            var cleanup = false;

            if (!Directory.Exists($"{Application.dataPath}/Resources"))
            {
                Directory.CreateDirectory($"{Application.dataPath}/Resources");
                cleanup = true;
            }

            AssetDatabase.CreateAsset(config, $"Assets/Resources/{nameof(ElevenLabsConfiguration)}-Test.asset");

            var configPath = AssetDatabase.GetAssetPath(config);
            var auth = ElevenLabsAuthentication.Default;

            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.IsNotEmpty(auth.Info.ApiKey);
            Assert.AreEqual(auth.Info.ApiKey, config.ApiKey);
            AssetDatabase.DeleteAsset(configPath);

            if (cleanup)
            {
                AssetDatabase.DeleteAsset("Assets/Resources");
            }
        }

        [Test]
        public void Test_05_Authentication()
        {
            var defaultAuth = new ElevenLabsAuthentication();
            Assert.IsNotNull(defaultAuth);
            Assert.IsNotNull(defaultAuth.Info.ApiKey);
            Assert.AreEqual(defaultAuth.Info.ApiKey, defaultAuth.Info.ApiKey);

            var manualAuth = new ElevenLabsAuthentication("key-testAA");
            Assert.IsNotNull(manualAuth);
            Assert.IsNotNull(manualAuth.Info.ApiKey);
            Assert.AreEqual(manualAuth.Info.ApiKey, manualAuth.Info.ApiKey);
        }

        [Test]
        public void Test_06_GetKey()
        {
            var auth = new ElevenLabsAuthentication("key-testAA");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testAA", auth.Info.ApiKey);
        }

        [Test]
        public void Test_07_GetKeyFailed()
        {
            ElevenLabsAuthentication auth = null;

            try
            {
                auth = new ElevenLabsAuthentication("fail-key");
            }
            catch (InvalidCredentialException)
            {
                Assert.IsNull(auth);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, $"Expected exception {nameof(InvalidCredentialException)} but got {e.GetType().Name}");
            }
        }

        [Test]
        public void Test_08_ParseKey()
        {
            var auth = new ElevenLabsAuthentication("key-testAA");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testAA", auth.Info.ApiKey);
            auth = "key-testCC";
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testCC", auth.Info.ApiKey);

            auth = new ElevenLabsAuthentication("key-testBB");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testBB", auth.Info.ApiKey);
        }

        [Test]
        public void Test_09_CustomDomainConfigurationSettings()
        {
            var auth = new ElevenLabsAuthentication("customIssuedToken");
            var settings = new ElevenLabsSettings(domain: "api.your-custom-domain.com");
            var api = new ElevenLabsClient(auth, settings);
            Console.WriteLine(api.Settings.BaseRequestUrlFormat);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(".elevenlabs"))
            {
                File.Delete(".elevenlabs");
            }
        }
    }
}
