namespace ProjectIdle
{
    public interface ILocalDB
    {
        void Save<T>(string key, T value);
        T Load<T>(string key);
        bool Exists(string key);
        void Delete(string key);
        void Clear();
    }
}