using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WebApplication1.Services
{
    public class AzureB2CClient
    {
        private readonly string _directoryId;
        private readonly string _groupPrefix;
        private readonly string _bearer;

        public AzureB2CClient(AzureB2CClientOptions options)
        {
            _directoryId = options.DirectoryId;
            _groupPrefix = options.RolePrefix;

            // prepare request
            var client = new RestClient(options.LoginUrl);
            var request = new RestRequest($"{_directoryId}/oauth2/token")
            {
                AlwaysMultipartFormData = true
            };

            request.AddHeader("Cache-Control", "no-cache");
            request.AddParameter("resource", "https://graph.windows.net");
            request.AddParameter("client_id", options.ClientId);
            request.AddParameter("client_secret", options.ClientSecret);
            request.AddParameter("grant_type", "client_credentials");

            // send and parse
            var response = client.Post(request);
            var json = JObject.Parse(response.Content);
            _bearer = json.GetValue("access_token").Value<string>();
        }
        
        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            // prepare request
            var client = new RestClient("https://graph.windows.net");
            var request = new RestRequest($"{_directoryId}/users/{userId}/memberOf");
            request.AddHeader("Authorization", $"Bearer {_bearer}");
            request.AddQueryParameter("api-version", "1.6");

            // send
            var response = await client.ExecuteGetTaskAsync(request);
            
            // parse roles
            var roles = JObject.Parse(response.Content)["value"]
                .Select(o => o.Value<string>("displayName"))
                .Where(o => o.StartsWith(_groupPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(o => o.Replace(_groupPrefix, string.Empty).Trim());

            return roles;
        }
    }
}
