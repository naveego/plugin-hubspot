
using System.Linq;
using Xunit;
using FluentAssertions;
using Pub;

namespace Plugin_Hubspot.HubSpotApi
{
    public class DynamicApiSchema_SchemaGenerationTests
    {
        private Schema SchemaToTest { get; set; }

     
        public DynamicApiSchema_SchemaGenerationTests()
        {
            
            var d = new DynamicApiSchema(
                DynamicObject.Contacts,
                "Contacts",
                "A list of contacts for my company");

    
            d.AddProperty(new APIProperty
            {
                Name = "first_name", 
                Label = "First Name",
                Type = "string", 
                Description = "First Name of contact"
            });
            
            d.AddProperty(new APIProperty
            {
                Name = "coolness_factor", 
                Label = "Coolness Factor",
                Type = "number", 
                Description = "How cool is this person?"
            });
            
            d.AddProperty(new APIProperty
            {
                Name = "date_of_birth", 
                Label = "Date of Birth",
                Type = "datetime", 
                Description = "When was this person born"
            });

            SchemaToTest = d.ToSchema();
        }

        [Fact]
        public void ShouldSetTheSchemaIdToTheLowercaseValueOfObject()
        {
            SchemaToTest.Id.Should().Be("contacts");
        }

        [Fact]
        public void ShouldSetIdNameAndDescriptionOnSchema()
        {
            SchemaToTest.Id.Should().Be("contacts");
            SchemaToTest.Name.Should().Be("Contacts");
            SchemaToTest.Description.Should().Be("A list of contacts for my company");
        }

        [Fact]
        public void ShouldResultsInTheCorrectNumberOfProperties()
        {
            SchemaToTest.Properties.Should().HaveCount(3, "Because we added three properties");
        }

        // See: https://developers.hubspot.com/docs/methods/crm-properties/create-property
        [Theory]
        [InlineData("string", PropertyType.String)]
        [InlineData("number", PropertyType.Decimal)]
        [InlineData("bool", PropertyType.Bool)]
        [InlineData("datetime", PropertyType.Datetime)]
        [InlineData("enumeration", PropertyType.String)]
        public void ShouldConvertPropertyTypes(string apiType, PropertyType expectedType)
        {
            var d = new DynamicApiSchema(DynamicObject.Contacts, "", "");
            
            d.AddProperty(new APIProperty
            {
                Name = "",
                Description = "",
                Type = apiType
            });

            d.ToSchema().Properties.First().Type.Should().Be(expectedType);
        }

        
        
    }
}