﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WebApplication1.Services
{
    public class MSGraphClient
    {
        private readonly string _groupPrefix;
        private readonly string _bearer;

        public MSGraphClient(MSGraphOptions options)
        {
            // prepare msgraph authentication request
            var client = new RestClient("https://login.microsoftonline.com");
            var request = new RestRequest($"{options.DirectoryId}/oauth2/v2.0/token")
            {
                AlwaysMultipartFormData = true
            };

            request.AddHeader("Cache-Control", "no-cache");
            request.AddParameter("client_id", options.ClientId);
            request.AddParameter("client_secret", options.ClientSecret);
            request.AddParameter("scope", "https://graph.microsoft.com/.default");
            request.AddParameter("grant_type", "client_credentials");

            // send and extract token
            var response = client.Post(request);
            var json = JObject.Parse(response.Content);
            _bearer = json.GetValue("access_token").Value<string>();

            // save group prefix for role filtering
            _groupPrefix = options.RolePrefix;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            // prepare msgraph memberOf request
            var client = new RestClient("https://graph.microsoft.com/v1.0");
            var request = new RestRequest($"users/{userId}/memberOf");
            request.AddHeader("Authorization", $"Bearer {_bearer}");

            // send and extract roles
            var response = await client.ExecuteGetTaskAsync(request);
            var roles = JObject.Parse(response.Content)["value"]
                .Select(o => o.Value<string>("displayName"));

            // use the role prefix to filter returned groups if supplied
            if (!string.IsNullOrEmpty(_groupPrefix))
                roles = roles.Where(o => o.StartsWith(_groupPrefix, StringComparison.OrdinalIgnoreCase))
                    .Select(o => o.Replace(_groupPrefix, string.Empty).Trim());

            return roles;
        }
    }
}
