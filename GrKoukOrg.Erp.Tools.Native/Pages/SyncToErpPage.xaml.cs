using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class SyncToErpPage : ContentPage
{
    public SyncToErpPage(SyncToErpPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}