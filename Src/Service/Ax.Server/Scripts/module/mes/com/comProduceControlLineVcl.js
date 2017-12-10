/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：CPS建模的产线模型模块的前端逻辑处理页面
 * 创建标识：Huangwz 2017/05/04
 * 逻辑说明：
 *
 ************************************************************************/
comProduceControlLineVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};

var proto = comProduceControlLineVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comProduceControlLineVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex === 1 || e.dataInfo.tableIndex === 2) {
                Ext.Msg.alert("提示", "请点击下方的【工厂建模】按钮添加数据！");
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex === 1 || e.dataInfo.tableIndex === 2) {
                Ext.Msg.alert("提示", "请点击下方的【工厂建模】按钮删除数据！");
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "RELID") {
                var progId = e.dataInfo.dataRow.get("PROGID");
                if (!progId) {
                    Ext.Msg.alert("提示", "请先选择功能标识！");
                } else {
                    var controlName = e.dataInfo.dataRow.get("CONTROLNAME").split('[')[0];
                    var relId = e.dataInfo.dataRow.get("RELID");
                    let curPks = relId ? [relId] : undefined
                    Ax.utils.LibVclSystemUtils.openBill(progId, BillTypeEnum.Master, controlName, BillActionEnum.Browse, undefined, curPks, undefined)
                }
            } else {
                Ext.Msg.alert("提示", "请点击下方的【工厂建模】按钮修改数据！");
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnCreateProduceControlLine") {
                if (this.isEdit) {
                    Ext.Msg.alert("系统提示", "非编辑状态才能进行 工厂建模！");
                } else {
                    var produceControlLineId = masterRow.get("PRODUCECONTROLLINEID")
                    var factoryModuleType = masterRow.get("FACTORYMODULETYPE")
                    CPSConfigureManage.CpsConfigureMain.jumpTo(produceControlLineId, factoryModuleType, this)
                    this.win.minimize()   // 最小化
                }
            }
            if (e.dataInfo.fieldName == "BtnShowProduceControlLine") {
                if (this.isEdit) {
                    Ext.Msg.alert("系统提示", "非编辑状态才能 预览！");
                } else {
                    var produceControlLineId = masterRow.get("PRODUCECONTROLLINEID");
                    CPSConfigureManage.CpsConfigureMain.loadCanvas(produceControlLineId);
                }
            }
            break;
    }
}
