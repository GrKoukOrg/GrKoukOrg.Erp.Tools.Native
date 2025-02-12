using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Maui.Controls;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ItemDetailsPageModel : ObservableObject
{
    private readonly LocalItemsRepo _localItemsRepo;
    [ObservableProperty] private ICollection<ItemListDto> _items;
    [ObservableProperty] private ItemListDto _selectedItem;
    [ObservableProperty]
    private string _searchText; 
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
    private async Task ScanBarcode()
    {
        var barcodeScannerPopupPage = new BarCodeScannerPopupPage();
        var currentPage = Application.Current?.MainPage;
        if (currentPage != null)
        {
            var popup = new BarCodeScannerPopupPage();
            var result = await currentPage.ShowPopupAsync(barcodeScannerPopupPage, CancellationToken.None);

            if (result is not null)
            {
                try
                {
                    SearchText = result.ToString();
                    
                    var barcodeItem = Items.FirstOrDefault(item => item.Barcodes.Contains(SearchText) );
                    if (barcodeItem is not null)
                    {
                        SelectedItem=barcodeItem;
                    }
                }
                catch
                {
                    Console.WriteLine();
                }
               
            }
            else
            {
                Console.WriteLine("Null result or cancelled");
            }
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