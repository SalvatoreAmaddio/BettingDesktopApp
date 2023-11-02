using Betting.Controller;
using SARGUI;
using SARModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Betting.View
{
    public partial class AccountHolderList : Page, IView
    {
        public IAbstractController Controller { get; }
        public AccountHolderList()
        {
            InitializeComponent();
            Controller = this.GetController();
            Controller.SetUI(this);
        }
    }
}
