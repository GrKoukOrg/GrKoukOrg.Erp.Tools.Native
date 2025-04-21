using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErpCashDiaryListPageModel = GrKoukOrg.Erp.Tools.Native.PageModels.ErpCashDiaryListPageModel;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class ErpCashDiaryListPage : ContentPage
{
    public ErpCashDiaryListPage(ErpCashDiaryListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}