namespace Swartz.OutputCache
{
    public interface ICacheService : IDependency
    {
        /// <summary>
        ///     Removes all cache entries associated with a specific tag.
        /// </summary>
        /// <param name="tag">The tag value.</param>
        void RemoveByTag(string tag);
    }
}