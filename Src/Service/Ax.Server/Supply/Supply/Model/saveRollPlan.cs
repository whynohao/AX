using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ax.Server.Supply.Model
{
    [Serializable]
    public class saveRollPlan
    {
        string billNo = string.Empty;

        public string BILLNO
        {
            get { return billNo; }
            set { billNo = value; }
        }
        int row_Id = 0;

        public int ROW_ID
        {
            get { return row_Id; }
            set { row_Id = value; }
        }
        string deliveryNoteNo = string.Empty;

        public string DELIVERYNOTENO
        {
            get { return deliveryNoteNo; }
            set { deliveryNoteNo = value; }
        }
        string barcode = string.Empty;

        public string BARCODE
        {
            get { return barcode; }
            set { barcode = value; }
        }
        Int64 arriveDate = 0;

        public Int64 ARRIVEDATE
        {
            get { return arriveDate; }
            set { arriveDate = value; }
        }
        int arriveQuantity = 0;

        public int ARRIVEQUANTITY
        {
            get { return arriveQuantity; }
            set { arriveQuantity = value; }
        }
    }
}