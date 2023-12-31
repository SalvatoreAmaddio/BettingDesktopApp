﻿using SARGUI.CustomGUI;
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
    public partial class BetFilterPanel : FormHeader
    {
        public BetFilterPanel()
        {
            InitializeComponent();
        }


        #region IsOnForm
        public static readonly DependencyProperty IsOnFormProperty = SARGUI.View.Binder.Register<bool, BetFilterPanel>(nameof(IsOnForm), true, false, IsOnFormPropertyChanged, true, true, true);

        private static void IsOnFormPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BetFilterPanel thisPanel = (BetFilterPanel)d;
            bool isOnForm = (bool)e.NewValue;
            thisPanel.OptionGrid.ColumnsDefinition2 = (isOnForm) 
            ? "0,0,101,40,120,25,120,10,110,10,110,10,110,10,110,10,110"
            : "140,140,101,40,120,25,120,10,100,10,100,10,100,10,100,10,100";
        }

        public bool IsOnForm
        {
            get => (bool)GetValue(IsOnFormProperty);
            set => SetValue(IsOnFormProperty, value);
        }
        #endregion
    }
}
