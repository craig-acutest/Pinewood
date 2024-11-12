namespace Pinewood.Services
{
    public interface ICacheService
    {
        List<T> Get<T>(string key);
        void Update<T>(string key, List<T> Data);
        void Remove(string key);
    }
}
