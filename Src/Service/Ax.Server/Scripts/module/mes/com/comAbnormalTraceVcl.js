comAbnormalTraceVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comAbnormalTraceVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comAbnormalTraceVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterTable = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                Ext.Msg.alert("提示", "请使用“添加处理意见”按钮来添加处理意见！");
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                Ext.Msg.alert("提示", "请使用“添加处理意见”按钮来添加处理意见！");
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.FormClosed:
            var billNo = masterTable.data.items[0].get("BILLNO");
            this.invorkBcf("TransferProcessMsg", [this.list, billNo]);
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "btnBuildOpinion") {
                if (this.isEdit) {
                    var masterRow = masterTable.data.items[0];
                    var subTableRows = this.dataSet.getTable(1).data.items;
                    var dealwithOpinion = "";
                    var subRowIndex = -1;
                    var personId = Ext.util.Cookies.get('loginPersonId');
                    var personName = Ext.util.Cookies.get('loginPersonName');
                    var vcl = this;
                    for(var i = 0; i < subTableRows.length; i++){
                        if(personId == subTableRows[i].get("PERSONID")){
                            dealwithOpinion = subTableRows[i].get("DEALWITHOPINION");
                            subRowIndex = i;
                            break;
                        }
                    }
                    Ext.MessageBox.buttonText.yes = "处理完成";
                    Ext.MessageBox.buttonText.no = "处理未完成";
                    Ext.MessageBox.buttonText.cancel = "特殊处理";
                    Ext.MessageBox.show({
                        title: "添加处理意见界面",
                        modal: false,
                        value: dealwithOpinion,
                        msg: "处理意见",
                        buttons: Ext.MessageBox.YESNOCANCEL,
                        closable:true,
                        prompt:true,
                        multiline:true,
                        progress:false,
                        wait:false,
                        width:500,
                        fn: function (btn, text) {
                            if (!text) {
                                Ext.Msg.alert("提示", "处理意见不可为空！");
                                return;
                            }
                            if (subRowIndex == -1) {
                                var newRow = vcl.addRow(masterRow, 1);
                                newRow.set("PERSONID", personId);//填充数据
                                newRow.set("PERSONNAME", personName);//填充数据
                                newRow.set("DEALWITHOPINION", text);//填充数据
                                vcl.forms[1].loadRecord(newRow);
                                vcl.forms[1].updateRecord(newRow);
                            } else {
                                subTableRows[i].set("DEALWITHOPINION", text);//填充数据
                                vcl.forms[1].loadRecord(subTableRows[subRowIndex]);
                                vcl.forms[1].updateRecord(vcl.dataSet.getTable(1).data.items[subRowIndex]);
                            }

                            if (btn == "no") {
                                masterRow.set("DEALWITHSTATE", 1);
                                vcl.list = [];
                            } else if (btn == "yes") {
                                masterRow.set("DEALWITHSTATE", 2);
                                vcl.list = [];
                            } else if (btn == "cancel") {
                                masterRow.set("DEALWITHSTATE", 1);
                                vcl.list = [];
                            }
                            vcl.forms[0].loadRecord(masterRow);
                            vcl.forms[0].updateRecord(masterRow);
                            
                            if (btn == "cancel") {
                                var typeId = masterRow.get("TYPEID");
                                var typeName = masterRow.get("TYPENAME");
                                var processLevel = masterRow.get("PROCESSLEVEL");
                                Ax.utils.LibVclSystemUtils.openDataFunc("com.AbnormalTraceFunc", "特殊处理人员界面", [vcl, typeId, typeName, processLevel]);
                            }
                        }
                    });
                }
                else {
                    Ext.Msg.alert("系统提示", "只能在编辑状态下操作此功能！");
                }
            }
            break;
    }
}