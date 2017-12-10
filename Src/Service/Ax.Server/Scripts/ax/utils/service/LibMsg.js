Ext.ns('Ax.utils');

var LibMessageKind = {
    Info: 0,
    Warn: 1,
    Error: 2,
    SysException: 3
}

Ax.utils.LibMsg = {
    show: function (info) {
        if (info === undefined || info.length === undefined || info.length == 0)
            return;
        var store = Ext.data.StoreManager.lookup('libMessageStore');
        if (store === undefined) {
            store = Ext.create('Ext.data.Store', {
                storeId: 'libMessageStore',
                fields: ['kind', 'msg'],
                proxy: {
                    type: 'memory',
                    reader: {
                        type: 'json'
                    }
                }
            });
        }
        store.loadData(info, false);
        var gridPanel = Ext.create('Ext.grid.Panel', {
            store: store,
            columns: [
                {
                    text: '类别',
                    dataIndex: 'kind',
                    renderer: function (val) {
                        switch (val) {
                            case 0:
                                return '<span style="color:' + '#0d99cf' + '">提示</span>';
                            case 1:
                                return '<span style="color:' + '#fbc105' + '">警告</span>';
                            case 2:
                                return '<span style="color:' + '#c44949' + '">错误</span>';
                            case 3:
                                return '<span style="color:' + '#c44949' + '">异常</span>';
                        }
                    }
                },
                {
                    text: '信息', dataIndex: 'msg', flex: 1,
                    renderer: function (value, meta, record) {
                        meta.style = 'white-space:normal;word-break:break-all;';
                        return value;
                    }
                }
            ]
        });
        var win = Ext.create('Ext.window.Window', {
            title: '信息窗',
            autoScroll: true,
            width: 500,
            height: 300,
            layout: { type: 'vbox', align: 'stretch' },
            constrainHeader: true,
            minimizable: false,
            maximizable: false,
            modal: true,
            items: [gridPanel]
        });
        win.show();
    }
};
