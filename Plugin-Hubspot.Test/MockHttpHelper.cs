using RichardSzalay.MockHttp;

namespace Plugin_Hubspot
{
    public static class MockHttpHelper
    {

        public static void RespondWithJsonFile(this MockedRequest request, string filePath)
        {
            var fileContent = System.IO.File.ReadAllText(filePath);
            request.Respond("application/json", fileContent);
        }

    }
}