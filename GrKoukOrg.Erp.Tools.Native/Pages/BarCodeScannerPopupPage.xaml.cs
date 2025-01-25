using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class BarCodeScannerPopupPage 
{
    public BarCodeScannerPopupPage()
    {
        InitializeComponent();
    }
    private void scanner_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        scanner.IsDetecting = false;

        Close(e.Results[0].Value);
    }
}