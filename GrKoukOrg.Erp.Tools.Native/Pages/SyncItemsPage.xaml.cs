using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class SyncItemsPage : ContentPage
{
    public SyncItemsPage(SyncItemsPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}