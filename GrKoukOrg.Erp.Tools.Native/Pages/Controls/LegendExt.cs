using Syncfusion.Maui.Toolkit.Charts;

namespace GrKoukOrg.Erp.Tools.Native.Pages.Controls
{
    public class LegendExt : ChartLegend
    {
        protected override double GetMaximumSizeCoefficient()
        {
            return 0.5;
        }
    }
}
