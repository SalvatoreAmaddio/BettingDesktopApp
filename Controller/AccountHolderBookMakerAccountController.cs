using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
using System.Linq;
using System.Threading.Tasks;

namespace Betting.Controller
{
    public class AccountHolderBookMakerAccountController : AbstractDataController<AccountHolderBookMakerAccount>
    {
        AccountHolder? CurrentAccountHolder;

        public RecordSource<BookMakerAccount> Bookmakers { get; }

        public AccountHolderBookMakerAccountController()
        {
            Bookmakers = IRecordSource.InitSource<BookMakerAccount>();
            Bookmakers.SetFilter(new BookMakerOrder());
        }

        public override void OpenRecord(IAbstractModel? record)
        {
           if (record == null || record.IsNewRecord) return;
           BetWindow betWindow = new(record);
           betWindow.ShowDialog();
        }

        public override void OpenNewRecord(IAbstractModel? record)
        {
            if (CurrentAccountHolder == null) return;
            ChildSource.GoNewRecord();
        }

        public override void OnAppearingGoTo(IAbstractModel? record)
        {
            if (record != null && record is AccountHolder accHld)
            {
                CurrentAccountHolder = accHld;
                var filteredRange = MainSource.Where<AccountHolderBookMakerAccount>(s=>s.AccountHolder.IsEqualTo(record),false).ToList();
                ChildSource.ReplaceData(filteredRange);

                switch (CurrentAccountHolder.IsNewRecord) 
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
            if (CurrentAccountHolder == null || record==null) return false;
            AccountHolderBookMakerAccount AccHldrBkMkrAcc = (AccountHolderBookMakerAccount)record;
            AccHldrBkMkrAcc.AccountHolder=CurrentAccountHolder;
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
