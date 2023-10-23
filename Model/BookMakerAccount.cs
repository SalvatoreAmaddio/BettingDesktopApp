using SARModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static SARGUI.View;

namespace Betting.Model
{
    public class BookMakerAccount : AbstractTableModel<BookMakerAccount>
    {
        #region BackProps
        long _BookMakerAccountID;
        string _BookMakerName = string.Empty;
        bool _status = true;
        int _promos;
        int _nopromos;
        double _promoSpend;
        double _noPromoSpend;
        string? _imgPath = null;
        int _greenThreshold;
        int _bettingFlow=100;
        double _currentThreshold;
        SolidColorBrush _backgroundThreshold=Brushes.Green;
        SolidColorBrush _foregroundDiff = Brushes.Black;
        #endregion

        #region Props
        [PK]
        public long BookMakerAccountID { get => _BookMakerAccountID; set => Set(ref value, ref _BookMakerAccountID); }
        [ColumnName]
        public string BookMakerName { get => _BookMakerName; set => Set(ref value, ref _BookMakerName); }
        [ColumnName]
        public bool Status { get => _status; set => Set(ref value, ref _status); }
        [ColumnName]
        public string? ImgPath { get => _imgPath; set => Set(ref value, ref _imgPath); }
        [ColumnName]
        public int GreenThreshold { get => _greenThreshold; set => Set(ref value, ref _greenThreshold); }
        [ColumnName]
        public int BettingFlow { get => _bettingFlow; set => Set(ref value, ref _bettingFlow); }

        public double PromoSpend { get => _promoSpend; private set => Set(ref value, ref _promoSpend); }
        public double NoPromoSpend { get => _noPromoSpend; private set => Set(ref value, ref _noPromoSpend); }
        public int Promos { get=>_promos; private set=>Set(ref value, ref _promos); }
        public int NoPromos { get=>_nopromos; private set=>Set(ref value, ref _nopromos); }
        public double Diff { get => PromoSpend - NoPromoSpend; }
        public int YellowThreshold { get => _greenThreshold+10; }
        public int RedThreshold { get => YellowThreshold + 10; }
        public double CurrentThreshold { get => _currentThreshold; set => Set(ref value, ref _currentThreshold); }
        public SolidColorBrush BackgroundThreshold { get => _backgroundThreshold; set => Set(ref value, ref _backgroundThreshold); }
        public SolidColorBrush ForegroundDiff { get => _foregroundDiff; set => Set(ref value, ref _foregroundDiff); }

        public ImageStorageManager? ImageStorageManager { get; set; }
        #endregion

        #region Constructors
        public BookMakerAccount() 
        {
            ImageStorageManager = new("BookMakerLogos",
            (IsDirty, newImgPath) => ImgPath = newImgPath,
            () => ImgPath, () => this == null,
            (val) =>ImgPath = val?.ToString());
            AfterUpdate += BookMakerAccount_AfterUpdate;
            IsDirty = false;
        }

        private void BookMakerAccount_AfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(GreenThreshold)))
            {
                int greenThreshold = (int)e.GetNewValue();
                BettingFlow = 100-greenThreshold;
                UpdateSpending();
            }

            if (e.PropIs(nameof(CurrentThreshold)))
            {
                double currentThreshould = (double)e.GetNewValue();
                if (currentThreshould <= GreenThreshold)
                {
                    BackgroundThreshold = Brushes.Green;
                }
                else
                {
                    BackgroundThreshold = Brushes.Yellow;
                    if (currentThreshould >= RedThreshold)
                    {
                        BackgroundThreshold = Brushes.Red;
                    }
                }
            }
        }

        public BookMakerAccount(long id) : this() => _BookMakerAccountID = id;

        public BookMakerAccount(IDataReader reader) : this()
        {
            _BookMakerAccountID = reader.GetInt64(0);
            _BookMakerName = reader.GetString(1);
            _status = reader.GetBoolean(2);
            try
            {
                _imgPath = reader.GetString(3);
            }
            catch { }

            try
            {
                _greenThreshold = reader.GetInt32(4);
            }
            catch { }

            try
            {
                _bettingFlow = reader.GetInt32(5);
            }
            catch { }
        }
        #endregion

        public override bool IsNewRecord => _BookMakerAccountID == 0;

        public override string ObjectName => _BookMakerName;

        public override int ObjectHashCode => HashCode.Combine(_BookMakerAccountID);

        public override bool CanSave()
        {
            if (string.IsNullOrEmpty(BookMakerName)) return false;
            return true;
        }

        public override bool IsEqualTo(object? obj) =>
        obj is BookMakerAccount _book && _book.BookMakerAccountID == _BookMakerAccountID;

        public override BookMakerAccount GetRecord(IDataReader reader) => new(reader);

        public override void Params(Params param)
        {
            param.AddProperty(BookMakerAccountID, nameof(BookMakerAccountID));
            param.AddProperty(BookMakerName, nameof(BookMakerName));
            param.AddProperty(Status, nameof(Status));
            param.AddProperty(ImgPath, nameof(ImgPath));
            param.AddProperty(GreenThreshold,nameof(GreenThreshold));
            param.AddProperty(BettingFlow,nameof(BettingFlow));
        }
        
        public override Task SetForeignKeys() => Task.CompletedTask;
        public override void SetPrimaryKey(long id) => _BookMakerAccountID = id;

        public void UpdateSpending()
        {
            var NoPromoBets=DatabaseManager.GetDatabaseTable<Bet>().DataSource.Where<Bet>(s => s.BookMakerAccount.IsEqualTo(this) && s.IsPromo.Equals("NO"), false).ToList();
            var PromoBets = DatabaseManager.GetDatabaseTable<Bet>().DataSource.Where<Bet>(s => s.BookMakerAccount.IsEqualTo(this) && s.IsPromo.Equals("YES"), false).ToList();
            
            Promos = PromoBets.Count;
            NoPromos = NoPromoBets.Count;
            int TotalBets = Promos + NoPromos;

            NoPromoSpend=NoPromoBets.Sum(s=>s.Profit);
            PromoSpend=PromoBets.Sum(s=>s.Profit);

            NotifyView(nameof(Diff));
            if (TotalBets>0)
            {
                CurrentThreshold = ((double)Promos / (double)TotalBets) * 100;
            }

            ForegroundDiff = Brushes.Black;
            if (CurrentThreshold > 0)
            {
                ForegroundDiff = (Diff >= 0) ? Brushes.Black : Brushes.Red;
            } 

            IsDirty = false;
        }
    }

}
