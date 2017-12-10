using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Data
{
    public class LibDBTransaction
    {
        private int _Count = 0;
        private DbTransaction _SqlTransaction = null;
        private bool _Running = false;
        private DbConnection _CurrDbConnection = null;

        public bool Running
        {
            get { return _Running; }
        }

        public DbTransaction SqlTransaction
        {
            get { return _SqlTransaction; }
            set { _SqlTransaction = value; }
        }

        public void BeginTransaction(DbConnection conn)
        {
            if (_Count == 0)
            {
                _SqlTransaction = conn.BeginTransaction();
                _CurrDbConnection = conn;
                _Running = true;
            }
            _Count++;
        }

        public void Commit()
        {
            _Count--;
            if (_Count == 0)
            {
                _SqlTransaction.Commit();
                _Running = false;
                if (_CurrDbConnection != null)
                {
                    _CurrDbConnection.Close();
                    _CurrDbConnection = null;
                }
            }
        }

        public void Rollback()
        {
            _Count--;
            if (_Count == 0)
            {
                _SqlTransaction.Rollback();
                _Running = false;
                if (_CurrDbConnection != null)
                {
                    _CurrDbConnection.Close();
                    _CurrDbConnection = null;
                }
            }
        }
    }
}
