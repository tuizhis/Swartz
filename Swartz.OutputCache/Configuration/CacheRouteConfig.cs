namespace Swartz.OutputCache.Configuration
{
    public class CacheRouteConfig
    {
        public int? Duration { get; set; }
        public int? GraceTime { get; set; }
        public int? MaxAge { get; set; }
    }
}