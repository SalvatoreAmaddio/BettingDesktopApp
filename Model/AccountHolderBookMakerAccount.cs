using SARModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betting.Model
{
    public class AccountHolderBookMakerAccount : AbstractTableModel<AccountHolderBookMakerAccount>
    {
        #region BackProps
        long _accountHolderBookMakerAccountId;
        AccountHolder _accountHolder = new();
        BookMakerAccount _bookMakerAccount = new();
        string _userName = string.Empty;
        string _password = string.Empty;
        #endregion

        #region Props
        [PK]
        public long AccountHolderBookMakerAccountId { get => _accountHolderBookMakerAccountId; set => Set(ref value, ref _accountHolderBookMakerAccountId); }
        [FK("AccountHolderID")]
        public AccountHolder AccountHolder { get => _accountHolder; set => Set(ref value, ref _accountHolder); }
        [FK("BookMakerAccountID")]
        public BookMakerAccount BookMakerAccount { get => _bookMakerAccount; set => Set(ref value, ref _bookMakerAccount); }
        [ColumnName]
        public string UserName { get => _userName; set => Set(ref value, ref _userName); }
        [ColumnName]
        public string Password { get => _password; set => Set(ref value, ref _password); }
        #endregion

        #region Constructors
        public AccountHolderBookMakerAccount() {}
        public AccountHolderBookMakerAccount(AccountHolder accountHolder) =>_accountHolder=accountHolder;
        public AccountHolderBookMakerAccount(IDataReader reader)
        {
            _accountHolderBookMakerAccountId = reader.GetInt64(0);
            _accountHolder = new(reader.GetInt64(1));
            _bookMakerAccount = new(reader.GetInt64(2));
            _userName = reader.GetString(3);
            _password = reader.GetString(4);
        }
        public AccountHolderBookMakerAccount(long id) => _accountHolderBookMakerAccountId = id;
        public AccountHolderBookMakerAccount(BookMakerAccount bookmaker) => _bookMakerAccount = bookmaker;
        #endregion
        public override bool IsNewRecord => _accountHolderBookMakerAccountId==0;
        public override string ObjectName => $"{AccountHolder} with {BookMakerAccount} Agency (Account: {UserName})";
        public override int ObjectHashCode => HashCode.Combine(_accountHolderBookMakerAccountId);
        public override bool CanSave() => true;
        public override AccountHolderBookMakerAccount GetRecord(IDataReader reader) => new(reader);
        public override bool IsEqualTo(object? obj)=>
        obj is AccountHolderBookMakerAccount accHoldBkMkAcc && accHoldBkMkAcc.AccountHolderBookMakerAccountId == AccountHolderBookMakerAccountId;

        public override void Params(Params param)
        {
            param.AddProperty(AccountHolderBookMakerAccountId, nameof(AccountHolderBookMakerAccountId));
            param.AddProperty(AccountHolder.AccountHolderId, "AccountHolderID");
            param.AddProperty(BookMakerAccount.BookMakerAccountID, "BookMakerAccountID");
            param.AddProperty(UserName, nameof(UserName));
            param.AddProperty(Password, nameof(Password));
        }

        public override Task SetForeignKeys()
        {
            _accountHolder = GetFK(_accountHolder);
            _bookMakerAccount.BookMakerName = GetFK(BookMakerAccount).BookMakerName;
            return Task.CompletedTask;
        }
        public override void SetPrimaryKey(long id)=> AccountHolderBookMakerAccountId = id;
    }
}