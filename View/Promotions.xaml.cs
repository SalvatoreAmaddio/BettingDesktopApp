using Betting.Controller;
using SARGUI;
using SARModel;
using System.Windows.Controls;

namespace Betting.View
{
     public partial class Promotions : Page, IView
    {
        public IAbstractController Controller { get; }

        public Promotions()
        {
            InitializeComponent();
            Controller = this.GetController();
            Controller.SetUI(this);
            ((PromotionController)Controller).HideAgencyField = false;
        }

    }
}