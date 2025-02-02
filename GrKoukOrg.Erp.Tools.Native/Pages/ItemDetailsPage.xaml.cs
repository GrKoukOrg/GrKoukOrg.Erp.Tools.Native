using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class ItemDetailsPage : ContentPage
{
    public ItemDetailsPage(ItemDetailsPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }

    private void DropDownListBase_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }
}