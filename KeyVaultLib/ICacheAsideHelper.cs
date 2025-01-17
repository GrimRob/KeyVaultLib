using System.Threading.Tasks;
using System;

namespace KeyVaultLib;

public interface ICacheAsideHelper
{
    string GetKey(params string[] keys);
    void RemoveKey(string key);
    void Add(object item, string key);
    T GetOrAdd<T>(Func<T> builder, TimeSpan expiresIn, params string[] keys);
    Task<T> GetOrAddAsync<T>(Func<Task<T>> builder, TimeSpan expiresIn, params string[] keys);
}