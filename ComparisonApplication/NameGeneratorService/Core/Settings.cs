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


        // TODO: set these correctly


        /// <summary>
        /// The base path of the random number generation service
        /// </summary>
        public const string RandomServiceBaseUrl = "http://localhost:8835";


        /// <summary>
        /// The base path of the random number generation service
        /// </summary>
        public const string LookupServiceBaseUrl = "http://localhost:8365";
    }
}
