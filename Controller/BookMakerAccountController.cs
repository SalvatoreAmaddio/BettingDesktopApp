using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SARGUI.View;

namespace Betting.Controller
{
    public class BookMakerAccountController : AbstractDataController<BookMakerAccount>
    {
        #region BookMakerOrder
        BookMakerOrder _bookMakerOrder;
        public BookMakerOrder BookMakerOrder
        {
            get=> _bookMakerOrder;
            set => Set(ref value,ref _bookMakerOrder);
        }
        #endregion

        public ImageStorageManager? ImageStorageManager { get; set; }

        public BookMakerAccountController()
        {
            _bookMakerOrder = new(ChildSource.SourceID);
            AfterUpdate += OnAfterUpdate;
        }

        protected override void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(Search)))
            {
                BookMakerOrder.Requery();
                SelectedRecord = ChildSource.FirstOrDefault();
            }
        }

        public static Task Calculate()
        {
            Parallel.ForEach(DatabaseManager.GetDatabaseTable<BookMakerAccount>().DataSource, (record) =>
            ((BookMakerAccount)record).UpdateSpending());
            return Task.CompletedTask;
        }

        public static Task Calculate(BookMakerAccount bkAcc)
        {
            Parallel.ForEach(DatabaseManager.GetDatabaseTable<BookMakerAccount>().DataSource.Where<BookMakerAccount>(s=>s.IsEqualTo(bkAcc),false), (record) =>
            record.UpdateSpending());
            return Task.CompletedTask;
        }

        public override void OpenRecord(IAbstractModel record)
        {
            if (record.IsNewRecord) return;
            if (record.IsDirty)
                if (!Save(record)) return;

            PromotionList promotionList = new(record);
            promotionList.ShowDialog();
        }

        public override void OpenNewRecord(IAbstractModel record)=>ChildSource.GoNewRecord();

        public override bool Save(IAbstractModel? record)
        {
            Task task = new(() => ((BookMakerAccount?)record)?.ImageStorageManager?.UpdateStorage());

            bool result=base.Save(record);
            if (result) 
            {
                BookMakerOrder.Requery();
                task.Start();
            }
            return result;
        }

        public override bool Delete(IAbstractModel? record)
        {
            string? imgPath = (record != null) ? ((BookMakerAccount)record).ImgPath : string.Empty;

            Task task = new(() => ImageStorageManager?.OnRecordDeleted(imgPath));

            bool result = base.Delete(record);
            if (result) 
            {
                BookMakerOrder.Requery();
                task.Start();
            }
            return result;
        }

        public override async void RunOffice(OfficeApplication officeApp)
        {

            IsLoading = true;
            IsLoading = await Task.Run(
                        () =>
                        WriteExcel(OfficeFileMode.WRITE, Path.Combine(Sys.DesktopPath, "BookMakerAccountsReport.xlsx"),
                        (excel) =>
                        {
                            excel.Range.Style("C1:J1", new("0", Styles.NumberFormat));
                            excel.Range.Style("C1:J1", new(XLAlign.Center, Styles.HorizontalAlignment));
                            excel.Range.Style("H1:J1", new("£#,##0;[Red]-£#,##0", Styles.NumberFormat));
                            excel.Range.Style("G1", new("0%", Styles.NumberFormat));
                            excel.Range.Merge("C1:D1");
                            excel.Range.Style("C1:D1",new (37.5,Styles.RowHeight));
                            excel.Range.Style("C1:D1", new(7, Styles.ColumnWidth));
                            excel.Range.UseUsedRange();
                            int row = excel.Range.Rows + 2;
                            excel.Range.WriteValue(row, 1, "TOTALS");
                            excel.Range.WriteValue(row, 5, $"=SUM(E2:E{excel.Range.Rows})");
                            excel.Range.WriteValue(row, 6, $"=SUM(F2:F{excel.Range.Rows})");
                            excel.Range.WriteValue(row, 7, $"=SUM(G2:G{excel.Range.Rows})");
                            excel.Range.WriteValue(row, 8, $"=SUM(H2:H{excel.Range.Rows})");
                            excel.Range.WriteValue(row, 9, $"=SUM(I2:I{excel.Range.Rows})");
                            excel.Range.WriteValue(row, 10, $"=SUM(J2:J{excel.Range.Rows})");
                            excel.Range.Style($"A{row}:J{row}", new(true, Styles.Bold));
                            excel.Range.Style($"A{row}:J{row}", new(new ExcelColor(Color.LightGray), Styles.FillColor));
                            excel.Range.Style($"A1:J1", new(true, Styles.WrapText));
                            excel.Range.Style($"A1:J1", new(14, Styles.ColumnWidth));
                        }));
        }

        public override Task<object?[,]> OrganiseExcelData()
        {
            object?[,] data = GenerateDataTable("AGENCY", "STATUS", "BETTING FLOW", "", "PROMO", "NON PROMO",
           "PROMO BETS", "PROMO SPENDS", "NON PROMO SPENDS", "DIFF");

            Parallel.For(0, ChildSource.RecordCount, (row) =>
            {
                BookMakerAccount record = (BookMakerAccount)ChildSource.Get(row);
                int r = row + 1;
                data[row + 1, 0] = record.BookMakerName;
                data[row + 1, 1] = (record.Status) ? "ACTIVE" : "UNACTIVE";
                data[row + 1, 2] = record.GreenThreshold;
                data[row + 1, 3] = record.BettingFlow;
                data[row + 1, 4] = record.Promos;
                data[row + 1, 5] = record.NoPromos;
                data[row + 1, 6] = record.CurrentThreshold/100;
                data[row + 1, 7] = record.PromoSpend;
                data[row + 1, 8] = record.NoPromoSpend;
                data[row + 1, 9] = record.Diff;
            });
            return Task.FromResult(data);
        }
    }

    public class BookMakerOrder : AbstractRecordsOrganizer
    {
        public BookMakerOrder() { }
        public BookMakerOrder(long sourceid) : base(sourceid) { }     
        protected override IRecordSource OriginalSource => DatabaseManager.GetDatabaseTable<BookMakerAccount>().DataSource;
        public override bool FilterCriteria(IAbstractModel record)
        {
            string? dataContext = GetDataContext<string>();
            if (dataContext == null) return false;
            var result = record?.ToString()?.ToLower().Contains(dataContext.ToLower());
            return result ?? false;
        }
        public override void OnReorder(IRecordSource FilteredSource) => FilteredSource.ReplaceData(FilteredSource.OrderBy(s => s.ObjectName));   
    }
}