using Betting.Controller;
using SARGUI;
using SARModel;
using System.Windows.Controls;

namespace Betting.View
{
    /// <summary>
    /// Interaction logic for BetList.xaml
    /// </summary>
    public partial class BetList : Page, IView
    {
        public IAbstractController Controller { get; }

        public BetList()
        {
            InitializeComponent();
            Controller = (BetController)DataContext;
            Controller.SetUI(this);

        }
    }
}
