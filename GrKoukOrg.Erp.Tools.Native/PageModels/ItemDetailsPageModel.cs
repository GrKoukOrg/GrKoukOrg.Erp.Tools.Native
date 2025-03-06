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
    private readonly LocalSaleDocumentsRepo _localSaleDocumentsRepo;
    private readonly LocalSalesDocLinesRepo _localSalesDocLinesRepo;
    [ObservableProperty] private ICollection<ItemListDto> _items;
    [ObservableProperty] private ItemListDto _selectedItem;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ICollection<SearchItem> _searchItems;
    [ObservableProperty] private ItemStatisticsDto _itemStatistics=new ItemStatisticsDto();
    public ItemDetailsPageModel(LocalItemsRepo localItemsRepo, LocalBuyDocumentsRepo localBuyDocumentsRepo,
        LocalBuyDocLinesRepo localBuyDocLinesRepo
        ,LocalSaleDocumentsRepo localSaleDocumentsRepo
        ,LocalSalesDocLinesRepo localSalesDocLinesRepo
        )
    {
        _localItemsRepo = localItemsRepo;
        _localBuyDocumentsRepo = localBuyDocumentsRepo;
        _localBuyDocLinesRepo = localBuyDocLinesRepo;
        _localSaleDocumentsRepo = localSaleDocumentsRepo;
        _localSalesDocLinesRepo = localSalesDocLinesRepo;
    }

    private async Task CalculateItemStatistics(int itemId)
    {
        var buyDocuments = await _localBuyDocumentsRepo.ListBuyDocsAsync();
        var buyDocLines = await _localBuyDocLinesRepo.ListBuyDocLinesAsync();
        var saleDocuments = await _localSaleDocumentsRepo.ListSaleDocsAsync();
        var saleDocLines = await _localSalesDocLinesRepo.ListSaleDocLinesAsync();
        var itemPurchasesData = buyDocuments
            .Join(
                buyDocLines,
                doc => doc.Id,
                line => line.BuyDocId,
                (doc, line) => new
                    { doc.BuyDocDefId, line.UnitQty, line.UnitTotalAmount, line.UnitDiscountAmount, line.ItemId }
            )
            .Where(x => x.ItemId == itemId);
        
        var itemSalesData = saleDocuments
            .Join(
                saleDocLines,
                doc => doc.Id,
                line => line.SaleDocId,
                (doc, line) => new
                    { doc.SaleDocDefId, line.UnitQty, line.UnitTotalAmount, line.UnitDiscountAmount, line.ItemId }
            )
            .Where(x => x.ItemId == itemId);
        
        var totalQuantityPurchased = itemPurchasesData
            .Sum(x =>
                x.BuyDocDefId == 9 // Purchase
                    ? x.UnitQty
                    : x.BuyDocDefId == 17 // Return
                        ? -x.UnitQty
                        : 0);
        var totalQuantitySold = itemSalesData
            .Sum(x =>
                x.SaleDocDefId == 54 // Sale
                    ? x.UnitQty
                    :  0);
        
        var totalPurchaseCost = itemPurchasesData
            .Sum(x =>
                x.BuyDocDefId == 9 // Purchase
                    ? x.UnitTotalAmount
                    : x.BuyDocDefId == 17 // Return
                        ? -x.UnitTotalAmount
                        : 0);
        var totalSaleIncome = itemSalesData
            .Sum(x =>
                x.SaleDocDefId == 54 // Sale
                    ? x.UnitTotalAmount
                    : 0);
       

        var totalDiscountCost = itemPurchasesData
            .Where(x => x.BuyDocDefId == 14) // Discounts only
            .Sum(x => x.UnitDiscountAmount);
       
        decimal meanPrice = totalQuantityPurchased > 0
            ? (totalPurchaseCost - totalDiscountCost) / totalQuantityPurchased
            : 0;

        decimal meanSalesPrice = totalQuantitySold > 0 ? totalSaleIncome / totalQuantitySold : 0;
            
        ItemStatistics = new ItemStatisticsDto()
        {
            ItemId = itemId,
            MeanPrice = meanPrice,
            TotalQuantityInWarehouse = totalQuantityPurchased,
            MeanSalesPrice = meanSalesPrice,
            TotalQuantitySold = totalQuantitySold,
        };
        
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
                    CalculateItemStatistics(SelectedItem.Id);
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