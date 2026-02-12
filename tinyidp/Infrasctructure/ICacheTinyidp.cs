

public interface ICacheTinyidp<T>
{
    void Set(string key, T value);
    T? Get(string key);
    void Remove(string key);
}