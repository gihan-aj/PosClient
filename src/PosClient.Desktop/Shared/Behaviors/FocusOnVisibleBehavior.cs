using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace PosClient.Desktop.Shared.Behaviors
{
    public class FocusOnVisibleBehavior : Behavior<Control>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged += OnIsVisibleChanged;
            AssociatedObject.IsEnabledChanged += OnIsEnabledChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.IsVisibleChanged -= OnIsVisibleChanged;
            AssociatedObject.IsEnabledChanged -= OnIsEnabledChanged;
        }

        private void OnIsEnabledChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                AssociatedObject.Focus();
            }
        }

        private void OnIsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                AssociatedObject.Focus();
            }
        }
    }
}
