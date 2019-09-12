using System;
using System.Linq;
using RichardSzalay.MockHttp;
using Xunit;
using FluentAssertions;

namespace Plugin_Hubspot.HubSpotApi
{
    public class HubSpotApi_GetRecordsTests
    {
        private readonly HubSpotApiClient sut;

        public HubSpotApi_GetRecordsTests()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            sut = new HubSpotApiClient(mockHttp.ToHttpClient());
            
            sut.UseApiToken("123");
            
            mockHttp.When("https://api.hubapi.com/contacts/v1/lists/all/contacts/all")
                .RespondWithJsonFile("TestData/contacts.json");
            
            mockHttp.When("https://api.hubapi.com/companies/v2/companies/paged")
                .RespondWithJsonFile("TestData/companies.json");
            
            mockHttp.When("https://api.hubapi.com/properties/v1/contacts/properties")
                .RespondWithJsonFile("TestData/contact.properties.json");
            
            mockHttp.When("https://api.hubapi.com/properties/v1/companies/properties")
                .RespondWithJsonFile("TestData/companies.properties.json");
        }

        [Fact]
        public async void CanReadContacts()
        {
            // Act
            var ds = await sut.GetRecords(DynamicObject.Contacts);
            
            // Assert
            ds.Records.Count.Should().Be(2, "that is how many records there are");
            var record = ds.Records.First();
            record["firstname"].Should().Be("Cool");
            record["lastmodifieddate"].Should().Be(new DateTime(2019, 9, 3, 21,14,57,101));
            record["company"].Should().Be("HubSpot");
        }
        
        [Fact]
        public async void CanReadCompanies()
        {
            // Act
            var ds = await sut.GetRecords(DynamicObject.Companies);
            
            // Assert
            ds.Records.Count.Should().Be(1, "that is how many records there are");
         
        }
        
        [Fact]
        public async void ShouldInjectTheIdPropValue()
        {
            // Act
            var ds = await sut.GetRecords(DynamicObject.Contacts);
            
            // Assert
            ds.Records.Count.Should().Be(2, "that is how many records there are");
            var record = ds.Records.First();
            record["vid"].Should().Be("1");
          
        }
    }
}