
plsInformationSupportingVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsInformationSupportingVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsInformationSupportingVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        this.forms[0].updateRecord(masterRow); 
        var matingId = masterRow.get('MATINGID');
        var table = this.dataSet.getTable(1);
        if (e.dataInfo.fieldName == "btnLoad") { 
            var affectDays = masterRow.get('AFFECTDAYS');
            if (affectDays > 0) { 
                var data = this.invorkBcf('GetMatingPlanData', [affectDays, matingId]);
                table.removeAll();
                for (var i = 0; i < data.length; i++) {
                    table.add(data[i]);
                }
            }
            else {
                alert("信息配套影响天数不能小于1。");
            }

        } else if (e.dataInfo.fieldName == "btnBuild") {
            var warehouserange = masterRow.get('WAREHOUSERANGE');
            //if (warehouserange == "") {
            //    alert("仓库范围不能为空。");
            //}
            //else {
                var MatingInfo = {};
               // MatingInfo.MatingId = masterRow.get("MATINGID");
                //MatingInfo.Data = [];
                var BuildMatingInfo = [];
                if (table.data.items.length > 0) { 
                    for (var i = 0; i < table.data.items.length; i++) { 
                         var record = table.data.items[i]; 
                         BuildMatingInfo.push({
                             WORKNO: record.get("WORKNO"),
                             WORKSHOPSECTIONID: record.get("WORKSHOPSECTIONID"),
                             MATINGID: record.get("MATINGID"),
                             ROWID: record.get("ROW_ID"),
                             QUANTITY: record.get("QUANTITY"),
                             BOMID: record.get("BOMID")
                         })
                    }  
                    this.invorkBcf('BuildMatingPlan', [warehouserange, BuildMatingInfo])
                }
           // }
        }
    }
}