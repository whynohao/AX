//plsMonthChangeRateVcl.js
stkScrewShopFinalQualityRptVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);

    this.summaryRenderer.TotalRateFun = function (v, sd, f) {
        var idx = '';
        var regx = /^[0-9]*$/;
        for (var i = this.dataIndex.length - 1; i >= 0; i--) {
            var v = this.dataIndex[i];
            if (regx.test(v)) {
                if (idx.length == 0)
                    idx = v;
                else
                    idx = Ext.String.insert(idx, v, 0);
            }
            else
                break;
        }
        if (sd.record.data['TARGETVALUE' + idx] != 0) {
            var value = sd.record.data['ACTUALVALUE' + idx] / sd.record.data['TARGETVALUE' + idx];
            if (value >= 0) {
                return '<span style="color:blue;">' + (value * 100).toFixed(2) + '%</span>';
            }
            else {
                return '<span style="color:red;">' + (value * 100).toFixed(2) + '%</span>';
            }
        }
        else
            return '<span style="color:blue;">' + (0).toFixed(2) + '%</span>';
    }
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
}
var proto = stkScrewShopFinalQualityRptVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = stkScrewShopFinalQualityRptVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataBase.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            for (var i = 1; i < 32; i++)
            {
                if (e.dataInfo.fieldName == ('TARGETVALUE' + i.toString())) {
                    var oldtarget = e.dataInfo.oldValue;
                    var target = e.dataInfo.value;
                    var actual = e.dataInfo.dataRow.get('ACTUALVALUE' + i.toString());
                    var sumtarget = e.dataInfo.dataRow.get('TARGETVALUE32');
                    var sumactual = e.dataInfo.dataRow.get('ACTUALVALUE32');
                    if (target != 0) {
                        e.dataInfo.dataRow.set('ACHIEVINGRATE' + i.toString(), parseFloat(actual / target).toFixed(4));
                        e.dataInfo.dataRow.set('TARGETVALUE32', sumtarget - oldtarget + target);
                        if (sumtarget - oldtarget + target != 0) {
                            e.dataInfo.dataRow.set('ACHIEVINGRATE32', parseFloat(sumactual / (sumtarget - oldtarget + target)).toFixed(4));
                        }
                        else {
                            e.dataInfo.dataRow.set('ACHIEVINGRATE32', 0);
                        }
                    }
                    else {
                        e.dataInfo.dataRow.set('ACHIEVINGRATE' + i.toString(), 0);
                        e.dataInfo.dataRow.set('TARGETVALUE32', sumtarget - oldtarget + target);
                        if (sumtarget - oldtarget + target != 0) {
                            e.dataInfo.dataRow.set('ACHIEVINGRATE32', parseFloat(sumactual / (sumtarget - oldtarget + target)).toFixed(4));
                        }
                        else {
                            e.dataInfo.dataRow.set('ACHIEVINGRATE32', 0);
                        }
                    }
                }
                else if (e.dataInfo.fieldName == ('ACTUALVALUE' + i.toString()))
                {
                    var target = e.dataInfo.dataRow.get('TARGETVALUE' + i.toString());
                    var actual = e.dataInfo.value;
                    var oldactual = e.dataInfo.oldValue;
                    var sumtarget = e.dataInfo.dataRow.get('TARGETVALUE32');
                    var sumactual = e.dataInfo.dataRow.get('ACTUALVALUE32');
                    if (target != 0) {
                        e.dataInfo.dataRow.set('ACHIEVINGRATE' + i.toString(), parseFloat(actual / target).toFixed(4));
                        e.dataInfo.dataRow.set('ACTUALVALUE32', sumactual - oldactual + actual);
                        e.dataInfo.dataRow.set('ACHIEVINGRATE32', parseFloat((sumactual-oldactual+actual) / sumtarget).toFixed(4));
                    }
                    else {
                        e.dataInfo.dataRow.set('ACHIEVINGRATE' + i.toString(), 0);
                        e.dataInfo.dataRow.set('ACTUALVALUE32', sumactual - oldactual + actual);
                        e.dataInfo.dataRow.set('ACHIEVINGRATE32', parseFloat((sumactual - oldactual + actual) / sumtarget).toFixed(4));
                    }
                }
            }
            break;
    }
};

