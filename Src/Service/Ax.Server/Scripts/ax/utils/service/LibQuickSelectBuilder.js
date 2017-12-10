Ext.ns('Ax.utils');


var BillListingFilter = {
    Draft: 1,
    UnRelease: 2,
    Release: 4,
    Invalid: 8,
    EndCase: 16,
    UnAudit: 32,
    Audit: 64,
    UnValidity: 128,
    Validity: 256
}


Ax.utils.LibQuickSelectBuilder = {
    createSelectBar: function (billType) {
        var draft = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '草稿',
            checked: true
        });
        var unRelease = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '未生效',
            checked: true
        });
        var release = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '生效',
            checked: true
        });
        var invalid = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '作废'
        });
        var endCase = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '结案',
            checked: true
        });
        var unAudit = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '未审核',
            checked: true
        });
        var audit = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '已审核',
            checked: true
        });

        var unValidity = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '失效',
            checked: true
        });
        var validity = Ext.create('Ext.form.field.Checkbox', {
            labelWidth: 80,
            labelAlign: 'right',
            fieldLabel: '有效',
            checked: true
        });
        var items;
        if (billType == BillTypeEnum.Master) {
            items = [draft, unRelease, release, unValidity, validity, unAudit, audit];
        } else {
            items = [draft, unRelease, release, invalid, endCase, unAudit, audit];
        }
        var panel = Ext.create('Ext.panel.Panel', {
            height: 30,
            items: {
                xtype: 'fieldset',
                border: false,
                layout: { type: 'hbox', align: 'stretch' },
                items: items
            }
        });

        function buildChecked() {
            var filter = 0;
            if (draft.getValue())
                filter += BillListingFilter.Draft;
            if (unRelease.getValue())
                filter += BillListingFilter.UnRelease;
            if (release.getValue())
                filter += BillListingFilter.Release;
            if (unAudit.getValue())
                filter += BillListingFilter.UnAudit;
            if (audit.getValue())
                filter += BillListingFilter.Audit;
            if (billType == BillTypeEnum.Master) {
                if (unValidity.getValue())
                    filter += BillListingFilter.UnValidity;
                if (validity.getValue())
                    filter += BillListingFilter.Validity;
            } else {
                if (invalid.getValue())
                    filter += BillListingFilter.Invalid;
                if (endCase.getValue())
                    filter += BillListingFilter.EndCase;
            }
            return filter;
        };

        return [panel, buildChecked];
    }
};
