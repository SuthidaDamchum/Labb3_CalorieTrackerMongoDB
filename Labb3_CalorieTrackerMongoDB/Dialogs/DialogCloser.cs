using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Labb3_CalorieTrackerMongoDB.Dialogs
{
    class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
           DependencyProperty.RegisterAttached(
               "DialogResult",
               typeof(bool?),
               typeof(DialogCloser),
               new PropertyMetadata(null, OnDialogResultChanged));

        public static void SetDialogResult(Window target, bool? value) =>
            target.SetValue(DialogResultProperty, value);

        public static bool? GetDialogResult(Window target) =>
            (bool?)target.GetValue(DialogResultProperty);

        private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && e.NewValue is bool result)
            {
                window.DialogResult = result;
                window.Close();
            }
        }
    }
}
    