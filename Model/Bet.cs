using Betting.Controller;
using SARGUI.Converters;
using SARModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betting.Model
{
    public class Bet : AbstractTableModel<Bet>
    {
        #region BackProps
        long _betID;
        AccountHolderBookMakerAccount _accountHolderBookMakerAccount = new();
        BookMakerAccount _bookmakerAccount = new();
        AccountHolder _accountHolder = new();
        DateTime? _dateOfBet = null;
        DateTime? _timeofBet = null;
        Venue _venue = new();
        BetCode _betCode = new();
        byte _raceno = 1;
        Runner _runner = new();
        BetPayment _betPayment = new();
        Market _market = new();
        double _odd;
        double _wager;
        double _profit;
        bool _result = false;
        string _notes = string.Empty;
        public IRecordsOrganizer? _betAccountHolderFilter;
        #endregion

        #region Props
        [PK]
        public long BetID { get => _betID; set => Set(ref value, ref _betID); }
        [FK("AccountHolderBookMakerID")]
        public AccountHolderBookMakerAccount AccountHolderBookMakerAccount { get => _accountHolderBookMakerAccount; set => Set(ref value, ref _accountHolderBookMakerAccount); }
        [ColumnName("DateOfBet")]
        public DateTime? DateOfBet { get => _dateOfBet; set => Set(ref value, ref _dateOfBet); }
        [ColumnName("TimeOfBet")]
        public DateTime? TimeOfBet { get => _timeofBet; set => Set(ref value, ref _timeofBet); }
        [FK("VenueID")]
        public Venue Venue { get => _venue; set => Set(ref value, ref _venue); }
        [FK("BetCodeID")]
        public BetCode BetCode { get => _betCode; set => Set(ref value, ref _betCode); }
        [ColumnName]
        public byte RaceNo { get => _raceno; set => Set(ref value, ref _raceno); }
        [FK("RunnerID")]
        public Runner Runner { get => _runner; set => Set(ref value, ref _runner); }
        [FK("BetPaymentID")]
        public BetPayment BetPayment { get => _betPayment; set => Set(ref value, ref _betPayment); }
        [FK("MarketID")]
        public Market Market { get => _market; set => Set(ref value, ref _market); }
        [ColumnName]
        public double Odd { get => _odd; set => Set(ref value, ref _odd); }
        [ColumnName]
        public double Wager { get => _wager; set => Set(ref value, ref _wager); }
        [ColumnName]
        public double Profit { get => _profit; set => Set(ref value, ref _profit); }
        [ColumnName]
        public bool Result { get => _result; set => Set(ref value, ref _result); }
        [ColumnName]
        public string Notes { get => _notes; set => Set(ref value, ref _notes); }

        public BookMakerAccount BookMakerAccount
        {
            get => _bookmakerAccount;
            set => Set(ref value, ref _bookmakerAccount);
        }

        public AccountHolder AccountHolder
        {
            get => _accountHolder;
            set => Set(ref value, ref _accountHolder);
        }

        public IRecordsOrganizer? BetAccountHolderFilter { get => _betAccountHolderFilter; set => Set(ref value, ref _betAccountHolderFilter); }
        public string IsPromo { get => IsPromotion(); }
        #endregion

        #region Constructors
        public Bet()=>AfterUpdate += OnAfterUpdate;
        public Bet(AccountHolderBookMakerAccount accHldBkAcc) : this() => SetAccountsDetails(accHldBkAcc);
        public Bet(long id) : this() => _betID = id;
        public Bet(IRecordsOrganizer recordsOrganizer) : this()=>_betAccountHolderFilter=recordsOrganizer;
        public Bet(IDataReader reader) : this()
        {
            _betID = reader.GetInt64(0);
            _accountHolderBookMakerAccount = new(reader.GetInt64(1));
            _dateOfBet = DatabaseManager.GetDateFromDB(reader.GetString(2));
            _timeofBet = DatabaseManager.GetDateFromDB(reader.GetString(3));
            _venue = new(reader.GetInt64(4));
            _betCode = new(reader.GetInt64(5));
            _raceno = reader.GetByte(6);
            _runner = new(reader.GetInt64(7));
            _betPayment = new(reader.GetInt64(8));
            _market = new(reader.GetInt64(9));
            _odd = reader.GetDouble(10);
            _wager = reader.GetDouble(11);
            _profit = reader.GetDouble(12);
            _result = reader.GetBoolean(13);
            _notes = reader.GetString(14);
        }
        #endregion

        public void SetAccountsDetails(AccountHolderBookMakerAccount accHldBkAcc)
        {
            _accountHolderBookMakerAccount = accHldBkAcc;
            _bookmakerAccount = accHldBkAcc.BookMakerAccount;
            _accountHolder = accHldBkAcc.AccountHolder;
        }
        public void RefreshPromo() => NotifyView(nameof(IsPromo));
        private void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            bool updatePromo = e.PropIs(nameof(DateOfBet)) || e.PropIs(nameof(RaceNo)) || e.PropIs(nameof(AccountHolderBookMakerAccount));

            if (updatePromo)
            {
                RefreshPromo();
                return;
            }
            
            if (e.PropIs(nameof(BetAccountHolderFilter)))
            {
                IsDirty = false;
                return;
            }
        }
        string IsPromotion()
        {
            bool result = DatabaseManager.GetDatabaseTable<Promotion>().DataSource.Any(s => ((Promotion)s).DateOfPromotion.Equals(DateOfBet) && ((Promotion)s).IsPromotedRace(RaceNo) && ((Promotion)s).BookMakerAccount.Equals(AccountHolderBookMakerAccount.BookMakerAccount));
            return (result) ? "YES" : "NO";
        }
        public override bool IsNewRecord => _betID==0;
        public override string ObjectName => $"Bet On {_dateOfBet?.ToString("dd/MM/yyyy")} by {_accountHolderBookMakerAccount}";
        public override int ObjectHashCode => HashCode.Combine(BetID);
        public override bool CanSave() => true;
        public override Bet GetRecord(IDataReader reader)=> new(reader);
        public override bool IsEqualTo(object? obj)=> obj is Bet bet && bet._betID == _betID;
        public override void Params(Params param)
        {
            param.AddProperty(BetID, nameof(BetID));
            param.AddProperty(AccountHolderBookMakerAccount.AccountHolderBookMakerAccountId, "AccountHolderBookMakerID");
            param.AddProperty(DateOfBet, nameof(DateOfBet));
            param.AddProperty(TimeOfBet, nameof(TimeOfBet));
            param.AddProperty(Venue.ID, "VenueID");
            param.AddProperty(BetCode.ID, "BetCodeID");
            param.AddProperty(RaceNo, nameof(RaceNo));
            param.AddProperty(Runner.ID, "RunnerID");
            param.AddProperty(BetPayment.ID, "BetPaymentID");
            param.AddProperty(Market.ID, "MarketID");
            param.AddProperty(Odd, nameof(Odd));
            param.AddProperty(Wager, nameof(Wager));
            param.AddProperty(Profit, nameof(Profit));
            param.AddProperty(Result, nameof(Result));
            param.AddProperty(Notes, nameof(Notes));
        }
        public override Task SetForeignKeys()
        {
            _accountHolderBookMakerAccount = GetFK(_accountHolderBookMakerAccount);
            _accountHolder = GetFK(new AccountHolder(_accountHolderBookMakerAccount.AccountHolder.AccountHolderId));
            _bookmakerAccount = GetFK(new BookMakerAccount(_accountHolderBookMakerAccount.BookMakerAccount.BookMakerAccountID));
            _venue.Description = GetFK(_venue).Description;
            _market.Description = GetFK(_market).Description;
            _betCode.Description = GetFK(_betCode).Description;
            _betPayment.Description = GetFK(_betPayment).Description;
            _runner.Description = GetFK(_runner).Description;
            return Task.CompletedTask;
        }
        public override void SetPrimaryKey(long id)=> _betID = id;

        public void ReplaceBookMakerIF(AccountHolderBookMakerAccount AccHldrBkMkAcc)
        {
            if (AccountHolderBookMakerAccount.IsEqualTo(AccHldrBkMkAcc))
                BookMakerAccount = AccHldrBkMkAcc.BookMakerAccount;
        }
    }
}
