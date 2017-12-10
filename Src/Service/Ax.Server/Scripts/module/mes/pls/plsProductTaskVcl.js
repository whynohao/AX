plsProductTaskVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = plsProductTaskVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsProductTaskVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "CreatPPWork") {
                var billNo = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                var result = this.invorkBcf("AddPpWorkOrder", [billNo]);
                if (result.MessageList.length > 0) {
                    var ex = [];
                    for (var i = 0; i < result.MessageList.length; i++) {
                        var msgKind = result.MessageList[i].MessageKind;
                        ex.push({ kind: msgKind, msg: result.MessageList[i].Message });
                    }
                    Ax.utils.LibMsg.show(ex);
                }
            }
            break;
    }
}

