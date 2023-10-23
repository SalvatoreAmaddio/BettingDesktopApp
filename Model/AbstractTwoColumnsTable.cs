using SARModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betting.Model
{
    abstract public class AbstractTwoColumnsTable<M> : AbstractTableModel<M> where M : new()
    {
        #region backprops
        long _id;
        string _description = string.Empty;
        #endregion

        #region Props
        [PK]
        public long ID { get => _id; set => Set(ref value, ref _id); }
        [ColumnName]
        public string Description { get => _description; set => Set(ref value, ref _description); }
        #endregion

        #region Constructors
        public AbstractTwoColumnsTable() => IsDirty = false;
        public AbstractTwoColumnsTable(long id) => _id = id;

        public AbstractTwoColumnsTable(long id, string description) : this(id) =>
        _description = description;
        public AbstractTwoColumnsTable(IDataReader reader)
        {
            _id = reader.GetInt64(0);
            _description = reader.GetString(1);
        }
        #endregion

        public override bool IsNewRecord => _id == 0;

        public override string ObjectName => _description;

        public override int ObjectHashCode => HashCode.Combine(_id);

        public override bool CanSave()=>!string.IsNullOrEmpty(_description);

        public override void Params(Params param)
        {
            param.AddProperty(ID, nameof(ID));
            param.AddProperty(Description, nameof(Description));
        }

        public override Task SetForeignKeys()=> Task.CompletedTask;
        public override void SetPrimaryKey(long id)=> ID = id;

        public abstract override M GetRecord(IDataReader reader);
        public override bool IsEqualTo(object? obj)=>
        obj is AbstractTwoColumnsTable<M> m && m.ID == _id;
    }
    public class Gender : AbstractTwoColumnsTable<Gender>
    {
        public Gender()
        {
        }
        public Gender(long id) : base(id) { }

        public Gender(long id, string description) : base(id, description)
        {
        }

        public Gender(IDataReader reader) : base(reader) { }

        public override Gender GetRecord(IDataReader reader) => new(reader);
    }
    public class Venue : AbstractTwoColumnsTable<Venue>
    {
        public Venue() { }
        public Venue(long id) : base(id) { }
        public Venue(IDataReader reader) : base(reader) { }
        public override Venue GetRecord(IDataReader reader) => new(reader);

    }
    public class BetCode : AbstractTwoColumnsTable<BetCode>
    {
        public BetCode() { }
        public BetCode(long id) : base(id) { }
        public BetCode(IDataReader reader) : base(reader) { }
        public override BetCode GetRecord(IDataReader reader) => new(reader);

    }
    public class BetPayment : AbstractTwoColumnsTable<BetPayment> { 
        public BetPayment() { }
        public BetPayment(long id) : base(id) { }
        public BetPayment(IDataReader reader) : base(reader) { }
        public override BetPayment GetRecord(IDataReader reader) => new(reader);

    }
    public class Runner : AbstractTwoColumnsTable<Runner>
    {
        public Runner() { }
        public Runner(long id) : base(id) { }
        public Runner(IDataReader reader) : base(reader) { }
        public override Runner GetRecord(IDataReader reader) => new(reader);

    }
    public class Market : AbstractTwoColumnsTable<Market>
    {
        public Market() { }
        public Market(long id) : base(id) { }
        public Market(IDataReader reader) : base(reader) { }
        public override Market GetRecord(IDataReader reader) => new(reader);


    }
}
