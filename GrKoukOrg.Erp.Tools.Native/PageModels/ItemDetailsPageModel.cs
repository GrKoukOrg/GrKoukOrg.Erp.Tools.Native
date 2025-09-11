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
using GrKoukOrg.Erp.Tools.Native.Data;
using Microsoft.Maui.Controls;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ItemDetailsPageModel : ObservableObject
{
    private readonly LocalItemsRepo _localItemsRepo;
    private readonly LocalBuyDocumentsRepo _localBuyDocumentsRepo;
    private readonly LocalBuyDocLinesRepo _localBuyDocLinesRepo;
    private readonly LocalSaleDocumentsRepo _localSaleDocumentsRepo;
    private readonly LocalSalesDocLinesRepo _localSalesDocLinesRepo;
    private readonly LocalCostTrackingRepo _localCostTrackingRepo;
    [ObservableProperty] private ICollection<ItemListDto> _items;
    [ObservableProperty] private ItemListDto _selectedItem;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ICollection<SearchItem> _searchItems;
    [ObservableProperty] private ItemStatisticsDto _itemStatistics = new ItemStatisticsDto();
    [ObservableProperty] private decimal _markupPercentage = 0;
    [ObservableProperty] private decimal _markupAmount = 0;
    [ObservableProperty] private decimal _currentCost = 0;

    public ItemDetailsPageModel(LocalItemsRepo localItemsRepo, LocalBuyDocumentsRepo localBuyDocumentsRepo,
        LocalBuyDocLinesRepo localBuyDocLinesRepo
        , LocalSaleDocumentsRepo localSaleDocumentsRepo
        , LocalSalesDocLinesRepo localSalesDocLinesRepo
        , LocalCostTrackingRepo localCostTrackingRepo
    )
    {
        _localItemsRepo = localItemsRepo;
        _localBuyDocumentsRepo = localBuyDocumentsRepo;
        _localBuyDocLinesRepo = localBuyDocLinesRepo;
        _localSaleDocumentsRepo = localSaleDocumentsRepo;
        _localSalesDocLinesRepo = localSalesDocLinesRepo;
        _localCostTrackingRepo = localCostTrackingRepo;
    }

    private async Task CalculateItemStatistics(int itemId)
    {
        var item = await _localItemsRepo.GetAsync(itemId);

        // Fetch aggregate stats directly from the database using JOINs
        var purchaseAgg = await _localBuyDocLinesRepo.GetItemPurchaseAggregatesAsync(itemId);
        var salesAgg = await _localSalesDocLinesRepo.GetItemSalesAggregatesAsync(itemId);

        var totalQuantityPurchased = purchaseAgg.TotalQuantityPurchased;
        var totalQuantitySold = salesAgg.TotalQuantitySold;

        var totalPurchaseCost = purchaseAgg.TotalPurchaseCost;
        var totalSaleIncome = salesAgg.TotalSaleIncome;

        var totalDiscountCost = purchaseAgg.TotalDiscountCost;

        decimal meanPrice = totalQuantityPurchased > 0
            ? (totalPurchaseCost - totalDiscountCost) / totalQuantityPurchased
            : 0;

        decimal meanSalesPrice = totalQuantitySold > 0 ? totalSaleIncome / totalQuantitySold : 0;
        var salePriceBrut = item?.TimiPolisisLianFpa;
        decimal markupPercentage = (decimal)(salePriceBrut > 0 ? (salePriceBrut - meanPrice) / meanPrice * 100 : 0);
        decimal markupAmountBrut = (decimal)(salePriceBrut > 0 ? salePriceBrut - meanPrice : 0);

        ItemStatistics = new ItemStatisticsDto()
        {
            ItemId = itemId,
            MeanPrice = meanPrice,
            TotalQuantityInWarehouse = totalQuantityPurchased,
            MeanSalesPrice = meanSalesPrice,
            MarkUpAmountNet = markupAmountBrut,
            MarkUpPercentage = markupPercentage,
            TotalQuantitySold = totalQuantitySold,
        };

        try
        {
            // Get the last cost for the current date
            CurrentCost = await _localCostTrackingRepo.GetAverageCostForDateAsync(itemId, DateTime.Today);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting current cost: {ex.Message}");
            CurrentCost = 0m;
        }
    }

    [RelayCommand]
    private async Task SelectionChanged(object selectedItem)
    {
        if (selectedItem is SearchItem selectedSearchItem)
        {
            // Optionally find and set _selectedItem
            SelectedItem = Items.FirstOrDefault(item => item.Id == selectedSearchItem.Id);
            if (SelectedItem is not null)
            {
                try
                {
                    await CalculateItemStatistics(SelectedItem.Id);
                }
                catch (Exception e)
                {
                    await AppShell.DisplayToastAsync($"Error: {e.Message}");
                }
            }
        }
    }

    [RelayCommand]
    private async Task ShowItemBuyDocs()
    {
        if (SelectedItem is not null)
        {
            await Shell.Current.GoToAsync($"itembuylist?itemid={SelectedItem.Id}");
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
          //  var result = await currentPage.ShowPopupAsync(barcodeScannerPopupPage, CancellationToken.None);

            // if (result is not null)
            // {
            //     try
            //     {
            //         SearchText = result.ToString();
            //
            //         var barcodeItem = Items.FirstOrDefault(item => item.Barcodes.Contains(SearchText));
            //         if (barcodeItem is not null)
            //         {
            //             SelectedItem = barcodeItem;
            //             CalculateItemStatistics(SelectedItem.Id);
            //         }
            //     }
            //     catch
            //     {
            //         Console.WriteLine();
            //     }
            // }
            // else
            // {
            //     Console.WriteLine("Null result or cancelled");
            // }
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