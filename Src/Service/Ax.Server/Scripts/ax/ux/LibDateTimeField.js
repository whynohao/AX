Ext.define('Ax.ux.DateTimePicker', {
    extend: 'Ext.picker.Date',
    alias: 'widget.datetimepicker',
    todayText: '确定',
    timeLabel: '时间',
    dateTimeVal: null,
    initComponent: function () {
        // keep time part for value
        //var value = this.dateTimeVal;
        this.callParent();
        //this.value = value;
    },

    //onMouseDown: function (e) {
    //    e.preventDefault();
    //},
    onRender: function (container, position) {
        this.value = this.dateTimeVal;
        if (!this.timefield) {
            this.timefield = Ext.create('Ext.form.field.Time', {
                fieldLabel: this.timeLabel,
                labelWidth: 40,
                format: 'H:i:s',
                altFormats: 'His|gis|is|s',
                value: this.dateTimeVal
            });
        }

        this.timefield.ownerCt = this;

        //this.timefield.on('change', this.timeChange, this);

        this.callParent(arguments);

        var table = Ext.get(Ext.DomQuery.selectNode('table', this.el.dom));

        var tfEl = Ext.core.DomHelper.insertAfter(table, {

            tag: 'div',

            style: 'border:0px;',

            children: [{

                tag: 'div',

                cls: 'x-datepicker-footer ux-timefield'

            }]

        }, true);

        this.timefield.render(this.el.child('div div.ux-timefield'));

        var p = this.getEl().parent('div.x-layer');

        if (p) {

            p.setStyle("height", p.getHeight() + 31);

        }

    },

    // listener 时间域修改, timefield change

    //timeChange: function (tf, time, rawtime) {

    //    // if(!this.todayKeyListener) { // before render

    //    this.value = this.fillDateTime(this.value);

    //    // } else {

    //    // this.setValue(this.value);

    //    // }

    //},

    // @private

    fillDateTime: function (value) {

        if (this.timefield) {

            var timeVal = this.timefield.getValue();

            value.setHours(timeVal.getHours());

            value.setMinutes(timeVal.getMinutes());

            value.setSeconds(timeVal.getSeconds());

        }

        return value;

    },

    // @private

    changeTimeFiledValue: function (value) {

        //this.timefield.un('change', this.timeChange, this);

        this.timefield.setValue(this.value);

        //this.timefield.on('change', this.timeChange, this);

    },

    /* TODO 时间值与输入框绑定, 考虑: 创建this.timeValue 将日期和时间分开保存. */

    // overwrite

    setValue: function (value) {

        this.value = value;

        this.changeTimeFiledValue(value);

        return this.update(this.value);

    },

    // overwrite

    //getValue: function () {

    //    return this.fillDateTime(this.value);

    //},


    // overwrite : fill time before setValue

    handleDateClick: function (e, t) {
        var me = this,
            handler = me.handler;

        e.stopEvent();
        if (!me.disabled && t.dateValue && !Ext.fly(t.parentNode).hasCls(me.disabledCellCls)) {
            me.setValue(this.fillDateTime(new Date(t.dateValue)));

            me.fireEvent('select', me, me.value);
            if (handler) {
                handler.call(me.scope || me, me, me.value);
            }
            // event handling is turned off on hide
            // when we are using the picker in a field
            // therefore onSelect comes AFTER the select
            // event.
            me.onSelect();
        }
    },

    // overwrite : fill time before setValue
    selectToday: function () {

        var me = this,

            btn = me.todayBtn,

            handler = me.handler;

        if (btn && !btn.disabled) {
            me.setValue(this.fillDateTime(new Date(me.value)));
            me.fireEvent('select', me, me.value);

            if (handler) {

                handler.call(me.scope || me, me, me.value);

            }

            me.onSelect();

        }

        return me;

    }

});



Ext.define('Ax.ux.LibDateTimeField', {
    extend: 'Ext.form.field.Date',
    alias: 'widget.libDatetimefield',
    requires: ['Ax.ux.DateTimePicker'],
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    format: 'Y-m-d H:i:s',
    altFormats: 'YmdHis',
    enableKeyEvents: true,
    initComponent: function () {
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
        this.callParent();
    },
    listeners: {
        keydown: function (self, e, eOpts) {
            this.keydown(self, e, eOpts)
        }
    },
    onBlur: function (e) {
        var me = this;
        if (!me.readOnly) {
            if (me.up('form')) {
                if (!me.validating(me)) {
                    me.focus(false, true);
                    return;
                }
            }
        }
        me.callParent([e]);
    },
    setValue: function (v) {
        if (v != undefined && typeof v !== 'number' && v.getFullYear) {
            arguments[0] = (v.getFullYear() * 10000 + (v.getMonth() + 1) * 100 + v.getDate()) * 1000000 + v.getHours() * 10000 + v.getMinutes() * 100 + v.getSeconds();
        };
        if (v == undefined || v == 0)
            arguments[0] = undefined;
        return this.callParent(arguments);
    },
    getValue: function () {
        if (this.rawValue == '')
            this.value = 0;
        return this.value;
    },
    onExpand: function () {
        var value = this.getValue();
        value = Ext.Date.parse(value, "YmdHis", true);
        this.picker.setValue(Ext.isDate(value) ? value : new Date());
    },
    createPicker: function () {
        var win = this.up('window');
        if (win === undefined)
            win = this.up('[isVcl=true]');
        if (win && (!win.vcl || win.vcl.isEdit)) {
            var me = this,
            format = Ext.String.format;
            var dateTimeVal = this.getValue() == 0 ? new Date() : Ext.Date.parse(me.value, "YmdHis", true);
            return Ext.create('Ax.ux.DateTimePicker', {
                dateTimeVal: dateTimeVal,
                pickerField: me,
                floating: true,
                focusable: false, // Key events are listened from the input field which is never blurred
                hidden: true,
                minDate: me.minValue,
                maxDate: me.maxValue,
                disabledDatesRE: me.disabledDatesRE,
                disabledDatesText: me.disabledDatesText,
                disabledDays: me.disabledDays,
                disabledDaysText: me.disabledDaysText,
                format: me.format,
                showToday: me.showToday,
                startDay: me.startDay,
                minText: format(me.minText, me.formatDate(me.minValue)),
                maxText: format(me.maxText, me.formatDate(me.maxValue)),
                listeners: {
                    scope: me,
                    select: me.onSelect
                },
                keyNavConfig: {
                    esc: function () {
                        me.collapse();
                    }
                }
            });
        }
    }
});


Ext.define('Ax.ux.LibGanttDateTimeField', {
    extend: 'Ext.form.field.Date',
    alias: 'widget.libGanttDatetimefield',
    requires: ['Ax.ux.DateTimePicker'],
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    format: 'Y-m-d H:i:s',
    altFormats: 'YmdHis',
    enableKeyEvents: true,
    initComponent: function () {
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
        this.callParent();
    },
    listeners: {
        keydown: function (self, e, eOpts) {
            this.keydown(self, e, eOpts)
        }
    },
    onBlur: function (e) {
        var me = this;
        if (!me.readOnly) {
            if (me.up('form')) {
                if (!me.validating(me)) {
                    me.focus(false, true);
                    return;
                }
            }
        }
        me.callParent([e]);
    },
    onExpand: function () {
        if (this.picker)
            this.picker.setValue(this.getValue());
    },
    createPicker: function () {
        var me = this;
        var win = this.up('window');
        if (win === undefined)
            win = this.up('[isVcl=true]');
        if (win && win.vcl.isEdit) {
            format = Ext.String.format;
            var dateTimeVal = this.getValue();
            return Ext.create('Ax.ux.DateTimePicker', {
                dateTimeVal: dateTimeVal,
                pickerField: me,
                floating: true,
                focusable: false, // Key events are listened from the input field which is never blurred
                hidden: true,
                minDate: me.minValue,
                maxDate: me.maxValue,
                disabledDatesRE: me.disabledDatesRE,
                disabledDatesText: me.disabledDatesText,
                disabledDays: me.disabledDays,
                disabledDaysText: me.disabledDaysText,
                format: me.format,
                showToday: me.showToday,
                startDay: me.startDay,
                minText: format(me.minText, me.formatDate(me.minValue)),
                maxText: format(me.maxText, me.formatDate(me.maxValue)),
                listeners: {
                    scope: me,
                    select: me.onSelect
                },
                keyNavConfig: {
                    esc: function () {
                        me.collapse();
                    }
                }
            });
        }
    }

});
