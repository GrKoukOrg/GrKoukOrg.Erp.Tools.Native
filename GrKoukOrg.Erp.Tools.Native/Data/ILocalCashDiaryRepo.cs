using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public interface ILocalCashDiaryRepo<T>
{
    Task Init();
    Task<List<T>> ListItemsAsync();
    Task<bool> ItemExist(int id);
    Task<T?> GetItemAsync(int id);
    Task<int> AddItemAsync(T item);
    Task<int> UpdateItemAsync(T item);
    Task<int> DeleteItemAsync(T item);
    Task<int> DeleteAllItemsAsync();
}
