using System;
using System.Collections.Generic;
using System.Text;

namespace ArtifactsOfMight.RunConfig
{
    /// <summary>
    /// Sorta controls our logger levels/paths
    /// 
    /// Or I guess i could build like a logger with path keys n stuff but that seems
    /// mega overkill atm
    /// 
    /// TODO switch to scoped logger
    /// </summary>
    public static class LoggerSettings
    {

        /// <summary>
        /// Controls some logging in our PreventWeirdSizeOnClientSetPickupOptions function
        /// </summary>
        public static bool LOG_CLIENT_PICKUP_OPTIONS = false;

        /// <summary>
        /// Controls logging for us building draft options for a player
        /// </summary>
        public static bool LOG_BUILD_DRAFT_OPTIONS = true;


        /// <summary>
        /// The scopped logger would be weird if it had its own scoped logger
        /// So we control its logging with these settings
        /// </summary>
        public static class Scopped
        {
            /// <summary>
            /// Whether we log when we check matching packages or not
            /// </summary>
            public static bool LOG_LOGGER_MATCHES = false;
        }
    }
}
