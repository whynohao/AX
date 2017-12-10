/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />

Ext.ns('Ax.vcl');

var LibEventTypeEnum = {
    Validating: 0,
    Validated: 1,
    CheckField: 2,
    ColumnDbClick: 3,
    FormClosing: 4,
    FormClosed: 5,
    ButtonClick: 6,
    AddRow: 7,
    DeleteRow: 8,
    BeforeAddRow: 9,
    BeforeDeleteRow: 10,
    ConfirmAttribute: 11,
    DblClick: 12
};

Ax.vcl.LibEventArgs = function (libEventType, dataInfo) {
    this.libEventType = libEventType;
    this.dataInfo = dataInfo;
};
Ax.vcl.LibEventArgs.prototype = {
    constructor: Ax.vcl.LibEventArgs
};