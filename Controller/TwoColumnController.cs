using Betting.Model;
using SARGUI;
using SARModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betting.Controller
{

    abstract public class AbstractTwoColumnController<M> : AbstractDataController<M> where M : AbstractTableModel<M>, new()
    {
        public AbstractTwoColumnController()
        {
            AfterUpdate += OnAfterUpdate;
            ChildSource.SetFilter(new TwoColumnRecordOrganiser<M>());
        }

        public override Task<bool> WriteExcel() =>
        WriteExcel(OfficeFileMode.WRITE, Path.Combine(Sys.DesktopPath, $"{typeof(M).Name}.xlsx"),
        (excel) => { });

        public override Task<object?[,]> OrganiseExcelData()
        {
            object?[,] data = GenerateDataTable(typeof(M).Name);

            Parallel.For(0, ChildSource.RecordCount, (row) =>
            {
                AbstractTwoColumnsTable<M> record = (AbstractTwoColumnsTable<M>)ChildSource.Get(row);
                data[row + 1, 0] = record.Description;
            });
            return Task.FromResult(data);
        }
        protected override void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            if (e.PropIs(nameof(Search)))
            {
                if (e.NewValueIsNull) return;
                ChildSource.Requery();
                SelectedRecord = ChildSource.FirstOrDefault();
                return;
            }
        }
    }

    public class VenueController : AbstractTwoColumnController<Venue>
    {
    }

    public class BetCodeController : AbstractTwoColumnController<BetCode>
    {
    }

    public class RunnerController : AbstractTwoColumnController<Runner>
    {
    }

    public class BetPaymentController : AbstractTwoColumnController<BetPayment>
    {
    }

    public class MarketController : AbstractTwoColumnController<Market>
    {
    }

    public class TwoColumnRecordOrganiser<M> : AbstractRecordsOrganizer<M> where M : AbstractTableModel<M>, new()
    {
        public TwoColumnRecordOrganiser() {}

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
