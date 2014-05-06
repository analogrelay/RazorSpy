using System.ComponentModel.Composition;
using System.Windows;
using RazorSpy.ViewModel;

namespace RazorSpy
{
    internal static class UIExtensions
    {
        public static void RegisterWithContainer(this FrameworkElement self)
        {
            try
            {
                App.Container.ComposeParts(self);
            }
            catch (CompositionException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
