using Betting.Model;
using SARGUI;
using SARModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Betting.Controller
{
    public class BetController : AbstractDataController<Bet>
    {
        static AccountHolder? AccHold;
        static readonly Task RefreshAccountHoldersTask = new(() =>
        {
            IEnumerable<IAbstractModel> bets = DatabaseManager.GetDatabaseTable<Bet>().DataSource.Where(s => ((Bet)s).AccountHolder.IsEqualTo(AccHold));
            Parallel.ForEach(bets, (record) => ((Bet)record).BetAccountHolderFilter?.Requery());
            AccHold=null;
        });

        #region BackProps
        AccountHolderBookMakerAccount? AccHldBkAcc { get; set; }
        BookMakerOrder _bookMakerOrder;
        string _colsWidth = "45,100,100,100,50,100,100,50,100,100,100,70,70,70,45,45";
        string _wincolsWidth = "45,0,0,100,50,100,100,50,100,100,100,70,70,70,45,45";
        BetFiler _betFilter;
        #endregion

        #region RecordSources
        public List<byte> Races { get; } = new()
        {
            1,2,3,4,5,6,7,8,9,10
        };
        public RecordSource<BookMakerAccount> BookMakers { get; }
        public RecordSource<Venue> Venues { get; }
        public RecordSource<Market> Markets { get; }
        public RecordSource<BetCode> BetCodes { get; }
        public RecordSource<BetPayment> BetPayments { get; }
        public RecordSource<Runner> Runners { get; }
        #endregion

        #region Props
        public BookMakerOrder BookMakerOrder
        {
            get => _bookMakerOrder;
            set => Set(ref value, ref _bookMakerOrder);
        }
        public BetFiler BetFilter { get => _betFilter; set => Set(ref value, ref _betFilter); }
        public string ColumnsWidth { get => _colsWidth; set => Set(ref value, ref _colsWidth); }
        public string WinColumnsWidth { get => _wincolsWidth; set => Set(ref value, ref _wincolsWidth); }
        #endregion

        #region FiltersOptions
        public RecordSource<BookMakerAccount> BookMakersFilter { get; }
        public RecordSource<AccountHolder> AccountHolderFilter { get; }
        public RecordSource<Venue> VenueFilter { get; }
        public RecordSource<BetCode> BetCodeFilter { get; }
        public RecordSource<Runner> RunnerFilter { get; }
        public RecordSource<BetPayment> BetPaymentFilter { get; }
        public RecordSource<Market> MarketFilter { get; }

        BookMakerAccount _bookMakerAccount;
        public BookMakerAccount BookMakerAccount { get=>_bookMakerAccount; set=>Set(ref value, ref _bookMakerAccount); }
        #endregion

        public BetController()
        {
            AccountHolderFilter = new((IEnumerable<AccountHolder>)DatabaseManager.GetDatabaseTable<AccountHolder>().DataSource);
            DatabaseManager.AddChild<AccountHolder>(AccountHolderFilter);

            BookMakersFilter = new((IEnumerable<BookMakerAccount>)DatabaseManager.GetDatabaseTable<BookMakerAccount>().DataSource);
            DatabaseManager.AddChild<BookMakerAccount>(BookMakersFilter);

            BookMakers = new((IEnumerable<BookMakerAccount>)DatabaseManager.GetDatabaseTable<BookMakerAccount>().DataSource);
            DatabaseManager.AddChild<BookMakerAccount>(BookMakers);
            _bookMakerOrder = new(BookMakers.SourceID);

            Venues = new((IEnumerable<Venue>)DatabaseManager.GetDatabaseTable<Venue>().DataSource);
            DatabaseManager.AddChild<Venue>(Venues);

            VenueFilter = new((IEnumerable<Venue>)DatabaseManager.GetDatabaseTable<Venue>().DataSource);
            DatabaseManager.AddChild<Venue>(VenueFilter);

            Markets = new((IEnumerable<Market>)DatabaseManager.GetDatabaseTable<Market>().DataSource);
            DatabaseManager.AddChild<Market>(Markets);

            MarketFilter = new((IEnumerable<Market>)DatabaseManager.GetDatabaseTable<Market>().DataSource);
            DatabaseManager.AddChild<Market>(MarketFilter);

            BetCodes = new((IEnumerable<BetCode>)DatabaseManager.GetDatabaseTable<BetCode>().DataSource);
            DatabaseManager.AddChild<BetCode>(BetCodes);

            BetCodeFilter = new((IEnumerable<BetCode>)DatabaseManager.GetDatabaseTable<BetCode>().DataSource);
            DatabaseManager.AddChild<BetCode>(BetCodeFilter);

            BetPayments = new((IEnumerable<BetPayment>)DatabaseManager.GetDatabaseTable<BetPayment>().DataSource);
            DatabaseManager.AddChild<BetPayment>(BetPayments);

            BetPaymentFilter = new((IEnumerable<BetPayment>)DatabaseManager.GetDatabaseTable<BetPayment>().DataSource);
            DatabaseManager.AddChild<BetPayment>(BetPaymentFilter);

            Runners = new((IEnumerable<Runner>)DatabaseManager.GetDatabaseTable<Runner>().DataSource);
            DatabaseManager.AddChild<Runner>(Runners);

            RunnerFilter = new((IEnumerable<Runner>)DatabaseManager.GetDatabaseTable<Runner>().DataSource);
            DatabaseManager.AddChild<Runner>(RunnerFilter);

            _betFilter = new(ChildSource.SourceID);
        }

        public override void OpenNewRecord(IAbstractModel record)
        {
            ChildSource.GoNewRecord();
            Bet bet = (Bet)ChildSource.Last();
            bet.BetAccountHolderFilter = new BetAccountHolderFilter();
            if (AccHldBkAcc!=null) bet.SetAccountsDetails(AccHldBkAcc);
        }

        #region Tasks
        public static void RunRefreshAccountHoldersTask(AccountHolder accHold)
        {
            AccHold = accHold;
            RefreshAccountHoldersTask.Start();
        }

        public static async void RunRefreshOnBookMakerChangedTask(AccountHolderBookMakerAccount accHldrBkMkAcc) 
        {            
            if (AccHold != null)
                await RefreshAccountHoldersTask;

         await RefreshOnBookMakerChangedAsync(accHldrBkMkAcc);
        }

        private static Task RefreshOnBookMakerChangedAsync(AccountHolderBookMakerAccount accHldrBkMkAcc)
        {
            IRecordSource bets = DatabaseManager.GetDatabaseTable<Bet>().DataSource;
            Parallel.ForEach(bets, (record) =>
            {
                Bet bet = (Bet)record;
                bet?.ReplaceBookMakerIF(accHldrBkMkAcc);
                bet?.BetAccountHolderFilter?.Requery();
                bet?.RefreshPromo();
            });

            BookMakerAccountController.Calculate();
            return Task.CompletedTask;
        }

        public static Task SetFilter() 
        {
            Parallel.ForEach(DatabaseManager.GetDatabaseTable<Bet>().DataSource, (record) => ((Bet)record)._betAccountHolderFilter = new BetAccountHolderFilter());
            return Task.CompletedTask;
        } 
        #endregion

        public override bool Save(IAbstractModel? record)
        {
            if (record == null) return false;
            Bet bet = (Bet)record;
            if (AccHldBkAcc == null)
            {
                bet.AccountHolderBookMakerAccount
                =
                (AccountHolderBookMakerAccount)DatabaseManager
                .GetDatabaseTable<AccountHolderBookMakerAccount>()
                .DataSource
                .First(s =>
                            ((AccountHolderBookMakerAccount)s).BookMakerAccount.IsEqualTo(bet.BookMakerAccount)
                            && ((AccountHolderBookMakerAccount)s).AccountHolder.IsEqualTo(bet.AccountHolder)
                            );
            }

            bool result = base.Save(bet);
            if (result) BookMakerAccountController.Calculate(bet.BookMakerAccount);
            return result;
        }

        public override bool Delete(IAbstractModel? record)
        {
            BookMakerAccount? bkAcc = ((Bet?)record)?.BookMakerAccount;
            bool result = base.Delete(record);
            if (result && bkAcc!=null) BookMakerAccountController.Calculate(bkAcc);            
            return result;
        }

        public override void OnAppearingGoTo(IAbstractModel record)
        {
            if (record is AccountHolderBookMakerAccount accHldBkAcc)
            {
                AccHldBkAcc=accHldBkAcc;
                var range = MainSource.Where(s => ((Bet)s).AccountHolderBookMakerAccount.IsEqualTo(record)).ToList();
                ChildSource.ReplaceData(range);
                base.OnAppearingGoTo((ChildSource.RecordCount > 0) ? ChildSource.First() : new Bet(AccHldBkAcc));
                return;
            }
            base.OnAppearingGoTo(record);
        }

        public override bool OnFormClosing(CancelEventArgs e)
        {
            bool outcome=base.OnFormClosing(e);
            if (outcome) 
            {
                AccHldBkAcc = null;
            }
            return outcome;
        }
    }

    public class BetFiler : AbstractRecordsOrganizer
    {
        public BetFiler(long id) : base(id)
        {
        }

        protected override IRecordSource OriginalSource => DatabaseManager.GetDatabaseTable<Bet>().DataSource;

        public override bool FilterCriteria(IAbstractModel model)=>true;

        public override void OnReorder(IRecordSource FilteredSource)=>
        FilteredSource.ReplaceData(FilteredSource.OrderByDescending(s => ((Bet)s).DateOfBet).ThenBy(s=>((Bet)s).TimeOfBet));
    }
    public class BetAccountHolderFilter : AbstractRecordsOrganizer
    {
        public BetAccountHolderFilter()
        {
        }

        public BetAccountHolderFilter(long id) : base(id)
        {
        }

        protected override IRecordSource OriginalSource => DatabaseManager.GetDatabaseTable<AccountHolder>().DataSource;
        IRecordSource BookMakersAccountHolders => DatabaseManager.GetDatabaseTable<AccountHolderBookMakerAccount>().DataSource;
        public override bool FilterCriteria(IAbstractModel model)
        {
            BookMakerAccount? dataContext = GetDataContext<BookMakerAccount>();
            var results=BookMakersAccountHolders.Where(s=>((AccountHolderBookMakerAccount)s).BookMakerAccount.IsEqualTo(dataContext)).ToList();
            return results.Any(s=>((AccountHolderBookMakerAccount)s).AccountHolder.IsEqualTo(model));
        }

        public override void OnReorder(IRecordSource FilteredSource)=>
        FilteredSource.ReplaceData(FilteredSource.OrderBy(s=>s.ToString()));
    }
}
