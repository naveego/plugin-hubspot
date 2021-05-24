using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginHubspot.API.Factory;
using PluginHubspot.DataContracts;

namespace PluginHubspot.API.Utility.EndpointHelperEndpoints
{
    public class FeedbackSubmissionsEndpointHelper
    {
        

        private class FeedbackSubmissionsEndpoint : Endpoint
        {
        }

        public static readonly Dictionary<string, Endpoint> FeedbackSubmissionsEndpoints = new Dictionary<string, Endpoint>
        {
            {
                "AllFeedbackSubmissions", new FeedbackSubmissionsEndpoint
                {
                    Id = "AllFeedbackSubmissions",
                    Name = "All Feedback Submissions",
                    BasePath = "/crm/v3/",
                    AllPath = "/objects/feedback_submissions",
                    PropertiesPath = "/crm/v3/properties/feedback_submissions",
                    DetailPath = "/objects/feedback_submissions/{0}",
                    DetailPropertyId = "hs_unique_creation_key",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Get
                    },
                    PropertyKeys = new List<string>
                    {
                        "hs_unique_creation_key"
                    }
                }
            },
            {
                "UpsertFeedbackSubmissions", new FeedbackSubmissionsEndpoint
                {
                    Id = "UpsertFeedbackSubmissions",
                    Name = "Upsert Feedback Submissions",
                    BasePath = "/crm/v3/objects/feedback_submissions",
                    AllPath = "/",
                    DetailPath = "/",
                    DetailPropertyId = "hs_unique_creation_key",
                    SupportedActions = new List<EndpointActions>
                    {
                        EndpointActions.Post,
                        EndpointActions.Put
                    },
                    PropertyKeys = new List<string>
                    {
                        "hs_unique_creation_key"
                    }
                }
            },
        };
    }
}