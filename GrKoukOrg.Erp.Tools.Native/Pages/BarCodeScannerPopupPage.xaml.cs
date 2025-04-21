using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.Net.Maui;

namespace GrKoukOrg.Erp.Tools.Native.Pages;

public partial class BarCodeScannerPopupPage 
{
    public BarCodeScannerPopupPage()
    {
        InitializeComponent();
        scanner.Options = new BarcodeReaderOptions()
        {
TryHarder = true,
AutoRotate = true,
//Formats = BarcodeFormat.Code39
        };
    }
    private void scanner_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        scanner.IsDetecting = false;

        Close(e.Results[0].Value);
    }
}