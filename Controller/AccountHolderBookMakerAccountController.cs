using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Betting.Controller
{
    public class AccountHolderBookMakerAccountController : AbstractDataController<AccountHolderBookMakerAccount>
    {
        BookMakerOrder _bookMakerOrder;
        AccountHolder? accountHolder;

        public BookMakerOrder BookMakerOrder
        {
            get => _bookMakerOrder;
            set => Set(ref value, ref _bookMakerOrder);
        }
        public RecordSource<BookMakerAccount> Bookmakers { get; }

        public AccountHolderBookMakerAccountController()
        {
            Bookmakers = new((IEnumerable<BookMakerAccount>)DatabaseManager.GetDatabaseTable<BookMakerAccount>().DataSource);
            DatabaseManager.AddChild<BookMakerAccount>(Bookmakers);
            _bookMakerOrder = new(Bookmakers.SourceID);
        }

        public override void OpenRecord(IAbstractModel? record)
        {
           if (record == null || record.IsNewRecord) return;
           BetWindow betWindow = new(record);
           betWindow.ShowDialog();
        }

        public override void OpenNewRecord(IAbstractModel record)
        {
            if (accountHolder == null) return;
            ChildSource.GoNewRecord();
        }

        public override void OnAppearingGoTo(IAbstractModel? record)
        {
            if (record != null && record is AccountHolder accHld)
            {
                accountHolder = accHld;
                var filteredRange = MainSource.Where<AccountHolderBookMakerAccount>(s=>s.AccountHolder.IsEqualTo(record),false).ToList();
                ChildSource.ReplaceData(filteredRange);

                switch (accountHolder.IsNewRecord) 
                {
                    case true:
                    AllowNewRecord(false);
                    return;
                    case false:
                    AllowNewRecord(true);
                    break;
                }

                IAbstractModel nextRecord = (ChildSource.RecordCount > 0) ? ChildSource.First() : new AccountHolderBookMakerAccount();
                base.OnAppearingGoTo(nextRecord);
                return;
            }

            base.OnAppearingGoTo(record);
        }

        public override bool Save(IAbstractModel? record)
        {
            if (accountHolder == null || record==null) return false;
            AccountHolderBookMakerAccount AccHldrBkMkrAcc = (AccountHolderBookMakerAccount)record;
            AccHldrBkMkrAcc.AccountHolder=accountHolder;
            bool result = base.Save(AccHldrBkMkrAcc);
            if (result) BetController.RunRefreshOnBookMakerChangedTask(AccHldrBkMkrAcc);
            return result;
        }

        public override bool Delete(IAbstractModel? record)
        {
            if (record == null) return false;

            Task RefreshOnBookMakerChanged = new(() =>
            {
                AccountHolderBookMakerAccount AccHldrBkMkrAcc = (AccountHolderBookMakerAccount)record;
                BetController.RunRefreshOnBookMakerChangedTask(AccHldrBkMkrAcc);
            });

            bool result = base.Delete(record);
            RefreshOnBookMakerChanged.Start();
            return result;
        }
    }
}
