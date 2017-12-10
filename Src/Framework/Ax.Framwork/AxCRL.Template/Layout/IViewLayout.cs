using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.Layout
{
    public interface IViewLayout
    {
        string Name { get; }

        string SchemeName { get; set; }

        Dictionary<string, string> GetButtonList();
    }


    public static class LibViewLayoutBuilder
    {
        public static LibControlLayoutBlock BuildControlGroup(DataSet dataSet, int tableIndex, string displayName, IList<string> fieldList)
        {
            return new LibControlLayoutBlock(dataSet, tableIndex, displayName, fieldList);
        }

        public static LibGridLayoutBlock BuildGrid(DataSet dataSet, int tableIndex, string displayName, IList<string> fieldList = null, bool addAutoRowNo = false)
        {
            if (fieldList == null)
            {
                DataTable table = dataSet.Tables[tableIndex];
                fieldList = new List<string>();
                foreach (DataColumn item in table.Columns)
                {
                    fieldList.Add(item.ColumnName);
                }
            }
            LibGridLayoutBlock block = new LibGridLayoutBlock(dataSet, tableIndex, displayName, addAutoRowNo);
            block.FieldList = fieldList;
            return block;
        }

        public static LibBandGridLayoutBlock BuildBandGrid(DataSet dataSet, int tableIndex, string displayName, IList<BandColumn> bandColumn, bool addAutoRowNo = false)
        {
            LibBandGridLayoutBlock block = new LibBandGridLayoutBlock(dataSet, tableIndex, displayName, addAutoRowNo);
            block.BandColumn = bandColumn;
            return block;
        }

        public static LibFuncLayoutBlock BuildButton(IList<FunButton> buttons)
        {
            LibFuncLayoutBlock block = new LibFuncLayoutBlock();
            block.Buttons = buttons;
            return block;
        }

        public static Dictionary<string, string> GetButtonList(LibLayoutBlock buttonRange)
        {
            Dictionary<string, string> ret = null;
            if (buttonRange != null)
            {
                LibFuncLayoutBlock block = buttonRange as LibFuncLayoutBlock;
                if (block != null && block.Buttons.Count > 0)
                {
                    ret = new Dictionary<string, string>();
                    foreach (var item in block.Buttons)
                    {
                        FindButton(item, ret);
                    }
                }
            }
            return ret;
        }

        private static void FindButton(FunButton button, Dictionary<string, string> dic)
        {
            if (button.FunButtonList.Count == 0)
            {
                if (!dic.ContainsKey(button.Name))
                    dic.Add(button.Name, button.DisplayText);
            }
            else
            {
                foreach (var item in button.FunButtonList)
                {
                    FindButton(item, dic);
                }
            }
        }
    }

}
