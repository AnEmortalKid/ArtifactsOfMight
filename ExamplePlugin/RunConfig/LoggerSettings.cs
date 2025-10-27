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
    }
}
