
Ext.define('Ax.ux.form.LibDatecolumn', {
    extend: 'Ext.grid.column.Date',
    alias: 'widget.libDatecolumn',
    initComponent: function () {
        this.renderer = function (v, metaData, record, rowIndex, colIndex, store, view) {
            var axT = metaData.column.axT;
            if (v == undefined || v == 0 || isNaN(v)) {
                switch (axT) {
                    case 0:
                    case 1:
                        return;
                    case 2:
                        return "00:00";
                    case 3:
                        return "00:00:00";
                }
            }
            function pad(num, n) {
                var len = num.length;
                while (len < n) {
                    num = "0" + num;
                    len++;
                }
                return num;
            }
            v = v.toString();
            switch (axT) {
                case 0:
                    return (v.substring(0, 4) + '-' + v.substring(4, 6) + '-' + v.substring(6, 8) + '\t' + v.substring(8, 10) + ':' + v.substring(10, 12) + ':' + v.substring(12, 14));
                case 1:
                    return (v.substring(0, 4) + '-' + v.substring(4, 6) + '-' + v.substring(6, 8));
                case 2:
                    v = pad(v, 4);
                    return (v.substring(0, 2) + ':' + v.substring(2, 4));
                case 3:
                    v = pad(v, 6);
                    return (v.substring(0, 2) + ':' + v.substring(2, 4) + ':' + v.substring(4, 6));
            }
        }
        this.callParent();
    }
});

Ext.define('Ax.ux.form.LibGanttDatecolumn', {
    extend: 'Ext.grid.column.Date',
    alias: 'widget.libGanttDatecolumn',
    initComponent: function () {
        this.callParent();
        function pad(num, n) {
            var len = num.toString().length;
            while (len < n) {
                num = "0" + num;
                len++;
            }
            return num;
        }
        this.renderer = function (v, metaData, record, rowIndex, colIndex, store, view) {
            switch (this.axT) {
                case 0:
                    return (v.getFullYear() + '-' + pad((v.getMonth() + 1), 2) + '-' + pad(v.getDate(), 2) + '\t' + pad(v.getHours(), 2) + ':' + pad(v.getMinutes(), 2) + ':' + pad(v.getSeconds(), 2));
                case 1:
                    return (v.getFullYear() + '-' + pad((v.getMonth() + 1), 2) + '-' + pad(v.getDate(), 2));
                case 2:
                    return (pad(v.getHours(), 2) + ':' + pad(v.getMinutes(), 2));
                case 3:
                    return (pad(v.getHours(), 2) + ':' + pad(v.getMinutes(), 2) + ':' + pad(v.getSeconds(), 2));
            }
        }
    }
});