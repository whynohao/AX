wsWriteBarcodeVcl = function () {
    Ax.sfl.vcl.WsGatherVcl.apply(this, arguments);
};
var proto = wsWriteBarcodeVcl.prototype = Object.create(Ax.sfl.vcl.WsGatherVcl.prototype);
proto.constructor = wsWriteBarcodeVcl;