
comAbnormalReportVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comAbnormalReportVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comAbnormalReportVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var mastertable = this.dataSet.getTable(0);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnBuildTrace") { 
            var AbnormalReport = {};
            var record = mastertable.data.items[0];
            AbnormalReport.BillNo = record.get("BILLNO");
            AbnormalReport.TypeId = record.get("TYPEID");
            AbnormalReport.CurrentState = record.get("CURRENTSTATE");
            AbnormalReport.PersonId = record.get("PERSONID");
            //生成异常追踪单
            var data = this.invorkBcf('BuildAbnormalTrace', [AbnormalReport]);

            if (data != null) {
                var curPks = [];
                curPks.push(data[0].BILLNO);

                var typeId = data[0].TYPEID;
                var typeName;

                switch (typeId) {
                    case "PLST1":
                        typeName = "生产计划异常追踪单";
                        break;
                    case "PPT1":
                        typeName = "生产过程异常追踪单";
                        break;
                    case "PURT1":
                        typeName = "采购过程异常追踪单";
                        break;
                    case "STKT1":
                        typeName = "仓储物流异常追踪单";
                        break;
                }

                var entryParam = '{"ParamStore":{"TYPEID":"' + typeId + '"}}';
                //打开已生成的异常追踪单单据浏览页面
                Ax.utils.LibVclSystemUtils.openBill('com.AbnormalTrace', 1, typeName, BillActionEnum.Edit, Ext.decode(entryParam), curPks);
            }
        }
    }


}