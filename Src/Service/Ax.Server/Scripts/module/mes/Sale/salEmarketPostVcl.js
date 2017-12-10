salEmarketPostVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
}

var proto = salEmarketPostVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = salEmarketPostVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:
            var guid = this.invorkBcf('GetGuid', []);
            e.dataInfo.dataRow.set("TASKID", guid);
            break;
    }
}