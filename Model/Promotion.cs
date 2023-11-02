using MvvmHelpers;
using SARModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Betting.Model
{
    public class Promotion : AbstractTableModel<Promotion>
    {
        #region backprop
        long _promotionid;
        BookMakerAccount? _bookMakerAccount;
        DateTime? _dateOfPromotion = null;
        string _description = string.Empty;
        double _bonusUpTo;
        string? _imgPath = null;
        #endregion

        #region Prop
        [PK]
        public long PromotionID { get => _promotionid; set => Set(ref value, ref _promotionid); }
        [FK("BookMakerAccountID")]
        public BookMakerAccount? BookMakerAccount { get => _bookMakerAccount; set => Set(ref value, ref _bookMakerAccount); }
        [ColumnName]
        public DateTime? DateOfPromotion { get => _dateOfPromotion; set => Set(ref value, ref _dateOfPromotion); }
        [ColumnName]
        public string Description { get => _description; set => Set(ref value, ref _description); }
        [ColumnName]
        public double BonusUpTo { get => _bonusUpTo; set => Set(ref value, ref _bonusUpTo); }
        [ColumnName]
        public string? ImgPath { get => _imgPath; set => Set(ref value, ref _imgPath); }

        [ColumnNameArray("Race", 10)]
        public PromotedRaces PromotedRaces { get; private set; }

        public string Races { get => $"{PromotedRaces}"; }
        #endregion

        #region Constructors
        public Promotion()=>PromotedRaces = new(this);
        public Promotion(long _promoid) : this() => _promotionid = _promoid;
        public Promotion(BookMakerAccount bkAcc) : this() => _bookMakerAccount = bkAcc;
        public Promotion(IDataReader reader) : this()
        {
            _promotionid = reader.GetInt64(0);
            _bookMakerAccount = new(reader.GetInt64(1));
            _dateOfPromotion = DateTime.Parse(reader.GetString(2));
            _description = reader.GetString(3);
            _bonusUpTo = reader.GetDouble(4);
            _imgPath = reader.GetValue(5)?.ToString();
            PromotedRaces.FillUp(6, reader);
        }
        #endregion

        public bool IsPromotedRace(byte race) => PromotedRaces.IsPromotedRace(race);
        public override bool IsNewRecord => _promotionid==0;
        public override string ObjectName => $"Promotion by {BookMakerAccount} on {DateOfPromotion?.ToString("dd/MM/yyyy")} {PromotedRaces}";
        public override int ObjectHashCode => HashCode.Combine(PromotionID);
        public override bool CanSave() 
        {
            if (!IsDirty) return true;
            switch (false)
            {
                case false when BookMakerAccount == null:
                case false when !PromotedRaces.ArePromotedRacesSelected():
                case false when DateOfPromotion == null:
                    return false;
                default: break;
            }
            return true;
        }
        public override Promotion GetRecord(IDataReader reader) => new(reader);
        public override bool IsEqualTo(object? obj)=>obj is Promotion promo && promo.PromotionID == PromotionID;
        public override void Params(Params param)
        {
            param.AddProperty(PromotionID, nameof(PromotionID));
            param.AddProperty(BookMakerAccount?.BookMakerAccountID, "BookMakerAccountID");
            param.AddProperty(Description, nameof(Description));
            param.AddProperty(BonusUpTo, nameof(BonusUpTo));
            param.AddProperty(DateOfPromotion, nameof(DateOfPromotion));
            param.AddProperty(ImgPath, nameof(ImgPath));
            param.AddEnumerableProperty<bool>(PromotedRaces, "Race1", "Race2", "Race3", "Race4", "Race5", "Race6", "Race7", "Race8", "Race9", "Race10");
        }
        public override Task SetForeignKeys()
        {
            if (_bookMakerAccount!=null)
                _bookMakerAccount = GetFK(_bookMakerAccount);
            return Task.CompletedTask;
        }
        public override void SetPrimaryKey(long id)=>PromotionID = id;
    }

    public class PromotedRaces : ObservableRangeCollection<bool>
    {
        public int Races { get; }
        readonly IAbstractModel? Model;
        public PromotedRaces(IAbstractModel? model, int races = 10)
        {
            Races = races;
            for (int i = 1; i <= Races; i++) Add(false);
            Model = model;
            PropertyChanged += (sender, e) => NotifyUpdate();
            NotifyUpdate(false);
        }

        void NotifyUpdate(bool val = true)
        {
            if (Model == null) return;
            Model.IsDirty = val;
        }

        public void Reset()
        {
            for (int i = 0; i < Count; i++) this[i] = false;
        }

        public bool IsPromotedRace(byte race) => this[--race];
        public void FillUp(int startingindex, IDataReader reader)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = reader.GetBoolean(startingindex);
                startingindex++;
            }

            NotifyUpdate(false);
        }

        public bool ArePromotedRacesSelected()
        {
            foreach (var item in this)
            {
                if (item) return true;
            }
            return false;
        }

        public override string? ToString()
        {
            StringBuilder sb = new();
            sb.Append("On ");

            for (int index = 0; index < Count; index++)
            {
                if (this[index])
                {
                    sb.Append($"Race {index + 1},");
                }
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

    }
}
