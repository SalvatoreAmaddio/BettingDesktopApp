using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Betting.Controller
{
    public class BetController : AbstractDataController<Bet>
    {
        static AccountHolder? AccHld;
        static readonly Task RefreshAccountHoldersTask = new(() =>
        {
            IEnumerable<IAbstractModel> bets = DatabaseManager.GetDatabaseTable<Bet>().DataSource.Where(s => ((Bet)s).AccountHolder.IsEqualTo(AccHld));
            Parallel.ForEach(bets, (record) => ((Bet)record).BetAccountHolderFilter?.Requery());
            AccHld=null;
        });

        #region BackProps
        private AccountHolderBookMakerAccount? AccHldBkMkAcc { get; set; }
        private string _listViewColsWidth = "45,100,100,100,50,100,100,50,100,100,100,70,70,70,45,45";
        private string _winViewColsWidth = "45,0,0,100,50,100,100,50,100,100,100,70,70,70,45,45";
        private BetFilterManager _betFilterManager = new();
        #endregion

        #region RecordSources
        public List<byte> Races { get; } = new()
        {
            1,2,3,4,5,6,7,8,9,10
        };
        public RecordSource<AccountHolder> AccountHolders { get; }

        public RecordSource<BookMakerAccount> BookMakers { get; }
        public RecordSource<Venue> Venues { get; }
        public RecordSource<Market> Markets { get; }
        public RecordSource<BetCode> BetCodes { get; }
        public RecordSource<BetPayment> BetPayments { get; }
        public RecordSource<Runner> Runners { get; }
        #endregion

        #region Props
        public string ListViewColsWidth { get => _listViewColsWidth; set => Set(ref value, ref _listViewColsWidth); }
        public string WinViewColsWidth { get => _winViewColsWidth; set => Set(ref value, ref _winViewColsWidth); }
        public BetFilterManager BetFilterManager { get => _betFilterManager; set => Set(ref value, ref _betFilterManager); }
        #endregion

        public BetController()
        {
            AccountHolders = IRecordSource.InitSource<AccountHolder>();
            AccountHolders.SetFilter(new BetAccountHolderFilter());

            BookMakers = IRecordSource.InitSource<BookMakerAccount>();
            BookMakers.SetFilter(new BookMakerOrder());

            Venues = IRecordSource.InitSource<Venue>();

            Markets = IRecordSource.InitSource<Market>();

            BetCodes = IRecordSource.InitSource<BetCode>();

            BetPayments = IRecordSource.InitSource<BetPayment>();

            Runners = IRecordSource.InitSource<Runner>();

            ChildSource.SetFilter(new BetFilter());
            _betFilterManager.Run += BetFilterManager_Run;
        }

        private void BetFilterManager_Run(object? sender, EventArgs e)
        {
            ChildSource?.Filter?.SetDataContext(_betFilterManager);
            ChildSource?.Filter?.Run();
        }

        #region Tasks
        public static void RunRefreshAccountHoldersTask(AccountHolder accHold)
        {
            AccHld = accHold;
            RefreshAccountHoldersTask.Start();
        }

        public static async void RunRefreshOnBookMakerChangedTask(AccountHolderBookMakerAccount accHldrBkMkAcc) 
        {            
            if (AccHld != null)
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

        #region CRUD
        public override bool Save(IAbstractModel? record)
        {
            if (record == null) return false;
            Bet bet = (Bet)record;
            if (AccHldBkMkAcc == null)
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
        #endregion
        public override void OnAppearingGoTo(IAbstractModel record)
        {
            if (record is AccountHolderBookMakerAccount accHldBkAcc)
            {
                AccHldBkMkAcc=accHldBkAcc;
                var range = MainSource.Where(s => ((Bet)s).AccountHolderBookMakerAccount.IsEqualTo(record)).ToList();
                ChildSource.ReplaceData(range);
                _betFilterManager.SetBookMakerAccountHolder(accHldBkAcc);
                base.OnAppearingGoTo((ChildSource.RecordCount > 0) ? ChildSource.First() : new Bet(AccHldBkMkAcc));
                return;
            }
            base.OnAppearingGoTo(record);
        }
        public override void OpenNewRecord(IAbstractModel record)
        {
            ChildSource.GoNewRecord();
            Bet bet = (Bet)ChildSource.Last();
            bet.BetAccountHolderFilter = new BetAccountHolderFilter();
            if (AccHldBkMkAcc != null) bet.SetAccountsDetails(AccHldBkMkAcc);
        }
        public override bool OnFormClosing(CancelEventArgs e)
        {
            bool outcome=base.OnFormClosing(e);
            if (outcome) 
            {
                AccHldBkMkAcc = null;
            }
            return outcome;
        }

        public override Task<bool> WriteExcel() =>
        WriteExcel(OfficeFileMode.WRITE, Path.Combine(Sys.DesktopPath, "BetReport.xlsx"),
            (excel) =>
            {
                excel.Range.Style("D1", new("dd/MM/yyyy", Styles.NumberFormat));
                excel.Range.Style("E1", new("hh:mm", Styles.NumberFormat));
                excel.Range.Style("L1:N1", new("£#,##0;[Red]-£#,##0", Styles.NumberFormat));
                excel.Range.Style("L1:N1", new(11.45, Styles.ColumnWidth));
                excel.Range.Style("A1", new(14, Styles.ColumnWidth));
                excel.Range.Style("D1", new(12, Styles.ColumnWidth));
                excel.Range.Style("F1:H1", new(12, Styles.ColumnWidth));
                excel.Range.Style("E1", new(10, Styles.ColumnWidth));
                excel.Range.Style("K1", new(14, Styles.ColumnWidth));
            });
        
        public override Task<object?[,]> OrganiseExcelData()
        {
            object?[,] data = GenerateDataTable("IS PROMO", "BOOKMAKER", "ACCOUNT HOLDER", "DATE", "TIME", "VENUE",
           "BET CODE", "RACE", "RUNNER", "BET PAYMENT", "MARKET", "ODD", "WAGER","PROFIT");

            Parallel.For(0, ChildSource.RecordCount, (row) =>
            {
                Bet record = (Bet)ChildSource.Get(row);
                data[row + 1, 0] = record.IsPromo;
                data[row + 1, 1] = $"{record.BookMakerAccount}";
                data[row + 1, 2] = $"{record.AccountHolder}";
                data[row + 1, 3] = $"'{record.DateOfBet:d}";
                data[row + 1, 4] = $"{record.TimeOfBet:t}";
                data[row + 1, 5] = $"{record.Venue}";
                data[row + 1, 6] = $"{record.BetCode}";
                data[row + 1, 7] = record.RaceNo;
                data[row + 1, 8] = $"{record.Runner}";
                data[row + 1, 9] = $"{record.BetPayment}";
                data[row + 1, 10] = $"{record.Market}";
                data[row + 1, 11] = record.Odd;
                data[row + 1, 12] = record.Wager;
                data[row + 1, 13] = record.Profit;
            });
            return Task.FromResult(data);
        }
    }

    #region Filters
    public class BetFilter : AbstractRecordsOrganizer<Bet>
    {
        public override bool FilterCriteria(IAbstractModel model) 
        {
            BetFilterManager? dataContext = GetDataContext<BetFilterManager>();
            if (dataContext == null) return true;
            return dataContext.CheckRecord((Bet)model);
        }

        public override void OnReorder(IRecordSource FilteredSource)=>
        FilteredSource.ReplaceData(FilteredSource.OrderByDescending(s => ((Bet)s).DateOfBet).ThenBy(s=>((Bet)s).TimeOfBet));
    }
    public class BetAccountHolderFilter : AbstractRecordsOrganizer<AccountHolder>
    {
        private static IRecordSource BookMakersAccountHolders => DatabaseManager.GetDatabaseTable<AccountHolderBookMakerAccount>().DataSource;
 
        public override bool FilterCriteria(IAbstractModel model)
        {
            BookMakerAccount? dataContext = GetDataContext<BookMakerAccount>();
            var results= BookMakersAccountHolders.Where(s=>((AccountHolderBookMakerAccount)s).BookMakerAccount.IsEqualTo(dataContext)).ToList();
            return results.Any(s=>((AccountHolderBookMakerAccount)s).AccountHolder.IsEqualTo(model));
        }

        public override void OnReorder(IRecordSource FilteredSource)=>
        FilteredSource.ReplaceData(FilteredSource.OrderBy(s=>s.ToString()));
    }
    public class BookmakerFilter : AbstractRecordsOrganizer<BookMakerAccount>
    {
        private static IRecordSource BookMakersAccountHolders => DatabaseManager.GetDatabaseTable<AccountHolderBookMakerAccount>().DataSource;

        public override bool FilterCriteria(IAbstractModel record)
        {
            AccountHolder? dataContext = GetDataContext<AccountHolder>();
            if (dataContext == null || dataContext.IsNewRecord) return true;
            var results = BookMakersAccountHolders.Where(s => ((AccountHolderBookMakerAccount)s).AccountHolder.IsEqualTo(dataContext)).ToList();
            return results.Any(s => ((AccountHolderBookMakerAccount)s).BookMakerAccount.IsEqualTo(record));
        }

    }
    public abstract class BaseFilter<M> : AbstractRecordsOrganizer<M> where M : AbstractTableModel<M>, new()
    {
        protected IRecordSource Bets => DatabaseManager.GetDatabaseTable<Bet>().DataSource;

        protected IEnumerable<IAbstractModel>? DoJob() 
        {
            IAbstractModel? dataContext = GetDataContext<IAbstractModel>();
            if (dataContext == null || dataContext.IsNewRecord) return null;

            IEnumerable<IAbstractModel> results;

            return (dataContext is AccountHolder)
            ? results = Bets.Where(s => ((Bet)s).AccountHolder.IsEqualTo(dataContext)).ToList()
            : results = Bets.Where(s => ((Bet)s).BookMakerAccount.IsEqualTo(dataContext)).ToList();
        }
    }
    public class VenueFilter : BaseFilter<Venue>
    {
        public override bool FilterCriteria(IAbstractModel record)
        {
            IEnumerable<IAbstractModel>? results = DoJob();
            return (results == null) || results.Any(s => ((Bet)s).Venue.IsEqualTo(record));
        }
    }
    public class BetCodeFilter : BaseFilter<BetCode>
    {
        public override bool FilterCriteria(IAbstractModel record)
        {
            IEnumerable<IAbstractModel>? results = DoJob();
            return (results == null) || results.Any(s => ((Bet)s).BetCode.IsEqualTo(record));
        }
    }
    public class RunnerFilter : BaseFilter<Runner>
    {
        public override bool FilterCriteria(IAbstractModel record)
        {
            IEnumerable<IAbstractModel>? results = DoJob();
            if (results == null) return true;
            return results.Any(s => ((Bet)s).Runner.IsEqualTo(record));
        }
    }
    public class BetPaymentFilter : BaseFilter<BetPayment>
    {
        public override bool FilterCriteria(IAbstractModel record)
        {
            IEnumerable<IAbstractModel>? results = DoJob();
            return (results == null) || results.Any(s => ((Bet)s).BetPayment.IsEqualTo(record));
        }
    }
    public class MarketFilter : BaseFilter<Market>
    {
        public override bool FilterCriteria(IAbstractModel record)
        {
            IEnumerable<IAbstractModel>? results = DoJob();
            return (results == null) || results.Any(s => ((Bet)s).Market.IsEqualTo(record));
        }
    }

    public abstract class AbstractFilterManager<M> : AbstractNotifier
    {
        public event EventHandler? Run;
        private DateTime? _fromDate;
        private DateTime? _toDate;
        public DateTime? FromDate { get => _fromDate; set => Set(ref value, ref _fromDate); }
        public DateTime? ToDate { get => _toDate; set => Set(ref value, ref _toDate); }

        public AbstractFilterManager() 
        {
            AfterUpdate += OnAfterUpdate;
        }
        
        protected virtual void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            Run?.Invoke(this, EventArgs.Empty);
        }

        protected bool CheckDates(DateTime? date)
        {
            bool cond1 = true;
            bool cond2 = true;
            if (FromDate != null)
                cond1 = FromDate <= date;

            if (ToDate != null)
                cond2 = ToDate >= date;
            return cond1 && cond2;
        }

        public abstract bool CheckRecord(M record);
        public abstract bool CheckBools(bool value);
        protected static bool PropCheck<T>(ref T? _backProp, T prop)
        {
            if (_backProp == null) return true;
            Type type = typeof(M);
            if (type == typeof(bool))
                return true;
            if (_backProp is IAbstractModel model)
                if (model.IsNewRecord)
                    return true;
            return _backProp.Equals(prop);
        }
    }

    public class BetFilterManager : AbstractFilterManager<Bet>
    {

        #region RecordSources
        public RecordSource<BookMakerAccount> BookMakers { get; }
        public RecordSource<AccountHolder> AccountHolders { get; }
        public RecordSource<Venue> Venues { get; }
        public RecordSource<BetCode> BetCodes { get; }
        public RecordSource<Runner> Runners { get; }
        public RecordSource<BetPayment> BetPayments { get; }
        public RecordSource<Market> Markets { get; }
        #endregion

        #region backprops
        private bool _ispromo = false;
        private bool _isnopromo = false;
        private BookMakerAccount? _bookMakerAccount;
        private AccountHolder? _accountHolder;
        private Venue? _venue;
        private BetCode? _betCode;
        private Runner? _runner;
        private BetPayment? _betPayment;
        private Market? _market;
        private IAbstractModel? _model;
        #endregion

        #region Props
        public bool IsPromo { get => _ispromo; set => Set(ref value, ref _ispromo); }
        public bool IsNoPromo { get => _isnopromo; set => Set(ref value, ref _isnopromo); }
        public AccountHolder? AccountHolder { get => _accountHolder; set => Set(ref value, ref _accountHolder); }
        public BookMakerAccount? BookMakerAccount { get => _bookMakerAccount; set => Set(ref value, ref _bookMakerAccount); }
        public Venue? Venue { get => _venue; set => Set(ref value, ref _venue); }
        public BetCode? BetCode { get => _betCode; set => Set(ref value, ref _betCode); }
        public Runner? Runner { get => _runner; set => Set(ref value, ref _runner); }
        public BetPayment? BetPayment { get => _betPayment; set => Set(ref value, ref _betPayment); }
        public Market? Market { get => _market; set => Set(ref value, ref _market); }
        public IAbstractModel? Model { get => _model; set => Set(ref value, ref _model); }
        #endregion
        public BetFilterManager() 
        {
            AccountHolders = IRecordSource.InitSource<AccountHolder>();
    
            BookMakers = IRecordSource.InitSource<BookMakerAccount>();
            BookMakers.SetFilter(new BookmakerFilter());

            Venues = IRecordSource.InitSource<Venue>();
            Venues.SetFilter(new VenueFilter());

            Markets = IRecordSource.InitSource<Market >();
            Markets.SetFilter(new MarketFilter());

            BetCodes = IRecordSource.InitSource<BetCode>();
            BetCodes.SetFilter(new BetCodeFilter());

            BetPayments = IRecordSource.InitSource<BetPayment>();
            BetPayments.SetFilter(new BetPaymentFilter());

            Runners = IRecordSource.InitSource<Runner>();
            Runners.SetFilter(new RunnerFilter());
        }
        protected override void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(AccountHolder))) 
                Model = e.NewValueIsNull ? BookMakerAccount : (IAbstractModel?)e.GetNewValue();                

            if (e.PropIs(nameof(BookMakerAccount)))
                Model = (e.NewValueIsNull) ? AccountHolder : (IAbstractModel?)e.GetNewValue();

            if (e.PropIs(nameof(IsPromo)) || e.PropIs(nameof(IsNoPromo)))
            {
                bool? value = (bool?)e.GetNewValue();
                if (value.HasValue && value.Value)
                    switch(true) 
                    {
                        case true when e.PropIs(nameof(IsPromo)):
                            IsNoPromo = false;
                        break;
                        case true when e.PropIs(nameof(IsNoPromo)):
                            IsPromo = false;
                        break;
                    }
            }

            if (!e.PropIs(nameof(Model)) && _accountHolder != null && _bookMakerAccount != null)
                Model = _accountHolder;

            base.OnAfterUpdate(sender, e);
        }

        public void SetBookMakerAccountHolder(AccountHolderBookMakerAccount accHldBkAcc) 
        {
            _accountHolder = accHldBkAcc.AccountHolder;
            _bookMakerAccount = accHldBkAcc.BookMakerAccount;
        }

        public override bool CheckBools(bool value)
        {
            return true switch
            {
                true when IsPromo => value == true,
                true when IsNoPromo => value == false,
                _ => true,
            };
        }

        public override bool CheckRecord(Bet record) =>
                    PropCheck(ref _accountHolder, record.AccountHolder) &&
                    PropCheck(ref _bookMakerAccount, record.BookMakerAccount) &&
                    PropCheck(ref _venue, record.Venue) &&
                    PropCheck(ref _betCode, record.BetCode) &&
                    PropCheck(ref _runner, record.Runner) &&
                    PropCheck(ref _betPayment, record.BetPayment) &&
                    PropCheck(ref _market, record.Market) &&
                    CheckBools(record.IsPromo2) &&
                    CheckDates(record.DateOfBet);
    }
    #endregion
}
