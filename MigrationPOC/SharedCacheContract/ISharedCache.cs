using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCacheContract
{
    public interface ISharedCache
    {
        Task<T> GetItemAsync<T>(string key);

        Task SetItemAsync<T>(string key, T item, TimeSpan expired);

        Task SetItemAsync<T>(string key, T item);
    }
}
