stkPurStockInDeatilRpt_AxceVcl = function () {
    Ax.vcl.LibVclRpt.apply(this, arguments);
};
var proto = stkPurStockInDeatilRpt_AxceVcl.prototype = Object.create(Ax.vcl.LibVclRpt.prototype);
proto.constructor = stkPurStockInDeatilRpt_AxceVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclRpt.prototype.vclHandler.apply(this, arguments);
    var bodyTable = this.dataSet.getTable(1);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            //if (e.dataInfo.fieldName == "DETAIL") {
            var billNo = e.dataInfo.dataRow.data["PURCHASEBILLNO"];
            var rowId = e.dataInfo.dataRow.data["PURCHASEROWID"];
            var material = e.dataInfo.dataRow.data["MATERIALID"] + "," + e.dataInfo.dataRow.data["MATERIALNAME"];
            var materialSpec = e.dataInfo.dataRow.data["MATERIALSPEC"];
            var figureNo = e.dataInfo.dataRow.data["FIGURENO"];
            var textureId = e.dataInfo.dataRow.data["TEXTUREID"];
            var quantity = e.dataInfo.dataRow.data["PURCHASEQTY"];
            var noQuantity = e.dataInfo.dataRow.data["NOPURCHASEQTY"];
            var list = this.invorkBcf("GetPurSockInDetail", [billNo, rowId]);
            if (list.length > 0) {
                //创建窗体
                var repeatPanel = FormPanel(2);
                repeatPanel.add(TextField('采购单号', 'purChaseOrder', billNo, true, 300, 1));
                repeatPanel.add(TextField('采购单行号', 'purChaseRowId', rowId, true, 300, 1));
                repeatPanel.add(TextField('物料', 'material', material, true, 300, 1));
                repeatPanel.add(TextField('规格', 'materialSpec', materialSpec, true, 300, 1));
                repeatPanel.add(TextField('图号', 'figureNo', figureNo, true, 300, 1));
                repeatPanel.add(TextField('标识', 'textureId', textureId, true, 300, 1));
                repeatPanel.add(NumberField('采购数量', 'quantity', quantity, true, true, 300, 1));
                repeatPanel.add(NumberField('待入库数', 'noQuantity', noQuantity, true, true, 300, 1));

                var bodyGrid = Grid(list);
                bodyGrid.columnManager.columns[0].hidden = true;
                bodyGrid.store.loadData(list);

                var winPanel = Window('win_PurChaseOrder', '采购订单明细', null, 750, 600);
                winPanel.add(repeatPanel);
                winPanel.add(bodyGrid);
                winPanel.show();
            }
            break;

    }
}

Window = function (id, title, bottonArray, width, hight) {
    var win = Ext.create('Ext.window.Window', {
        id: Ext.isEmpty(id) ? "win_print" : id,
        title: Ext.isEmpty(title) ? "" : title,
        resizable: false,
        modal: true,
        width: Ext.isEmpty(width) ? 420 : width,
        height: Ext.isEmpty(hight) ? 350 : hight,
        closeAction: 'destroy',
        items: [],
        buttons: bottonArray,
        bodyStyle: { overflowY: 'scroll' }

    });
    return win;
}

FormPanel = function (columns) {
    var formpanel = Ext.create("Ext.form.Panel", {
        width: '100%',
        layout: {
            type: 'table',//表格布局    
            columns: Ext.isEmpty(columns) ? 1 : columns,
        },
        defaults: {
            margin: '10 20 10 20',
            xtype: 'textfield',
            //width: 350,
            labelWidth: 80
        },
        items: []
    });
    return formpanel;
}
NumberField = function (labelname, id, value, readonly, allowNumic, width, coluNum) {
    var textfield = Ext.create("Ext.form.NumberField", {
        fieldLabel: Ext.isEmpty(labelname) ? "" : labelname,
        value: Ext.isEmpty(value) ? 0 : value,
        id: Ext.isEmpty(id) ? "" : id,
        name: Ext.isEmpty(id) ? "" : id,
        width: Ext.isEmpty(width) ? 350 : width,
        colspan: Ext.isEmpty(coluNum) ? 1 : coluNum,
        readOnly: Ext.isEmpty(readonly) ? false : readonly,
        allowBlank: false,
        allowDecimals: Ext.isEmpty(allowNumic) ? true : allowNumic,
        minValue: 0
    });
    return textfield;
}
TextField = function (labelname, id, value, readonly, width, coluNum, hidden) {
    var textfield = Ext.create("Ext.form.TextField", {
        fieldLabel: Ext.isEmpty(labelname) ? "" : labelname,
        value: Ext.isEmpty(value) ? "" : value,
        id: Ext.isEmpty(id) ? "" : id,
        name: Ext.isEmpty(id) ? "" : id,
        width: Ext.isEmpty(width) ? 350 : width,
        colspan: Ext.isEmpty(coluNum) ? 1 : coluNum,
        readOnly: Ext.isEmpty(readonly) ? false : readonly,
        hidden: Ext.isEmpty(hidden) ? false : hidden,
        // allowBlank: false
    });
    return textfield;
}
gridStore = function (Data) {
    return Ext.create("Ext.data.Store", {
        fields: [{ name: 'BillNo', type: 'string' }, { name: 'Quantity', type: 'int' },
        { name: 'BatchNo', type: 'string' }, { name: 'SubBatchNo', type: 'string' }],
        data: Data
    });
}
Grid = function (girdData) {
    var grid = Ext.create("Ext.grid.Panel", {
        xtype: "grid",
        store: gridStore(girdData),
        //width: 600,
        flex: true,
        height: 300,
        margin: '10 0 10 0',
        columnLines: true,
        renderTo: Ext.getBody(),
        selModel: {
            injectCheckbox: 0,
            mode: "SIMPLE",     //"SINGLE"/"SIMPLE"/"MULTI"
            checkOnly: true     //只能通过checkbox选择
        },
        selType: "checkboxmodel",
        columns: [
            { text: '入库单号', dataIndex: 'BillNo' },
            { text: '数量', dataIndex: 'Quantity', xtype: 'numbercolumn', format: '0' },
            { text: '批号', dataIndex: 'BatchNo' },
            { text: '小批号', dataIndex: 'SubBatchNo' }
        ]
    });
    return grid;
}