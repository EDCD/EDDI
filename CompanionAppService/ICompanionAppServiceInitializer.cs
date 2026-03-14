namespace EddiCompanionAppService
{
    /// <summary>
    /// This interface allows DDE initialization to be deferred until after the UI context is established.
    /// </summary>
    public interface ICompanionAppServiceInitializer
    {
        /// <summary>
        /// Initialize the DDE responder for OAuth callbacks. This should only be called if a UI dispatcher is available.
        /// </summary>
        void InitializeOAuthCallback();
    }
}
