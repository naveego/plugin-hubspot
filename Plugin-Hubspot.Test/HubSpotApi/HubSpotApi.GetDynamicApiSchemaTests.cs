using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace Plugin_Hubspot.HubSpotApi
{
    public class HubSpotApiTests_GetDynamincSchemaTests
    {

        private readonly HubSpotApiClient sut;

        public HubSpotApiTests_GetDynamincSchemaTests()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            sut = new HubSpotApiClient(mockHttp.ToHttpClient());
            
            sut.UseApiToken("123");
            
            mockHttp.When("https://api.hubapi.com/properties/v1/contacts/properties")
                .RespondWithJsonFile("TestData/contact.properties.json");
        }

        [Fact]
        public async void ShouldReadAllPropertiesFromResponse()
        {
            // Act
            var ds = await sut.GetDynamicApiSchema(DynamicObject.Contacts);
            
            // Assert
            ds.Properties.Should().HaveCount(168, "All of the defined properties plus the Vid");
        }

        [Fact]
        public async void ShouldDeserializeCorePropertyAttributesCorrectly()
        {
            // Act
            var ds = await sut.GetDynamicApiSchema(DynamicObject.Contacts);
            
            // Assert
            var companySize = ds.Properties.First(p => p.Name == "company_size");
            companySize.Label.Should().Be("Company size");
            companySize.Description.Should().Be("A contact's company size. This property is required for the Facebook Ads Integration. This property will be automatically synced via the Lead Ads tool");
            companySize.Type.Should().Be("string");
            companySize.DisplayOrder.Should().Be(-1);
            companySize.GroupName.Should().Be("contactinformation");
        }
    }
}