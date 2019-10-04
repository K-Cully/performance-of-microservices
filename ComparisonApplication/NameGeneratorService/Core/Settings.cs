namespace NameGeneratorService.Core
{
    /// <summary>
    /// Provides access to common application settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The client name for accessing the random number generation API
        /// </summary>
        public const string RandomApiClientName = "randomApi";


        /// <summary>
        /// The client name for accessing the name id lookup API
        /// </summary>
        public const string NameLookupApiClientName = "nameLookupApi";


        /// <summary>
        /// The base path of the random number generation service
        /// </summary>
        public const string RandomServiceBaseUrl = BaseUrl + ":81";


        /// <summary>
        /// The base path of the random number generation service
        /// </summary>
        public const string LookupServiceBaseUrl = BaseUrl + ":82";


        private const string BaseUrl = "http://poc-comparison-neu.northeurope.cloudapp.azure.com";
    }
}
