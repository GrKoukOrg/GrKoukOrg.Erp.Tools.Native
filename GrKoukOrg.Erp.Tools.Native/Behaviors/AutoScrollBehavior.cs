using Syncfusion.Maui.DataGrid;

namespace GrKoukOrg.Erp.Tools.Native.Behaviors;

public class AutoScrollBehavior: Behavior<SfDataGrid>
{
    public static readonly BindableProperty ScrollToIndexProperty =
        BindableProperty.Create(nameof(ScrollToIndex), typeof(int), typeof(AutoScrollBehavior), -1, propertyChanged: OnScrollToIndexChanged);

    public int ScrollToIndex
    {
        get => (int)GetValue(ScrollToIndexProperty);
        set => SetValue(ScrollToIndexProperty, value);
    }

    private SfDataGrid AssociatedDataGrid;

    protected override void OnAttachedTo(SfDataGrid bindable)
    {
        base.OnAttachedTo(bindable);
        AssociatedDataGrid = bindable;
    }

    protected override void OnDetachingFrom(SfDataGrid bindable)
    {
        base.OnDetachingFrom(bindable);
        AssociatedDataGrid = null;
    }

    private static void OnScrollToIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var behavior = (AutoScrollBehavior)bindable;

        if (behavior.AssociatedDataGrid != null && behavior.ScrollToIndex >= 0)
        {
            behavior.AssociatedDataGrid.ScrollToRowIndex(behavior.ScrollToIndex,ScrollToPosition.End,true); // Scroll to the specified index
        }
    }

}