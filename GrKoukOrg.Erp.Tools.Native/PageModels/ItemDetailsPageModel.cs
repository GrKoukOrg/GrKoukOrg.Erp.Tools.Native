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
    private readonly LocalBuyDocumentsRepo _localBuyDocumentsRepo;
    private readonly LocalBuyDocLinesRepo _localBuyDocLinesRepo;
    [ObservableProperty] private ICollection<ItemListDto> _items;
    [ObservableProperty] private ItemListDto _selectedItem;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ICollection<SearchItem> _searchItems;
[ObservableProperty] private ItemStatisticsDto _itemStatistics=new ItemStatisticsDto();
    public ItemDetailsPageModel(LocalItemsRepo localItemsRepo, LocalBuyDocumentsRepo localBuyDocumentsRepo,
        LocalBuyDocLinesRepo localBuyDocLinesRepo)
    {
        _localItemsRepo = localItemsRepo;
        _localBuyDocumentsRepo = localBuyDocumentsRepo;
        _localBuyDocLinesRepo = localBuyDocLinesRepo;
    }

    private async Task CalculateItemStatistics(int itemId)
    {
        var buyDocuments = await _localBuyDocumentsRepo.ListBuyDocsAsync();
        var buyDocLines = await _localBuyDocLinesRepo.ListBuyDocLinesAsync();
        var itemData = buyDocuments
            .Join(
                buyDocLines,
                doc => doc.Id,
                line => line.BuyDocId,
                (doc, line) => new
                    { doc.BuyDocDefId, line.UnitQty, line.UnitTotalAmount, line.UnitDiscountAmount, line.ItemId }
            )
            .Where(x => x.ItemId == itemId);
        var totalQuantityInWarehouse = itemData
            .Sum(x =>
                x.BuyDocDefId == 9 // Purchase
                    ? x.UnitQty
                    : x.BuyDocDefId == 17 // Return
                        ? -x.UnitQty
                        : 0);
        var totalPurchaseCost = itemData
            .Sum(x =>
                x.BuyDocDefId == 9 // Purchase
                    ? x.UnitTotalAmount
                    : x.BuyDocDefId == 17 // Return
                        ? -x.UnitTotalAmount
                        : 0);
        
        // .Where(x => x.BuyDocDefId == 9) // Purchases only
            // .Sum(x => x.UnitTotalAmount);

        var totalDiscountCost = itemData
            .Where(x => x.BuyDocDefId == 14) // Discounts only
            .Sum(x => x.UnitDiscountAmount);
        decimal meanPrice = totalQuantityInWarehouse > 0
            ? (totalPurchaseCost - totalDiscountCost) / totalQuantityInWarehouse
            : 0;
        ItemStatistics = new ItemStatisticsDto()
        {
            ItemId = itemId,
            MeanPrice = meanPrice,
            TotalQuantityInWarehouse = totalQuantityInWarehouse
        };
        
    }

    [RelayCommand]
    private async Task SelectionChanged(object selectedItem)
    {
        if (selectedItem is SearchItem selectedSearchItem)
        {
            // Optionally find and set _selectedItem
            SelectedItem = Items.FirstOrDefault(item => item.Id == selectedSearchItem.Id);
            CalculateItemStatistics(SelectedItem.Id);
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

                    var barcodeItem = Items.FirstOrDefault(item => item.Barcodes.Contains(SearchText));
                    if (barcodeItem is not null)
                    {
                        SelectedItem = barcodeItem;
                        CalculateItemStatistics(SelectedItem.Id);
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
            Id = p.Id,
            SearchText = $"{p.Code},{p.Name},{p.Barcodes}"
        }).ToList();
    }
}