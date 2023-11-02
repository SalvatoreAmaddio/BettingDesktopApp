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
    public partial class PromotionForm : Window, IView
    {
        public IAbstractController Controller { get; }

        public PromotionForm()
        {
            InitializeComponent();
            Controller = this.GetController();
            Controller.SetUI(this);
            Controller.AllowNewRecord(true);
        }

        public PromotionForm(IAbstractModel record, bool HideAgencyField = false) : this()
        {
            if (HideAgencyField) GridContent.RowDefinition2 = "0,30,30,30,30,150";
            Title = (!HideAgencyField && record.IsNewRecord) ? "New Promotion" : $"{record}'s Promotions";
            Controller.OnAppearingGoTo(record);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Controller.OnFormClosing(e);
        }
    }
}
