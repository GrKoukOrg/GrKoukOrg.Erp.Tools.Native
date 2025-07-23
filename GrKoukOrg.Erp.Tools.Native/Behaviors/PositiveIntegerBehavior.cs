using Microsoft.Maui.Controls;

namespace GrKoukOrg.Erp.Tools.Native.Behaviors
{
    public class PositiveIntegerBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnEntryTextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= OnEntryTextChanged;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.IsFocused)
            {
                string filtered = new string(e.NewTextValue
                    .Where(char.IsDigit)
                    .ToArray());

                // Prevent leading zeros if you want:
                // filtered = filtered.TrimStart('0');
                // if (string.IsNullOrEmpty(filtered)) filtered = "0";
                
                if (entry.Text != filtered)
                    entry.Text = filtered;
            }
        }
    }
}