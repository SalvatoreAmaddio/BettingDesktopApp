using Betting.Model;
using SARGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betting.Controller
{

    abstract public class AbstractTwoColumnController<M> : AbstractDataController<M> where M : AbstractTableModel<M>, new()
    {
        AbstractRecordsOrganizer _recordsOrganizer;
        public AbstractRecordsOrganizer RecordsOrganizer { get=>_recordsOrganizer; set=>Set(ref value, ref _recordsOrganizer); }
        public AbstractTwoColumnController()
        {
            AfterUpdate += OnAfterUpdate;
             
        }

        private void OnAfterUpdate(object? sender, SARModel.AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(Search)))
            {
                if (e.NewValueIsNull) return;
                RecordsOrganizer.Requery();
                SelectedRecord = ChildSource.FirstOrDefault();
                return;
            }
        }
    }

    public class VenueController : AbstractTwoColumnController<Venue>
    {
        public VenueController() 
        {
            RecordsOrganizer = new TwoColumnRecordOrganiser<Venue>(ChildSource.SourceID);
        }
    }

    public class BetCodeController : AbstractTwoColumnController<BetCode>
    {
        public BetCodeController()
        {
            RecordsOrganizer = new TwoColumnRecordOrganiser<BetCode>(ChildSource.SourceID);
        }
    }

    public class RunnerController : AbstractTwoColumnController<Runner>
    {
        public RunnerController()
        {
            RecordsOrganizer = new TwoColumnRecordOrganiser<Runner>(ChildSource.SourceID);
        }
    }

    public class BetPaymentController : AbstractTwoColumnController<BetPayment>
    {
        public BetPaymentController()
        {
            RecordsOrganizer = new TwoColumnRecordOrganiser<BetPayment>(ChildSource.SourceID);
        }
    }

    public class MarketController : AbstractTwoColumnController<Market>
    {
        public MarketController()
        {
            RecordsOrganizer = new TwoColumnRecordOrganiser<Market>(ChildSource.SourceID);
        }
    }

    public class TwoColumnRecordOrganiser<M> : AbstractRecordsOrganizer where M : AbstractTableModel<M>, new()
    {
        protected override IRecordSource OriginalSource => DatabaseManager.GetDatabaseTable<M>().DataSource;
        public TwoColumnRecordOrganiser(long sourceid) => SourceID = sourceid;

        public override bool FilterCriteria(IAbstractModel model)
        {
            string? dataContext = GetDataContext<string>();
            if (dataContext == null) return false;
            var result = model?.ToString()?.ToLower().Contains(dataContext.ToLower());
            return result ?? false;
        }

        public override void OnReorder(IRecordSource FilteredSource) => FilteredSource.ReplaceData(FilteredSource.OrderBy(s => s.ToString()));
    }
}
