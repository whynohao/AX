using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    public static class DataTableHelper
    {
        public static void FillTableData(DataTable table, IDataReader reader)
        {
            table.BeginLoadData();
            try
            {
                while (reader.Read())
                {
                    DataRow newRow = table.NewRow();
                    newRow.BeginEdit();
                    try
                    {
                        int count = reader.FieldCount;
                        for (int i = 0; i < count; i++)
                        {
                            string name = reader.GetName(i);
                            if (table.Columns.Contains(name))
                            {
                                if (!Convert.IsDBNull(reader[i]))
                                    newRow[name] = reader[i];
                            }
                        }
                    }
                    finally
                    {
                        newRow.EndEdit();
                    }
                    table.Rows.Add(newRow);
                }
            }
            finally
            {
                table.EndLoadData();
            }
        }
    }
}
