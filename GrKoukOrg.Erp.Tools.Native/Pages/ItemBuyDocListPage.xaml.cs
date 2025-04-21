using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class ItemBuyDocListPage : ContentPage
{
    public ItemBuyDocListPage(ItemBuyDocListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}