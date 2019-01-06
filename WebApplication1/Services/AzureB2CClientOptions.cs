namespace WebApplication1.Services
{
    public class AzureB2CClientOptions
    {
        public string LoginUrl { get; set; }
        public string DirectoryId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RolePrefix { get; set; }
    }
}
