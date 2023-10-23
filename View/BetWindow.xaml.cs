using Betting.Model;
using SARGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace Betting.View
{
    /// <summary>
    /// Interaction logic for BetWindow.xaml
    /// </summary>
    public partial class BetWindow : Window
    {
        private IAbstractController Controller { get; }
        public BetWindow()
        {
            InitializeComponent();
            Controller = this.GetController();
        }

        public BetWindow(IAbstractModel record) : this()
        {
            Title = $"Bets placed by {record}";
            Controller.OnAppearingGoTo(record);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Controller.OnFormClosing(e);
        }
    }
}
