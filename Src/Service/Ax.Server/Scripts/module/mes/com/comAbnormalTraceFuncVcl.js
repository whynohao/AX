comAbnormalTraceFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comAbnormalTraceFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAbnormalTraceFuncVcl;

proto.doSetParam = function (vclObj) {
    proto.fromObj = vclObj[0];
    proto.winId = vclObj[0].winId;
    var typeId = vclObj[1];
    var typeName = vclObj[2];
    var processLevel = vclObj[3];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set('TYPEID', typeId);
    masterRow.set('TYPENAME', typeName);
    masterRow.set('PROCESSLEVEL', processLevel);
    this.forms[0].loadRecord(masterRow);
    //var rerurnList = This.invorkBcf("GetData", [typeId, processLevel]);
    //this.fillData(this, rerurnList);
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.FormClosed:
            var list = [];
            var bodyTable = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < bodyTable.length; i++) {
                list.push({
                    PersonId: bodyTable[i].data["PERSONID"],
                    PersonName: bodyTable[i].data["PERSONNAME"],
                    PhoneNo: bodyTable[i].data["PHONENO"],
                    WeChat: bodyTable[i].data["WECHAT"],
                    NeedSMS: bodyTable[i].data["NEEDSMS"],
                    SendWeChat: bodyTable[i].data["SENDWECHAT"]
                });
            }
            proto.fromObj.list = list;
            break;
    }
}
proto.fillData = function (vcl, list) {
    Ext.suspendLayouts();
    var curStore = vcl.dataSet.getTable(1);
    curStore.suspendEvents();
    try {
        vcl.dataSet.getTable(1).removeAll();
        var masterRow = vcl.dataSet.getTable(0).data.items[0];
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = vcl.addRow(masterRow, 1);
                newRow.set('PERSONID', info.PersonId);
                newRow.set('PERSONNAME', info.PersonName);
                newRow.set('POSITION', info.Position);
                newRow.set('PHONENO', info.PhoneNo);
                newRow.set('WECHAT', info.WeChat);
                newRow.set('NEEDSMS', info.NeedSMS);
                newRow.set('SENDWECHAT', info.SendWeChat);
            }
        }
    } finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }
}