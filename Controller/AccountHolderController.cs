using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Betting.Controller
{
    public class AccountHolderController : AbstractDataController<AccountHolder>
    {
        AccountHolderRecordOrganiser _accountHolderRecordOrganiser;
        public RecordSource<Gender> Genders { get; }
        public AccountHolderBookMakerAccountController AccountHolderBookMakerAccountController { get; } = new();
        public AccountHolderRecordOrganiser AccountHolderRecordOrganiser 
        { 
            get => _accountHolderRecordOrganiser;
            set => Set(ref value, ref _accountHolderRecordOrganiser); 
        }
        public AccountHolderController()
        {
            SubControllers.Add(AccountHolderBookMakerAccountController);
            Genders = new((IEnumerable<Gender>)DatabaseManager.GetDatabaseTable<Gender>().DataSource);
            DatabaseManager.AddChild<Gender>(Genders);
            AllowNewRecord(false);
            _accountHolderRecordOrganiser = new(ChildSource.SourceID);
        }
        protected override void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(Search)))
            {
                if (e.NewValueIsNull) return;
                AccountHolderRecordOrganiser.Requery();
                SelectedRecord = ChildSource.FirstOrDefault();
            }
        }
        public override void OnRecordMoved(object? sender, RecordMovedEvtArgs e)
        {
            base.OnRecordMoved(sender, e);
            AccountHolderBookMakerAccountController.OnAppearingGoTo(e.Record);
        }
        public override void OpenRecord(IAbstractModel? record)
        {
            AccountHolderForm form = new(record);
            form.ShowDialog();
        }
        public override void OnAppearingGoTo(IAbstractModel? record)
        {
            base.OnAppearingGoTo(record);
            AccountHolderBookMakerAccountController.OnAppearingGoTo(record);
        }
        public override bool Save(IAbstractModel? record)
        {
            bool NewRecordIsChanged = record != null && record.IsNewRecord && record.IsDirty;
            bool CurrentRecordIsChanged = record != null && !record.IsNewRecord && record.IsDirty;
            bool IsSaved = base.Save(record);
            switch (IsSaved && true)
            {
                case true when CurrentRecordIsChanged:
                case true when NewRecordIsChanged:
                    AccountHolderBookMakerAccountController.OnAppearingGoTo(record);
                    BetController.RunRefreshAccountHoldersTask((AccountHolder)record);
                    break;
                default:
                    AccountHolderBookMakerAccountController.OnAppearingGoTo(record);
                    break;
            }
            return IsSaved;
        }
        public override async void RunOffice(OfficeApplication officeApp)
        {

            IsLoading = true;
            IsLoading = await Task.Run(
                        ()=>
                        WriteExcel(OfficeFileMode.WRITE, Path.Combine(Sys.DesktopPath, "AccountHolderReport.xlsx"),
                        (excel) =>
                        {
                            excel.Range.Style("D1", new("dd/MM/yyyy", Styles.NumberFormat));
                        }));
        }
        public override Task<object?[,]> OrganiseExcelData() 
        {
            object?[,] data = GenerateDataTable ("FIRST NAME","MIDDLE NAME", "LAST NAME", "DOB", "GENDER", "STREET ADDRESS",
           "SUBURB", "STATE", "POST CODE", "PHONE NUM", "NEW PHONE NUM", "EMAIL","NEW EMAIL");

            Parallel.For(0, ChildSource.RecordCount, (row) =>
            {
                AccountHolder record = (AccountHolder)ChildSource.Get(row);
                data[row + 1, 0] = record.FirstName;
                data[row + 1, 1] = record.MiddleName;
                data[row + 1, 2] = record.LastName;
                data[row + 1, 3] = $"'{record.DOB:d}";
                data[row + 1, 4] = $"{record.Gender}";
                data[row + 1, 5] = record.StreetAddress;
                data[row + 1, 6] = record.Suburb;
                data[row + 1, 7] = record.State;
                data[row + 1, 8] = record.PostCode;
                data[row + 1, 9] = record.OriginalPhoneNumber;
                data[row + 1, 10] = record.NewPhoneNumber;
                data[row + 1, 11] = record.OriginalEmail;
                data[row + 1, 12] = record.NewEmail;
            });
            return Task.FromResult(data);
        }
    }

    public class AccountHolderRecordOrganiser : AbstractRecordsOrganizer {
        protected override IRecordSource OriginalSource => DatabaseManager.GetDatabaseTable<AccountHolder>().DataSource;
        public AccountHolderRecordOrganiser(long sourceid) => SourceID = sourceid;

        public override bool FilterCriteria(IAbstractModel model)
        {
            string? dataContext = GetDataContext<string>();  
            if (dataContext==null) return false;
            var result =model?.ToString()?.ToLower().Contains(dataContext.ToLower());
            return result ?? false;
        }

        public override void OnReorder(IRecordSource FilteredSource) => FilteredSource.ReplaceData(FilteredSource.OrderBy(s=>s.ToString()));
    }
}