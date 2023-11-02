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
    /// Interaction logic for AccountHolderForm.xaml
    /// </summary>
    public partial class AccountHolderForm : Window, IView
    {
        public IAbstractController Controller { get; }
        public AccountHolderForm()
        {
            InitializeComponent();
            Controller = this.GetController();
            Controller.AllowNewRecord(true);
        }

        public AccountHolderForm(IAbstractModel record) : this()=>
        Controller.OnAppearingGoTo(record);

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Controller.OnFormClosing(e);
        }
    }
}
