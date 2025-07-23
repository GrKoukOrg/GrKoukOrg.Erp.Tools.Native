using System.Globalization;

namespace GrKoukOrg.Erp.Tools.Native.Behaviors;

public class CurrencyBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        entry.Focused += OnEntryFocused;
        entry.Unfocused += OnEntryUnfocused;
        entry.Completed += OnEntryCompleted;
        entry.TextChanged += OnEntryTextChanged; 

        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.Focused -= OnEntryFocused;
        entry.Unfocused -= OnEntryUnfocused;
        entry.Completed -= OnEntryCompleted;
        entry.TextChanged -= OnEntryTextChanged;

        base.OnDetachingFrom(entry);
    }
    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            if (entry.IsFocused) // Only validate when focused (editing)
            {
                var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                string allowed = "0123456789" + sep;

                // Optionally: allow a minus sign at the start
                bool isNegative = e.NewTextValue.StartsWith("-");
                string working = isNegative ? e.NewTextValue.Substring(1) : e.NewTextValue;

                string filtered = new string(working.Where(c => allowed.Contains(c)).ToArray());

                // Only one decimal separator
                int firstSep = filtered.IndexOf(sep, StringComparison.Ordinal);
                if (firstSep >= 0)
                {
                    int nextSep = filtered.IndexOf(sep, firstSep + 1, StringComparison.Ordinal);
                    if (nextSep > 0)
                        filtered = filtered.Remove(nextSep, 1);
                }

                if (isNegative) filtered = "-" + filtered;

                if (entry.Text != filtered)
                    entry.Text = filtered;
            }
        }
    }


    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && !string.IsNullOrWhiteSpace(entry.Text))
        {
            // Remove currency formatting to show plain number
            if (decimal.TryParse(entry.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out var value))
            {
                // Remove any trailing .00 if integer
                var plainString = value % 1 == 0
                    ? ((int)value).ToString(CultureInfo.CurrentCulture)
                    : value.ToString("0.##", CultureInfo.CurrentCulture);
                entry.Text = plainString;

                // Select all text (override needed on main thread)
                Microsoft.Maui.Controls.Device.BeginInvokeOnMainThread(() =>
                {
                    entry.CursorPosition = 0;
                    entry.SelectionLength = entry.Text.Length;
                });
            }
        }
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        FormatEntry(sender as Entry);
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        FormatEntry(sender as Entry);
    }

    private void FormatEntry(Entry entry)
    {
        if (entry == null) return;

        if (decimal.TryParse(entry.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var value))
        {
            entry.Text = value.ToString("C2", CultureInfo.CurrentCulture);
        }
    }
}