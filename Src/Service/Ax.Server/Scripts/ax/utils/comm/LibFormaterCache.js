Ext.ns('Ax.utils');

Ax.utils.LibFormaterCache = function () {
    this.unitStore = new Ext.util.MixedCollection();
    this.baseStore = new Ext.util.MixedCollection();
};

Ax.utils.LibFormaterCache.prototype = {
    constructor: Ax.utils.LibFormaterCache,
    getUnitData: function (unitId) {
        var me = this;
        if (!this.unitStore.containsKey(unitId)) {
            Ext.Ajax.request({
                url: 'billSvc/getFormatUnit?unitId=' + unitId,
                async: false,
                method: 'GET',
                success: function (response) {
                    var ret = Ext.decode(response.responseText);
                    me.unitStore.add(unitId, ret.GetFormatUnitResult);
                }
            });
        }
        return this.unitStore.get(unitId);
    },
    getCompanyData: function (companyId) {
        if (!companyId)
            companyId = '';
        var me = this;
        if (!this.baseStore.containsKey(companyId)) {
            Ext.Ajax.request({
                url: 'billSvc/getCompanyFormat?companyId=' + companyId,
                async: false,
                method: 'GET',
                success: function (response) {
                    var ret = Ext.decode(response.responseText);
                    me.baseStore.add(companyId, ret.GetCompanyFormatResult);
                }
            });
        }
        return this.baseStore.get(companyId);
    },
    getPriceData: function (companyId) {
        var ret = 0;
        var param = this.getCompanyData(companyId);
        if (param)
            ret = param.Price;
        return ret;
    },
    getAmountData: function (companyId) {
        var ret = 0;
        var param = this.getCompanyData(companyId);
        if (param)
            ret = param.Amount;
        return ret;
    },
    getTaxRateData: function (companyId) {
        var ret = 0;
        var param = this.getCompanyData(companyId);
        if (param)
            ret = param.TaxRate;
        return ret;
    }
};
