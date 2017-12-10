using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Data.OleDb;
using System.IO;
using Microsoft.Office;
using AxCRL.Comm.Utils;

namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.ToExcel", ProgIdType = ProgIdType.Bcf)]
    public class AxpToExcelBcf : LibBcfGrid, ILibLiveUpdate
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpToExcelBcfTemplate("axp.ToExcel");
        }
        Microsoft.Office.Interop.Excel.Range range;
        [LibBusinessTask(Name = "LiveUpdate", DisplayText = "实时更新")]
        public DataSet LiveUpdate()
        {
            #region【人员】
            DataSet dataSet = new DataSet();

            string selectStr = @"SELECT * FROM COMPERSON ";
            dataSet = this.DataAccess.ExecuteDataSet(selectStr);
            DataTable explortTable = new DataTable("人员");
            explortTable.Columns.Add("人员代码");
            explortTable.Columns.Add("人员名称");
            explortTable.Columns.Add("职位");
            explortTable.Columns.Add("性别");
            explortTable.Columns.Add("部门");
            explortTable.Columns.Add("部门名称");
            explortTable.Columns.Add("邮箱");
            explortTable.Columns.Add("手机");

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                DataRow dataRow = explortTable.NewRow();
                dataRow["人员代码"] = row[0];
                dataRow["人员名称"] = row[1];
                dataRow["职位"] = row[2];
                dataRow["性别"] = row[3];
                dataRow["部门"] = row[4];
                dataRow["部门名称"] = row[5];
                dataRow["邮箱"] = row[6];
                dataRow["手机"] = row[7];
                explortTable.Rows.Add(dataRow);
            }
            DataSet explortDataSet = new DataSet();
            explortDataSet.Tables.Add(explortTable);
            string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output");
            //string filePath = @"./a6MiOu/AxPath/Output";
            DataSetToExcel(explortDataSet, path);
            #endregion


            #region【系统账户】
            DataSet dataSet1 = new DataSet();

            string selectStr1 = @"SELECT * FROM AXPUSER ";
            dataSet1 = this.DataAccess.ExecuteDataSet(selectStr1);
            DataTable explortTable1 = new DataTable("AXPUSER");
            explortTable1.Columns.Add("用户账号");
            explortTable1.Columns.Add("用户密码");
            explortTable1.Columns.Add("人员代码");
            explortTable1.Columns.Add("人员名称");
            explortTable1.Columns.Add("角色");
            explortTable1.Columns.Add("角色名称");
            explortTable1.Columns.Add("启用");
            explortTable1.Columns.Add("壁纸");
            explortTable1.Columns.Add("充满桌面");

            foreach (DataRow row in dataSet1.Tables[0].Rows)
            {
                DataRow dataRow = explortTable1.NewRow();
                dataRow["用户账号"] = row[0];
                dataRow["用户密码"] = row[1];
                dataRow["人员代码"] = row[2];
                dataRow["角色"] = row[3];
                dataRow["启用"] = row[4];
                dataRow["壁纸"] = row[5];
                dataRow["充满桌面"] = row[6];
                explortTable1.Rows.Add(dataRow);
            }
            DataSet explortDataSet1 = new DataSet();
            explortDataSet1.Tables.Add(explortTable1);
            string path1 = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output");
            //string filePath = @"./a6MiOu/AxPath/Output";
            DataSetToExcel(explortDataSet1, path1);
            #endregion
            return this.DataSet;
        }
        public bool DataSetToExcel(DataSet ds, string FilePath)
        {
            //建立Excel对象 
            //progressBar.Value = 0;
            //progressBar.Maximum = ds.Tables.Count;
            foreach (DataTable dt in ds.Tables)
            {
                try
                {
                    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                    Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                    Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];
                    worksheet.Rows.RowHeight = 20;
                    worksheet.Columns.ColumnWidth = 20;
                    worksheet.Name = dt.TableName;
                    excel.Visible = false;
                    excel.DisplayAlerts = false;
                    excel.AlertBeforeOverwriting = false;
                    int rowNumber = dt.Rows.Count;//不包括字段名 
                    int columnNumber = dt.Columns.Count;
                    int colIndex = 0;


                    //生成字段名称 
                    foreach (DataColumn col in dt.Columns)
                    {
                        colIndex++;
                        excel.Cells[1, colIndex] = col.ColumnName;
                    }

                    object[,] objData = new object[rowNumber, columnNumber];

                    for (int r = 0; r < rowNumber; r++)
                    {
                        for (int c = 0; c < columnNumber; c++)
                        {
                            objData[r, c] = dt.Rows[r][c];
                        }
                        //Application.DoEvents(); 
                    }

                    // 写入Excel 
                    range = excel.Range[excel.Cells[2, 1], excel.Cells[rowNumber + 1, columnNumber]];
                    range.NumberFormat = "0";//设置单元格为文本格式 
                    range.Value2 = objData;
                    string path = string.Format(FilePath + @"\{0}.xlsx", dt.TableName);
                    try
                    {

                        FileInfo fileInfo = new FileInfo(path);
                        if (!fileInfo.Exists)
                        {
                            workbook.Saved = true;
                            workbook.SaveAs(path);
                            excel.UserControl = false;
                        }
                        else
                        {
                            workbook.Saved = true;
                            workbook.SaveCopyAs(path);//保存
                            excel.UserControl = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Common.RecordError("ToExcelSave--" + ex.Message);
                    }
                    finally
                    {
                        //, Missing.Value, Missing.Value
                        workbook.Close(Microsoft.Office.Interop.Excel.XlSaveAction.xlSaveChanges);
                        excel.Quit();
                    }
                    //progressBar.Value += 1;

                }
                catch (Exception ex)
                {
                    //Common.RecordError("DataSetToExcel------" + ex.Message);
                }

            }
            return true;
        }
    }

    public class AxpToExcelBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPTOEXCEL";

        public AxpToExcelBcfTemplate(string progId)
            : base(progId, BillType.Grid, "人员导出Excel")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXCEL", "Excel", FieldSize.Size50));
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["EXCEL"] };
            this.DataSet.Tables.Add(masterTable);
        }



        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.GridRange = layout.BuildGrid(0, string.Empty, null, true);
            this.ViewTemplate = new LibGridTpl(this.DataSet, layout);
        }
    }
}
