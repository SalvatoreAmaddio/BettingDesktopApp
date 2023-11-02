using SARGUI.CustomGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Betting.View
{
    /// <summary>
    /// Interaction logic for PromotionFilterPanel.xaml
    /// </summary>
    public partial class PromotionFilterPanel : FormHeader
    {
        public PromotionFilterPanel()
        {
            InitializeComponent();
        }

        #region IsOnForm
        public static readonly DependencyProperty IsOnFormProperty = SARGUI.View.Binder.Register<bool, PromotionFilterPanel>(nameof(IsOnForm), true, false, IsOnFormPropertyChanged, true, true, true);

        private static void IsOnFormPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PromotionFilterPanel thisPanel = (PromotionFilterPanel)d;
            bool isOnForm = (bool)e.NewValue;
            thisPanel.OptionGrid.ColumnsDefinition2 = (isOnForm)
            ? "0,40,120,25,120,65,65,65,65,65,65,65,65,65,71"
            : "140,40,120,25,120,65,65,65,65,65,65,65,65,65,71";
        }

        public bool IsOnForm
        {
            get => (bool)GetValue(IsOnFormProperty);
            set => SetValue(IsOnFormProperty, value);
        }
        #endregion

    }
}
