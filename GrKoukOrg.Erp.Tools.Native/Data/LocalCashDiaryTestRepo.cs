using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public class LocalCashDiaryTestRepo : ILocalCashDiaryRepo<CashDiaryItemDto>
{
    public Task Init()
    {
        throw new NotImplementedException();
    }

    public Task<List<CashDiaryItemDto>> ListItemsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> ItemExist(int id)
    {
        throw new NotImplementedException();
    }

    public Task<CashDiaryItemDto?> GetItemAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> AddItemAsync(CashDiaryItemDto item)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateItemAsync(CashDiaryItemDto item)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteItemAsync(CashDiaryItemDto item)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAllItemsAsync()
    {
        throw new NotImplementedException();
    }
}