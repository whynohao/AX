finARAgingAnalysisRptVcl = function () {
    Ax.vcl.LibVclRpt.apply(this,arguments);
};
var proto = finARAgingAnalysisRptVcl.prototype = Object.create(Ax.vcl.LibVclRpt.prototype);
proto.constructor = finARAgingAnalysisRptVcl;

proto.showRpt = function (condition) {
    var data = this.invorkBcf("GetData", [condition]);
    this.tpl = this.invorkBcf("GetDynamicTemplate", [condition]);
    var grid;
    if (this.win !== undefined) {
        grid = this.dataSet.dataList[0].ownGrid;
        this.dataSet.dataList.pop();
        delete Ext.data.Model.schema.entities["fin.ARAgingAnalysisRptFINARAGINGANALYSIS"];
        delete Ext.data.Model.schema.entityClasses["fin.ARAgingAnalysisRptFINARAGINGANALYSIS"];
        this.setDataSet(data, false);

        var curRange = this.tpl.Layout.GridRange;
        var destColumns = Ext.decode(curRange.Renderer);
        var colFunc = function (columns) {
            for (var i = 0; i < columns.length; i++) {
                if (columns[i].columns)
                    colFunc(columns[i].columns);
                else if (columns[i].hasOwnProperty('summaryRenderer')) {
                    columns[i].summaryRenderer = vcl.summaryRenderer[columns[i].summaryRenderer];
                }
            }
        };
        colFunc(destColumns);

        Ext.suspendLayouts();
        grid.reconfigure(this.dataSet.dataList[0], destColumns);
        this.dataSet.dataList[0].ownGrid = grid;
        Ext.resumeLayouts(true);
    }
    else {
        this.setDataSet(data, false);
    }
    
};