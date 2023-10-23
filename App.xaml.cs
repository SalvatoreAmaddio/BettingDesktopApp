using Betting.Model;
using SARModel;
using System;
using System.Windows;

namespace Betting
{
    public partial class App : Application
    {
        public App()
        {
            DatabaseManager.Load();
            try {
                DatabaseManager.AddDatabaseTable(
                       new SQLiteTable<AccountHolder>(),
                       new SQLiteTable<BookMakerAccount>(),
                       new SQLiteTable<AccountHolderBookMakerAccount>(),
                       new SQLiteTable<Gender>(),
                       new SQLiteTable<BetCode>(),
                       new SQLiteTable<BetPayment>(),
                       new SQLiteTable<Venue>(),
                       new SQLiteTable<Market>(),
                       new SQLiteTable<Runner>(),
                       new SQLiteTable<Bet>(),
                       new SQLiteTable<Promotion>()
                       );
            }
            catch ( Exception ex ) {}
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)=>
        SARGUI.View.ReportStartupExceptions(e);
    }
}
