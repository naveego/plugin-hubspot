using System;
using PluginHubspot.Helper;
using Xunit;

namespace PluginHubspotTest.Helper
{
    public class SettingsTest
    {
        [Fact]
        public void ValidateValidTest()
        {
            // setup
            var settings = new Settings
            {
                ApiKey = "APIKEY",
            };

            // act
            settings.Validate();

            // assert
        }

        [Fact]
        public void ValidateNoApiKeyTest()
        {
            // setup
            var settings = new Settings
            {
                ApiKey = null,
            };

            // act
            Exception e = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("The Api Key property must be set", e.Message);
        }
    }
}