/// <reference path="LibVclDataBase.js" />

Ax.vcl.LibVclVisual = function () {
    Ax.vcl.LibVclBase.apply(this, arguments);
    this.title = '';
    this.queryCondition = {};
    this.frameId = Ext.id();;
};
var proto = Ax.vcl.LibVclVisual.prototype = Object.create(Ax.vcl.LibVclBase.prototype);
proto.constructor = Ax.vcl.LibVclVisual;

proto.showData = function () {
    var src = 'http://' + DesktopApp.visualHost + '/Chart/View.html?code=' + this.progId + '&Title=' + escape(this.title) + '&FType=' + Ext.encode(this.queryCondition);
    Ext.get(this.frameId).dom.src = src;
}

proto.formCallBackHandler = function (tag, param) {
    if (tag == "SYSTEM_QUERY") {
        this.queryCondition = { QueryFields: param.condition };
        this.showData();
    }
};