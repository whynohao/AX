comScheduleAttributeRuleVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    this.dataRow;
}

var attId = 0;
var proto = comScheduleAttributeRuleVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comScheduleAttributeRuleVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
        if (e.dataInfo.fieldName == "ATTRIBUTEITEMNAME") {
            var AttrId = e.dataInfo.dataRow.data["ATTRIBUTEITEMID"];
            if (AttrId != "") {
                var returnData = this.invorkBcf('GetAttItem', [AttrId]);
                if (this.billAction == BillActionEnum.Modif || this.billAction == BillActionEnum.AddNew) {

                    this.AttForm(returnData, this, e);
                }
            }
        }
    }

}


//最新特征窗体
proto.AttForm = function(returnData, This, e) {
    var Attitem = [];
    var RuleId = e.dataInfo.dataRow.data["ATTRIBUTERULEID"];
    var Row_Id = e.dataInfo.dataRow.data["ROW_ID"];
    //确认按钮
    var btnAttConfirm = new Ext.Button({
        width: 50,
        height: 25,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attWin" + e.dataInfo.dataRow.data["ATTRIBUTERULEID"] + e.dataInfo.dataRow.data["ROW_ID"]);
            var comBox = Ext.getCmp("attCom" + e.dataInfo.dataRow.data["ATTRIBUTERULEID"] + e.dataInfo.dataRow.data["ROW_ID"]);

            if (comBox.value == null) {
                Ext.Msg.alert("提示", '请选择特征项值！');
                return;
            }
            var attId = comBox.value.split('_')[0];
            var rowId = comBox.value.split('_')[1];
            var attValue = comBox.rawValue;
            e.dataInfo.dataRow.set("ATTRCODE", attId);
            e.dataInfo.dataRow.set("ATTRITEMROWID", rowId);
            e.dataInfo.dataRow.set("ATTRVALUE", attValue);
            if (yes) {
                thisWin.close();
            }

        } 
    })
    //取消按钮
    var btnAttCancel = new Ext.Button({
        width: 50,
        height: 25,
        text: "取消",
        type: 'submit',
        handler: function () {
            Ext.getCmp("attWin" + RuleId + Row_Id).close();
        }
    })
    //按钮Panle
    var btnAttPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        defaults: {
            margin: '0 50 0 50',
            columnWidth: .5
        },
        items: [btnAttConfirm, btnAttCancel]
    })

    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attWin" + RuleId + Row_Id,
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 340,
        height: 130,
        autoScroll: true,
        layout: 'column',
        items: [{
            id: 'Att' + attId,
            layout: 'column',
            xtype: 'fieldset',
            width: '96%',
            height: 45,

            defaulType: 'combobox',
            margin: '5 10 5 10',
            autoScroll: true,
            items: This.CreatAttComBox(returnData, e),
        }, btnAttPanel],
    });
    attId++;
    Salewin.show();
}

proto.CreatAttComBox = function (attData, e) {
    var attlist = [];
    var defaultValue;
    if (e.dataInfo.dataRow.data["ATTRCODE"] != '') {
        defaultValue = e.dataInfo.dataRow.data["ATTRCODE"] + '_' + e.dataInfo.dataRow.data["ATTRITEMROWID"];
    }
    else {
        defaultValue = attData[0]['AttrCode'];
    }
    for (var i = 0; i < attData.length; i++) {
        var data = { AttrCode: attData[i]['AttrCode'], AttrValue: attData[i]['AttrValue'], RowId: attData[i]['RowId'] };
        attlist.push(data);
    }
    var Store = Ext.create("Ext.data.Store", {
        fields: ["AttrCode", "AttrValue", "RowId"],
        data: attlist
    });
    var combox = new Ext.form.ComboBox({
        id: "attCom" + e.dataInfo.dataRow.data["ATTRIBUTERULEID"] + e.dataInfo.dataRow.data["ROW_ID"],
        mode: 'local',
        forceSelection: true,
        triggerAction: 'all',
        displayField: 'AttrValue',
        fieldLabel: e.dataInfo.dataRow.data["ATTRIBUTEITEMNAME"],
        AttrCode: attData[0]['AttrCode'],//特征项ID
        AttrValue: attData[0]['AttrValue'],//特征项的值
        RowId: attData[0]['RowId'],//特征项RowId
        valueField: 'AttrCode',
        value: defaultValue,//特征项的值
        fields: ["AttrCode", "AttrValue", "RowId"],
        store: Store,

        //editable: true,
        //text: attData.DefaultValue,
        margin: '5 5 0 5',
        columnWidth: .5,
        //labelWidth: 60,
    });
    return combox;
}