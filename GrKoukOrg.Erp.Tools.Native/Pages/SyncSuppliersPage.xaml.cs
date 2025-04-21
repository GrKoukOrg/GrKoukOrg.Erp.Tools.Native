using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class SyncSuppliersPage : ContentPage
{
    public SyncSuppliersPage(SyncSuppliersPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}