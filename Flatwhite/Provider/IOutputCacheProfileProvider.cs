namespace Flatwhite.Provider
{
    /// <summary>
    /// Output cache profile provider
    /// </summary>
    public interface IOutputCacheProfileProvider
    {
        /// <summary>
        /// Get a profile by profileName
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="profileName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        void ApplyProfileSetting<T>(T obj, string profileName) where T : class, new();
    }
}
