/// <reference path="LibVclDataBase.js" />

Ax.vcl.LibVclDataFunc = function () {
    Ax.vcl.LibVclDataBase.apply(this, arguments);
    this.currentPk = [];
    this.needRemindForClose = false;
    this.latestPk = new Ext.util.MixedCollection();
    this.isEdit = true;
};
var proto = Ax.vcl.LibVclDataFunc.prototype = Object.create(Ax.vcl.LibVclDataBase.prototype);
proto.constructor = Ax.vcl.LibVclDataFunc;


proto.openFunc = function () {
    var data = this.invorkBcf("OpenFunc", []);
    this.setDataSet(data, false);
    if (this.dataSet.getTable(0).data.items.length == 1) {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        for (var i = 0; i < this.forms.length; i++) {
            this.forms[i].loadRecord(masterRow);
        };
        var pks = this.tpl.Tables[this.dataSet.getTable(0).Name].Pk
        for (var i = 0; i < pks.length; i++) {
            this.latestPk.add(pks[i], this.dataSet.getTable(0).data.items[0].get(pks[i]));
        }
    }
};
