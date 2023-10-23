using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
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
