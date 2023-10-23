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
using System.Windows.Shapes;

namespace Betting.View
{
    public partial class PromotionList : Window
    {
        IAbstractController Controller { get; }
        public PromotionList()
        {
            InitializeComponent();
            Controller = this.GetController();
            Controller.SetUI(this);
        }

        public PromotionList(IAbstractModel record) : this() 
        {
            Title = $"{record}'s Promotions";
            Controller.OnAppearingGoTo(record);
        } 
    }
}
