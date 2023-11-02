using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static SARGUI.View;

namespace Betting.Controller
{
    public class PromotionController : AbstractDataController<Promotion>
    {
        private static BookMakerAccount? BkMkrAcc { get; set; }
        private PromotionFilterManager _promotionFilterManager = new();

        #region Props
        public bool HideAgencyField { get; set; } = true;
        public ImageStorageManager ImageStorageManager { get; set; }
        public PromotionFilterManager PromotionFilterManager { get => _promotionFilterManager; set => Set(ref value, ref _promotionFilterManager); }
        public RecordSource<BookMakerAccount> BookMakers {get;}
        #endregion
        
        public PromotionController()
        {
            BookMakers = IRecordSource.InitSource<BookMakerAccount>();
            BookMakers.SetFilter(new BookMakerOrder());

            ImageStorageManager = new("Banner",
                (IsDirty, newImgPath) =>
                    {
                        if (CurrentRecord == null) return;
                        CurrentRecord.ImgPath = newImgPath;
                        CurrentRecord.IsDirty = IsDirty;
                    },
                () => CurrentRecord?.ImgPath, () => CurrentRecord == null,
                (val) =>
                    {
                        if (CurrentRecord == null) return;
                        CurrentRecord.ImgPath = val?.ToString();
                    }
            );

            AfterUpdate += OnAfterUpdate;
            ChildSource.SetFilter(new PromoFilter());
            _promotionFilterManager.Run += PromotionFilterManager_Run;
        }
        private void PromotionFilterManager_Run(object? sender, EventArgs e)
        {
            ChildSource?.Filter?.SetDataContext(_promotionFilterManager);
            ChildSource?.Filter?.Run();
        }
        protected override void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(SelectedRecord)))
            {
                ImageStorageManager.SetImageOnPlaceholder();
                return;
            }
        }
        public override void OnRecordMoved(object? sender, RecordMovedEvtArgs e)
        {
            base.OnRecordMoved(sender, e);
            if (e.Record == null) return;
            ImageStorageManager?.ResetActionsAndFunctions(
                (IsDirty, newImgPath) =>
                {
                    if (CurrentRecord == null) return;
                    CurrentRecord.ImgPath = newImgPath;
                    CurrentRecord.IsDirty = IsDirty;
                },

                () => CurrentRecord?.ImgPath, () => CurrentRecord == null,

                (val) =>
                {
                    if (CurrentRecord == null) return;
                    CurrentRecord.ImgPath = val?.ToString();
                }
            );

            if (e.WentToNewRecord)
            {
                if (BkMkrAcc != null)
                {
                    Promotion promo = (Promotion)e.Record;
                    promo.BookMakerAccount = BkMkrAcc;
                    promo.IsDirty = false;
                    if (UIIsWindow())
                        GetUI<Window>().Title = $"New Promotion by {BkMkrAcc}";
                }
            }

            if (!e.Record.IsNewRecord && UIIsWindow())
                GetUI<Window>().Title = e.Record.ToString();
        }
        public override void OpenRecord(IAbstractModel? record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            PromotionForm promotionForm = new(record, HideAgencyField);
            promotionForm.ShowDialog();
        }
        public override void OnAppearingGoTo(IAbstractModel? record)
        {
            if (record is BookMakerAccount bookmaker)
            {
                BkMkrAcc = bookmaker;
                var range = MainSource.Where(s => ((Promotion)s).BookMakerAccount.IsEqualTo(record));
                RecordSource.ReplaceData(range);
                base.OnAppearingGoTo((ChildSource.RecordCount > 0) ? ChildSource.First() : new Promotion(BkMkrAcc));
                return;
            }
            base.OnAppearingGoTo(record);
        }
        public override bool OnFormClosing(CancelEventArgs e)
        {
            AllowNewRecord(false);
            return base.OnFormClosing(e);
        }
        public override bool Delete(IAbstractModel? record)
        {
            string? imgPath = (record != null) ? ((Promotion)record).ImgPath : string.Empty;

            Task task = new(() =>
            {
                ImageStorageManager?.OnRecordDeleted(imgPath);
            });

            bool result = base.Delete(record);
            if (result) task.Start();
            return result;
        }
        public override bool Save(IAbstractModel? record)
        {

            if (record != null && BkMkrAcc != null)
                ((Promotion)record).BookMakerAccount = BkMkrAcc;

            bool result = base.Save(record);
            if (UIIsWindow())
                GetUI<Window>().Title = record?.ToString();

            ImageStorageManager.UpdateStorage();

            if (BkMkrAcc != null) 
                BetController.RunRefreshOnBookMakerChangedTask(new(BkMkrAcc));            

            return result;
        }
        public override Task<bool> WriteExcel() =>
        WriteExcel(OfficeFileMode.WRITE, Path.Combine(Sys.DesktopPath, "PromotionReport.xlsx"),
          (excel) =>
          {
              excel.Range.Style("B1", new("dd/MM/yyyy", Styles.NumberFormat));
              excel.Range.Style("D1", new("£#,##0;[Red]-£#,##0", Styles.NumberFormat));
          });
        public override Task<object?[,]> OrganiseExcelData()
        {
            object?[,] data = GenerateDataTable("AGENCY", "DATE", "DESCRIPTION", "BONUS", "RACE 1", "RACE 2",
           "RACE 3", "RACE 4", "RACE 5", "RACE 6", "RACE 7", "RACE 8", "RACE 9", "RACE 10");

            Parallel.For(0, ChildSource.RecordCount, (row) =>
            {
                Promotion record = (Promotion)ChildSource.Get(row);
                data[row + 1, 0] = $"{record.BookMakerAccount}";
                data[row + 1, 1] = $"'{record.DateOfPromotion:d}";
                data[row + 1, 2] = record.Description;
                data[row + 1, 3] = record.BonusUpTo;
                data[row + 1, 4] = record.PromotedRaces[0];
                data[row + 1, 5] = record.PromotedRaces[1];
                data[row + 1, 6] = record.PromotedRaces[2];
                data[row + 1, 7] = record.PromotedRaces[3];
                data[row + 1, 8] = record.PromotedRaces[4];
                data[row + 1, 9] = record.PromotedRaces[5];
                data[row + 1, 10] = record.PromotedRaces[6];
                data[row + 1, 11] = record.PromotedRaces[7];
                data[row + 1, 12] = record.PromotedRaces[8];
                data[row + 1, 13] = record.PromotedRaces[9];
            });
            return Task.FromResult(data);
        }
    }

    public class PromoFilter : AbstractRecordsOrganizer<Promotion>
    {
        public override bool FilterCriteria(IAbstractModel record)
        {
            PromotionFilterManager? dataContext = GetDataContext<PromotionFilterManager>();
            if (dataContext == null) return true;
            return dataContext.CheckRecord((Promotion)record);
        }

        public override void OnReorder(IRecordSource FilteredSource) =>
        FilteredSource.ReplaceData(FilteredSource.OrderByDescending(s => ((Promotion)s).DateOfPromotion));
    }
    public class PromotionFilterManager : AbstractFilterManager<Promotion>
    {
        #region backprops
        private BookMakerAccount? _bookMakerAccount;
        private bool _race1 = false;
        private bool _race2 = false;
        private bool _race3 = false;
        private bool _race4 = false;
        private bool _race5 = false;
        private bool _race6 = false;
        private bool _race7 = false;
        private bool _race8 = false;
        private bool _race9 = false;
        private bool _race10 = false;
        private bool AllFalse
        {
            get =>
                _race1 == false
                && _race2 == false
                && _race3 == false
                && _race4 == false
                && _race5 == false
                && _race6 == false
                && _race7 == false
                && _race8 == false
                && _race9 == false
                && _race10 == false;
        }
        #endregion

        #region Props
        public bool Race1 { get => _race1; set => Set(ref value, ref _race1); }
        public bool Race2 { get => _race2; set => Set(ref value, ref _race2); }
        public bool Race3 { get => _race3; set => Set(ref value, ref _race3); }
        public bool Race4 { get => _race4; set => Set(ref value, ref _race4); }
        public bool Race5 { get => _race5; set => Set(ref value, ref _race5); }
        public bool Race6 { get => _race6; set => Set(ref value, ref _race6); }
        public bool Race7 { get => _race7; set => Set(ref value, ref _race7); }
        public bool Race8 { get => _race8; set => Set(ref value, ref _race8); }
        public bool Race9 { get => _race9; set => Set(ref value, ref _race9); }
        public bool Race10 { get => _race10; set => Set(ref value, ref _race10); }
        public BookMakerAccount? BookMakerAccount { get => _bookMakerAccount; set => Set(ref value, ref _bookMakerAccount); }
        #endregion

        public override bool CheckRecord(Promotion record) 
        {
            bool outcome =
                PropCheck(ref _bookMakerAccount, record.BookMakerAccount)
                && CheckDates(record.DateOfPromotion);
            if (!AllFalse) 
            {
                outcome = outcome && (CheckBools(record.PromotedRaces[0])
                                  || CheckBools(record.PromotedRaces[1])
                                  || CheckBools(record.PromotedRaces[2])
                                  || CheckBools(record.PromotedRaces[3])
                                  || CheckBools(record.PromotedRaces[4])
                                  || CheckBools(record.PromotedRaces[5])
                                  || CheckBools(record.PromotedRaces[6])
                                  || CheckBools(record.PromotedRaces[7])
                                  || CheckBools(record.PromotedRaces[8])
                                  || CheckBools(record.PromotedRaces[9]));
            }
            return outcome;
        }
        public override bool CheckBools(bool value)
            => true switch
            {
                true when Race1 && value => value == Race1,
                true when Race2 && value => value == Race2,
                _ => false,
            };            
        }
}