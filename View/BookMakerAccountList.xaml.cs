using Betting.Controller;
using SARGUI;
using SARModel;
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
    /// Interaction logic for BookMakerAccountList.xaml
    /// </summary>
    public partial class BookMakerAccountList : Page, IView
    {
        public IAbstractController Controller { get; }
        public BookMakerAccountList()
        {
            InitializeComponent();
            Controller = (BookMakerAccountController)DataContext;
            Controller.SetUI(this);
        }

    }
}
