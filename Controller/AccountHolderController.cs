using Betting.Model;
using Betting.View;
using SARGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public override void OpenRecord(IAbstractModel record)
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