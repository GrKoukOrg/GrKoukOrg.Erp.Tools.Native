using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ItemDetailsPageModel : ObservableObject
{
    private readonly LocalItemsRepo _localItemsRepo;
    [ObservableProperty] private ICollection<ItemListDto> _items;
    [ObservableProperty] private ItemListDto _selectedItem;

    [ObservableProperty] private ICollection<SearchItem> _searchItems;

    public ItemDetailsPageModel(LocalItemsRepo localItemsRepo)
    {
        _localItemsRepo = localItemsRepo;
    }

    [RelayCommand]
    private async Task SelectionChanged(object selectedItem)
    {
        if (selectedItem is SearchItem selectedSearchItem)
        {
           // Optionally find and set _selectedItem
           SelectedItem = Items.FirstOrDefault(item => item.Id == selectedSearchItem.Id);
        }
    }

    
    [RelayCommand]
    private async Task Appearing()
    {
        Items = await _localItemsRepo.ListAsync();
        SearchItems = Items.Select(p => new SearchItem()
        {
            Id=p.Id,
            SearchText = $"{p.Code},{p.Name},{p.Barcodes}"
        }).ToList();
    }
}