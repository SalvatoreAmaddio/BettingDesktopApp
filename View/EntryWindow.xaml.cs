using Betting.Controller;
using SARGUI.CustomGUI;
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
    /// Interaction logic for EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow : LoadingMask
    {
        public EntryWindow() => InitializeComponent();

        protected async override void OnClosing(CancelEventArgs e)
        {
            try
            {
                await Task.WhenAll(
                                    BookMakerAccountController.Calculate(),
                                    BetController.SetFilter()
                                    );
            }
            catch
            {
                MessageBox.Show("ERRORE ON CLOSING LOADING WINDOW");
            }
        }
    }
}