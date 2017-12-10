Ext.define("MthPlan.FilterField", {
    extend          : "Ext.form.TextField",
    width           : 150,
    enableKeyEvents : true,

    margin          : 0,
    border          : 0,
    cls             : 'filterfield',
    width           : '100%',
    hideLabel       : true,

    // The task store instance
    store           : null,

    listeners : {
        keyup      : {
            fn     : function (field, e) {
                var value = field.getValue();
                var regexp = new RegExp(Ext.String.escapeRegex(value), 'i')

                if (value) {
                    field.store.filterTreeBy(function (task) {
                        return regexp.test(task.get('Name'))
                    });
                } else {
                    field.store.clearTreeFilter();
                }
            },
            buffer : 200
        },
        specialkey : {
            fn : function (field, e) {
                if (e.getKey() === e.ESC) {
                    field.reset();

                    field.store.clearTreeFilter();
                }
            }
        }
    }
});
