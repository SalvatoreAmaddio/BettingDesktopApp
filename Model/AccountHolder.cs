using SARModel;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Betting.Model
{
    public class AccountHolder : AbstractTableModel<AccountHolder>
    {
        #region BackProps
        private long _accountHolderId;
        private string _firstName = string.Empty;
        private string _middleName = string.Empty;
        private string _lastName = string.Empty;
        private DateTime? _dob = null;
        private Gender? _gender = null;
        private string _streetAddress = string.Empty;
        private string _suburb = string.Empty;
        private string _state = string.Empty;
        private string _postCode = string.Empty;
        private string _originalPhoneNumber = string.Empty;
        private string _newPhoneNumber = string.Empty;
        private string _originalEmail = string.Empty;
        private string _newEmail = string.Empty;
        #endregion

        #region Properties
        [PK]
        public long AccountHolderId { get => _accountHolderId; set => Set(ref value, ref _accountHolderId); }
        [ColumnName]
        public string FirstName { get => _firstName; set => Set(ref value, ref _firstName); }
        [ColumnName]
        public string MiddleName { get => _middleName; set => Set(ref value, ref _middleName); }
        [ColumnName]
        public string LastName { get => _lastName; set => Set(ref value, ref _lastName); }
        [ColumnName]
        public DateTime? DOB { get => _dob; set => Set(ref value, ref _dob); }

        [ColumnName("GenderID")]
        public Gender? Gender { get => _gender; set => Set(ref value, ref _gender); }

        [ColumnName]
        public string StreetAddress { get => _streetAddress; set => Set(ref value, ref _streetAddress); }
        [ColumnName]
        public string Suburb { get => _suburb; set => Set(ref value, ref _suburb); }
        [ColumnName]
        public string State { get => _state; set => Set(ref value, ref _state); }
        [ColumnName]
        public string PostCode { get => _postCode; set => Set(ref value, ref _postCode); }
        [ColumnName]
        public string OriginalPhoneNumber { get => _originalPhoneNumber; set => Set(ref value, ref _originalPhoneNumber); }
        [ColumnName]
        public string NewPhoneNumber { get => _newPhoneNumber; set => Set(ref value, ref _newPhoneNumber); }
        [ColumnName]
        public string OriginalEmail { get => _originalEmail; set => Set(ref value, ref _originalEmail); }
        [ColumnName]
        public string NewEmail { get => _newEmail; set => Set(ref value, ref _newEmail); }
        #endregion

        #region Constructors
        public AccountHolder()=>AfterUpdate += OnAfterUpdate;
        public AccountHolder(long id) : this() => _accountHolderId = id;
        public AccountHolder(IDataReader reader) : this()
        {
            _accountHolderId = reader.GetInt64(0);
            _firstName = reader.GetString(1);
            _middleName = reader.GetString(2);
            _lastName = reader.GetString(3);
            try {_dob = DatabaseManager.GetDateFromDB(reader.GetString(4));}
            catch { }
            _gender = new(reader.GetInt32(5));
            _streetAddress = reader.GetString(6);
            _suburb = reader.GetString(7);
            _state = reader.GetString(8);
            _postCode = reader.GetString(9);
            _originalPhoneNumber = reader.GetString(10);
            _newPhoneNumber = reader.GetString(11);
            _originalEmail = reader.GetString(12);
            _newEmail = reader.GetString(13);
        }
        #endregion

        private void OnAfterUpdate(object? sender, AbstractPropChangedEventArgs e)
        {
            switch(true) 
            {
                case true when e.PropIs(nameof(FirstName)):
                    Capitalise(ref e, ref _firstName);
                break;
                case true when e.PropIs(nameof(MiddleName)):
                    Capitalise(ref e, ref _middleName);
                break;
                case true when e.PropIs(nameof(LastName)):
                    Capitalise(ref e, ref _lastName);
                break;
            }
        }
        public override bool IsNewRecord => AccountHolderId == 0;
        public override string ObjectName => $"{_firstName.Trim()} {_middleName.Trim()} {_lastName.Trim()}".Trim();
        public override int ObjectHashCode => HashCode.Combine(AccountHolderId);
        public override bool CanSave()
        {
            if (!IsDirty) return true;
                switch (false)
                {
                    case false when Gender == null:
                    case false when DOB == null:
                    case false when string.IsNullOrEmpty(FirstName):
                    case false when string.IsNullOrEmpty(LastName):
                        return false;
                    default:
                        break;
                }
                return true;
        }
        public override AccountHolder GetRecord(IDataReader reader) => new(reader);
        public override bool IsEqualTo(object? obj)=>
        obj is AccountHolder accHolder && accHolder.AccountHolderId == AccountHolderId;
        public override void Params(Params param)
        {
            param.AddProperty(AccountHolderId, nameof(AccountHolderId));
            param.AddProperty(FirstName, nameof(FirstName));
            param.AddProperty(MiddleName, nameof(MiddleName));
            param.AddProperty(LastName, nameof(LastName));
            param.AddProperty(DOB, nameof(DOB));
            param.AddProperty(Gender?.ID, "GenderID");
            param.AddProperty(StreetAddress, nameof(StreetAddress));
            param.AddProperty(Suburb, nameof(Suburb));
            param.AddProperty(State, nameof(State));
            param.AddProperty(PostCode, nameof(PostCode));
            param.AddProperty(OriginalPhoneNumber, nameof(OriginalPhoneNumber));
            param.AddProperty(NewPhoneNumber, nameof(NewPhoneNumber));
            param.AddProperty(OriginalEmail, nameof(OriginalEmail));
            param.AddProperty(NewEmail, nameof(NewEmail));
        }
        public override Task SetForeignKeys()
        {
            if (_gender != null) _gender.Description = GetFK(_gender).Description;
            return Task.CompletedTask;
        }
        public override void SetPrimaryKey(long id) => _accountHolderId = id;
    }
}
