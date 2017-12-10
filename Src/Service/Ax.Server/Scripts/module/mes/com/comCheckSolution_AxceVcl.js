comCheckSolution_AxceVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comCheckSolution_AxceVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comCheckSolution_AxceVcl;

proto.vclHandler = function(sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //自定义按钮
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "BtnSearch") {
            if (this.isEdit) {
                var checkName = this.dataSet.getTable(0).data.items[0].data['CHECKSTNAME']; //获取检测方案名称
                var figureNo = this.dataSet.getTable(0).data.items[0].data['FIGURENO'];
                var workProcessId = this.dataSet.getTable(0).data.items[0].data['WORKPROCESSID'];
                var workProcessName = this.dataSet.getTable(0).data.items[0].data['WORKPROCESSNAME'];
                Ax.utils.LibVclSystemUtils.openDataFunc('com.CheckSolutionDataFunc', '选择检测方案模板', [this, checkName, figureNo, workProcessId, workProcessName]);
            }
            else {
                Ext.Msg.alert("系统提示", "编辑状态才能使用数据加载按钮！");
            }
        }
    }
    //if (e.libEventType == LibEventTypeEnum.Validated) {
    //    if (e.dataInfo && e.dataInfo.tableIndex == 0) {
    //        if (e.dataInfo.fieldName == "FIGURENO" || e.dataInfo.fieldName == "WORKPROCESSID") {
    //            var fNo = Ext.getCmp("FIGURENO0_" + this.winId).rawValue;
    //            var wName = e.dataInfo.dataRow.get("WORKPROCESSNAME");
    //            var masterRow = this.dataSet.getTable(0).data.items[0];
    //            masterRow.set("CHECKSTNAME", fNo + "/" + wName);
    //            this.forms[0].loadRecord(masterRow);
    //            //更新表头
    //        }
    //    }
    //}
}


