using AxCRL.Bcf;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES_Com.MasterDataBcf
{
    public static class CodingRuleHelper
    {
        public static void CheckRuleData(LibBcfData bcf)
        {
            bool isCodingRule = bcf.ProgId == "com.CodingRule";
            string totalLenName = isCodingRule ? "CODINGRULELENGTH" : "BARCODELENGTH";
            DataRow masterRow = bcf.DataSet.Tables[0].Rows[0];
            int barcodeLength = 0;
            bool hasDy = false;
            bool hasSerialNum = false;
            int maxRowNo = 0;
            int serialNumRowNo = 0;
            foreach (DataRow curRow in bcf.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                int rowNo = LibSysUtils.ToInt32(curRow["ROWNO"]);
                if (maxRowNo < rowNo) maxRowNo = rowNo;
                int sectionLength = LibSysUtils.ToInt32(curRow["SECTIONLENGTH"]);
                DataRow[] childRows = curRow.GetChildRows(bcf.DataSet.Relations[1]);
                bool hasDetail = false;
                BarcodeRuleSectionType type = (BarcodeRuleSectionType)LibSysUtils.ToInt32(curRow["SECTIONTYPE"]);
                if (type == BarcodeRuleSectionType.Dynamic)
                {
                    if (!hasDy) hasDy = true;
                    if (string.IsNullOrEmpty(LibSysUtils.ToString(curRow["FIELDNAME"])))
                        bcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("行{0}设定为动态段,字段名不能为空。", rowNo));
                    foreach (DataRow subRow in childRows)
                    {
                        if (subRow.RowState == DataRowState.Deleted)
                            continue;
                        if (!hasDetail) hasDetail = true;
                        int sectionValueLen = LibSysUtils.ToString(subRow["SECTIONVALUE"]).Length;
                        if (sectionLength != sectionValueLen)
                        {
                            bcf.ManagerMessage.AddMessage(LibMessageKind.Error,
                                string.Format("行{0}的编码段长度为{1},子行{2}的编码值长度为{3}。长度须相等。",
                                rowNo, sectionLength, LibSysUtils.ToInt32(subRow["ROWNO"]), sectionValueLen));
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(LibSysUtils.ToString(curRow["FIELDNAME"])))
                        bcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("行{0}非动态段,字段名不能有值。", rowNo));
                    if (LibSysUtils.ToBoolean(curRow["DYRULEDETAIL"]))
                        bcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("行{0}非动态段,不能有动态规则明细。", rowNo));
                }
                switch (type)
                {
                    case BarcodeRuleSectionType.SerialNum:
                        if (hasSerialNum)
                            bcf.ManagerMessage.AddMessage(LibMessageKind.Error, "编码规则明细里不能同时设置多笔流水号。");
                        else
                            hasSerialNum = true;
                        serialNumRowNo = rowNo;
                        if (sectionLength < 3)
                            bcf.ManagerMessage.AddMessage(LibMessageKind.Warn, "流水码的编码长度建议最少大于3");
                        break;
                    case BarcodeRuleSectionType.DateL:
                        if (sectionLength != 8)
                            curRow["SECTIONLENGTH"] = 8;
                        break;
                    case BarcodeRuleSectionType.DateL1:
                        if (sectionLength != 8)
                            curRow["SECTIONLENGTH"] = 4;
                        break;
                    case BarcodeRuleSectionType.DateS:
                    case BarcodeRuleSectionType.DateS1:
                    case BarcodeRuleSectionType.DateAB:
                        if (sectionLength != 6)
                            curRow["SECTIONLENGTH"] = 6;
                        break;
                    case BarcodeRuleSectionType.DateL16:
                        if (sectionLength != 7)
                            curRow["SECTIONLENGTH"] = 7;
                        break;
                    case BarcodeRuleSectionType.DateS16:
                        if (sectionLength != 5)
                            curRow["SECTIONLENGTH"] = 5;
                        break;
                    default:
                        break;
                }
                if (LibSysUtils.ToBoolean(curRow["DYRULEDETAIL"]) != hasDetail)
                    curRow["DYRULEDETAIL"] = hasDetail;
                barcodeLength += LibSysUtils.ToInt32(curRow["SECTIONLENGTH"]);
            }
            if (!hasSerialNum)
                bcf.ManagerMessage.AddMessage(LibMessageKind.Error, "编码规则明细里需存在一笔流水号。");
            else if (serialNumRowNo != maxRowNo)
                bcf.ManagerMessage.AddMessage(LibMessageKind.Error, "编码规则明细里的流水号必须是最后一笔数据。");
            if (hasDy && string.IsNullOrEmpty(LibSysUtils.ToString(masterRow["PROGID"])))
            {
                bcf.ManagerMessage.AddMessage(LibMessageKind.Error, "编码规则明细存在动态段,表头功能字段不能为空。");
            }
            if (LibSysUtils.ToInt32(masterRow[totalLenName]) != barcodeLength)
            {
                masterRow[totalLenName] = barcodeLength;
            }
        }
    }
}
