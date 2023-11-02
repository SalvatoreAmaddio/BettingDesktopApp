using SARGUI.CustomGUI;
using System.Windows;

namespace Betting.View
{
    public partial class BetView : Grid2
    {
        public BetView() => InitializeComponent();

        #region IsOnForm
        public static readonly DependencyProperty IsOnFormProperty = SARGUI.View.Binder.Register<bool, BetView>(nameof(IsOnForm), true, false, IsOnFormPropertyChanged, true, true, true);

        private static void IsOnFormPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BetView thisPanel = (BetView)d;
            bool isOnForm = (bool)e.NewValue;
            thisPanel.ColumnsDefinition2 = (isOnForm)
            ? "45,0,0,100,50,100,100,50,100,100,100,70,70,70,45,45"
            : "45,100,100,100,50,100,100,50,100,100,100,70,70,70,45,45";
        }

        public bool IsOnForm
        {
            get => (bool)GetValue(IsOnFormProperty);
            set => SetValue(IsOnFormProperty, value);
        }
        #endregion

    }
}
