using SARGUI.CustomGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for BetFilterPanel.xaml
    /// </summary>
    public partial class BetFilterPanel : Grid2
    {
        public BetFilterPanel()
        {
            InitializeComponent();
        }

        private void OnToggleChecked(object sender, RoutedEventArgs e)=>
        ((ToggleButton)sender).Content = "OR";

        private void OnToggleUnchecked(object sender, RoutedEventArgs e) =>
        ((ToggleButton)sender).Content = "AND";
    }
}
