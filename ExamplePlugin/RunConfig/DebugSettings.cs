namespace ArtifactsOfMight.RunConfig
{
    /// <summary>
    /// Centralized location for toggling the local development hooks
    /// </summary>
    public static class DebugSettings
    {
        /// <summary>
        /// I have a bunch of cheating / wip hackery while devving that should be disabled
        /// specifically like giving myself money or spawning random items
        /// </summary>
        public static bool IS_DEV_MODE = false;

        /// <summary>
        /// Lets me test multiplayer alone without steam connecting and insta killing my other client
        /// https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Testing/Testing-Multiplayer-Alone/
        /// </summary>
        public static bool LOCAL_NETWORK_TEST = false;

        /// <summary>
        /// Determines whether im gonna log all the draft pool spam or not
        /// </summary>
        public static bool LOG_DRAFT_POOLS_INFO = false;
    }
}
