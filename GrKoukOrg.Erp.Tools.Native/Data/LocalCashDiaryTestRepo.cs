using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public class LocalCashDiaryTestRepo : ILocalCashDiaryRepo<CashDiaryItemDto>
{
    private List<CashDiaryItemDto> _items;

    public LocalCashDiaryTestRepo()
    {
        _items = new List<CashDiaryItemDto>()
        {
            new CashDiaryItemDto
            {
                Id = 1,
                TransDate = DateTime.Now,
                TransId = 101,
                TransName = "Cash Sale",
                Etiologia = "Daily retail sale",
                NetAmount = 100.00m,
                VatAmount = 24.00m,
                TotalAmount = 124.00m
            },
            new CashDiaryItemDto
            {
                Id = 2,
                TransDate = DateTime.Now.AddDays(-1),
                TransId = 102,
                TransName = "Cash Purchase",
                Etiologia = "Office supplies",
                NetAmount = 50.00m,
                VatAmount = 12.00m,
                TotalAmount = 62.00m
            },
            new CashDiaryItemDto
            {
                Id = 3,
                TransDate = DateTime.Now.AddDays(-1),
                TransId = 103,
                TransName = "Service Payment",
                Etiologia = "IT Services",
                NetAmount = 200.00m,
                VatAmount = 48.00m,
                TotalAmount = 248.00m
            },
            new CashDiaryItemDto
            {
                Id = 4,
                TransDate = DateTime.Now.AddDays(-2),
                TransId = 104,
                TransName = "Cash Deposit",
                Etiologia = "Bank deposit",
                NetAmount = 1000.00m,
                VatAmount = 0.00m,
                TotalAmount = 1000.00m
            },
            new CashDiaryItemDto
            {
                Id = 5,
                TransDate = DateTime.Now.AddDays(-2),
                TransId = 105,
                TransName = "Utility Payment",
                Etiologia = "Electricity bill",
                NetAmount = 150.00m,
                VatAmount = 36.00m,
                TotalAmount = 186.00m
            },
            new CashDiaryItemDto
            {
                Id = 6,
                TransDate = DateTime.Now.AddDays(-3),
                TransId = 106,
                TransName = "Cash Sale",
                Etiologia = "Wholesale transaction",
                NetAmount = 500.00m,
                VatAmount = 120.00m,
                TotalAmount = 620.00m
            },
            new CashDiaryItemDto
            {
                Id = 7,
                TransDate = DateTime.Now.AddDays(-3),
                TransId = 107,
                TransName = "Equipment Purchase",
                Etiologia = "New printer",
                NetAmount = 400.00m,
                VatAmount = 96.00m,
                TotalAmount = 496.00m
            },
            new CashDiaryItemDto
            {
                Id = 8,
                TransDate = DateTime.Now.AddDays(-4),
                TransId = 108,
                TransName = "Maintenance Fee",
                Etiologia = "Monthly maintenance",
                NetAmount = 75.00m,
                VatAmount = 18.00m,
                TotalAmount = 93.00m
            },
            new CashDiaryItemDto
            {
                Id = 9,
                TransDate = DateTime.Now.AddDays(-4),
                TransId = 109,
                TransName = "Cash Withdrawal",
                Etiologia = "Petty cash",
                NetAmount = 300.00m,
                VatAmount = 0.00m,
                TotalAmount = 300.00m
            },
            new CashDiaryItemDto
            {
                Id = 10,
                TransDate = DateTime.Now.AddDays(-5),
                TransId = 110,
                TransName = "Insurance Payment",
                Etiologia = "Annual insurance premium",
                NetAmount = 800.00m,
                VatAmount = 0.00m,
                TotalAmount = 800.00m
            }
        };
    }

    public Task Init()
    {
       // _items = _items ?? new List<CashDiaryItemDto>();
        return Task.CompletedTask;
    }

    public Task<List<CashDiaryItemDto>> ListItemsAsync()
    {
        return Task.FromResult(_items);
    }

    public Task<bool> ItemExist(int id)
    {
        return Task.FromResult(_items.Any(item => item.Id == id));
    }

    public Task<CashDiaryItemDto?> GetItemAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(item);
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