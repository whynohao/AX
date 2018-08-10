using AxCRL.Comm.Utils;
using AxCRL.Template.DataSource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.Layout
{

    public static class JsBuilder
    {
        public static string BuildControlGroup(IList<LayoutField> fields)
        {
            string ret = @"[{xtype: 'container',layout: { type: 'column', columns: 4 }, style: { marginTop: '6px', marginBottom: '6px' },
                            defaults: { labelAlign: 'right' }, defaultType: 'libTextField',items:[";
            StringBuilder builder = new StringBuilder();
            StringBuilder fieldBuilder = new StringBuilder();
            int count = 4;

            foreach (var item in fields)
            {
                if (item.ColumnSpan <= count)
                { builder.Append(string.Format("{0},", BuildField(item))); }

                else
                {
                    builder.Append("{'xtype': 'label',");
                    builder.AppendFormat("height:24,width: '100%',columnWidth:{0},colspan:{1}", count / 4.0, count);
                    builder.Append("},");
                    count = 4;
                    builder.Append(string.Format("{0},", BuildField(item)));
                }

                count -= item.ColumnSpan;

                if (count <= 0)
                { count = 4; }
            }

            if (builder.Length > 0)
            { builder.Remove(builder.Length - 1, 1); }

            ret = ret + builder.ToString() + "]}]";
            return ret;
        }

        public static string BuildField(LayoutField field)
        {
            StringBuilder builder = new StringBuilder();

            if (!string.IsNullOrEmpty(field.XType))
            { builder.AppendFormat("xtype:'{0}',", field.XType); }

            if (field.InputType != InputType.Text)
            {
                switch (field.InputType)
                {
                    case InputType.Password:
                        builder.Append("inputType:'password',");
                        break;

                    case InputType.File:
                        builder.Append("inputType:'file',");
                        break;

                    case InputType.Url:
                        builder.Append("inputType:'url',");
                        break;

                    case InputType.Email:
                        builder.Append("inputType:'email',");
                        break;
                }
            }

            switch (field.XType)
            {
                case "libSearchfield":
                    #region 查询语句和查询字段
                    builder.AppendFormat("selectSql:'{0}',", field.SelectSql);
                    builder.AppendFormat("selectFields:'{0}',", field.SelectFields);
                    #endregion
                    break;
            }

            if (field.XType == "libNumberField" && field.StepValue != 1.0)
            {
                builder.Append(string.Format("stepValue:{0},", field.StepValue)); //增加步进值设置属性 Zhangkj20170227
            }

            if (field.RowSpan > 1)
            { builder.AppendFormat("height:{0},rowspan:{1},autoScroll:true,", 24 * field.RowSpan, field.RowSpan); }

            else
            { builder.Append("height:24,"); }

            builder.AppendFormat("margin: '2,5,2,2',width: '100%',columnWidth:{0},colspan:{1},", field.ColumnSpan / 4.0, field.ColumnSpan);
            //if (field.ColumnSpan == 1)
            //{  }

            //else
            //{ builder.AppendFormat("columnWidth:1,colspan:{1},", 300 * field.ColumnSpan, field.ColumnSpan); }

            switch (field.ControlType)
            {
                case LibControlType.Quantity:
                    builder.AppendFormat("hideTrigger:true,formatField:'{0}',", field.Format);
                    break;

                case LibControlType.Number:
                    builder.Append("allowDecimals:false,");
                    break;

                case LibControlType.Double:
                    if (field.Precision != 2)
                    { builder.AppendFormat("decimalPrecision:{0},", field.Precision); }

                    break;

                case LibControlType.Rate:
                    builder.Append("numType:1,");
                    break;

                case LibControlType.Price:
                    builder.Append("numType:2,");
                    break;

                case LibControlType.Amount:
                    builder.Append("numType:3,");
                    break;

                case LibControlType.TaxRate:
                    builder.Append("numType:4,");
                    break;
            }

            if (!string.IsNullOrEmpty(field.RelProgId))
            { builder.AppendFormat("relProgId:'{0}',", field.RelProgId); }

            if (!string.IsNullOrEmpty(field.RelTableIndex))
            { builder.AppendFormat("relTableIndex:'{0}',", field.RelTableIndex); }

            if (field.GridAttribute != null)
            {
                var json = JsonConvert.SerializeObject(field.GridAttribute);
                builder.AppendFormat("gridAttribute:'{0}',", json);
            }

            if (field.ReadOnly)
            {
                builder.Append("readOnly:true,");
                field.DisplayText = string.Format("({0})", field.DisplayText);
            }

            builder.AppendFormat("fieldLabel: '{0}',", field.DisplayText);

            if (!field.AllowBlank)
            { builder.Append("labelStyle:'color:#a7392e',"); }

            if (!string.IsNullOrEmpty(field.RelSource))
            { builder.AppendFormat("relSource: {0},relName:'{1}',relPk:'{2}',selParams:{3},", field.RelSource, field.RelName, field.RelPk, field.SelParams); }

            if (field.TextOption != null && field.TextOption.Length > 0)
            {
                BuildTextOption(builder, field.TextOption, field.FontName);
                builder.Append(',');
            }

            if (field.KeyValueOption != null && field.KeyValueOption.Count > 0)
            {
                BuildKeyValueOption(builder, field.KeyValueOption, field.FontName);
                builder.Append(',');
            }

            if (field.XType == "libAttributeCodeField")
            {
                builder.AppendFormat("attrField:'{0}',attrDesc:'{1}',", field.AttributeField, field.Name.Replace("CODE", "DESC"));

            }
            else
                if (field.XType == "libAttributeDescField")
                {
                    builder.AppendFormat("attrField:'{0}',attrCode:'{1}',", field.AttributeField, field.Name.Replace("DESC", "CODE"));

                }
                else
                    if (field.XType == "libSearchfield" || field.XType == "libSearchfieldTree")
                    {
                        if (string.IsNullOrEmpty(field.FontName) == false)
                        { builder.AppendFormat("fontName:'{0}',", field.FontName); }
                    }

            builder.AppendFormat("name: '{0}',tableIndex:{1}", field.Name, field.TableIndex);
            return "{" + builder.ToString() + "}";
        }

        private static void BuildTextOptionFilter(StringBuilder builder, string[] textOption)
        {
            builder.Append(@",filter: {type: 'list',options:[");

            for (int i = 0; i < textOption.Length; i++)
            {
                if (i != 0)
                { builder.Append(','); }

                builder.Append("[" + i + ",'" + textOption[i] + "']");
            }

            builder.Append("]}");
        }

        private static void BuildTextOption(StringBuilder builder, string[] textOption, string fontName = null)
        {
            if (string.IsNullOrEmpty(fontName))
                builder.Append(@"queryMode: 'local', editable: false,
                                 displayField: 'value',
                                 valueField: 'key',
                                 store:Ext.create('Ext.data.Store', {fields: ['key', 'value'],data : [");

            else
            {
                builder.Append("queryMode: 'local', editable: false,displayField: 'value',valueField: 'key'," +
                                "listeners: {afterrender: function(c) { var inputDom= c.getEl().dom.getElementsByTagName('INPUT')[0];inputDom.style=\"font-family:" + fontName + "\";}}," +
                                "tpl:Ext.create('Ext.XTemplate','<tpl for=\".\"><li role=\"option\" style=\"font-family:" + fontName + "\" unselectable=\"on\" class=\"x-boundlist-item\">{value}</li></tpl>')," +
                                "store:Ext.create('Ext.data.Store', {fields: ['key', 'value'],data : [");
            }

            for (int i = 0; i < textOption.Length; i++)
            {
                if (i != 0)
                { builder.Append(','); }

                builder.Append("{'key':" + i + ",'value':'" + textOption[i] + "'}");
            }

            builder.Append("]})");
        }
        private static void BuildTextOptionTpl(StringBuilder builder, string name, string[] textOption, string fontName = null)
        {
            builder.Append(@",xtype:'templatecolumn',tpl:'");

            for (int i = 0; i < textOption.Length; i++)
            {
                if (string.IsNullOrEmpty(fontName))
                { builder.AppendFormat("<tpl if=\"{0} == {1}\">{2}</tpl>", name, i, textOption[i]); }

                else
                { builder.AppendFormat("<tpl if=\"{0} == {1}\"><span style=\"font-family:{3}\">{2}</span></tpl>", name, i, textOption[i], fontName); }
            }

            builder.Append("'");
        }

        private static void BuildKeyValueOptionFilter(StringBuilder builder, LibTextOptionCollection keyValueOption)
        {
            builder.Append(@",filter: {type: 'list',options:[");

            for (int i = 0; i < keyValueOption.Count; i++)
            {
                if (i != 0)
                { builder.Append(','); }

                builder.Append("['" + keyValueOption[i].Key + "','" + keyValueOption[i].Value + "']");
            }

            builder.Append("]}");
        }
        private static void BuildKeyValueOption(StringBuilder builder, LibTextOptionCollection keyValueOption, string fontName = "")
        {
            if (string.IsNullOrEmpty(fontName))
                builder.Append(@"queryMode: 'local', editable: false,
                                 displayField: 'value',
                                 valueField: 'key',
                                 store:Ext.create('Ext.data.Store', {fields: ['key', 'value'],data : [");

            else
            {
                builder.Append("queryMode: 'local', editable: false,displayField: 'value',valueField: 'key'," +
                                "listeners: {afterrender: function(c) { var inputDom= c.getEl().dom.getElementsByTagName('INPUT')[0];inputDom.style=\"font-family:" + fontName + "\";}}," +
                                "tpl:Ext.create('Ext.XTemplate','<tpl for=\".\"><li role=\"option\" style=\"font-family:" + fontName + "\" unselectable=\"on\" class=\"x-boundlist-item\">{value}</li></tpl>')," +
                                "store:Ext.create('Ext.data.Store', {fields: ['key', 'value'],data : [");
            }

            for (int i = 0; i < keyValueOption.Count; i++)
            {
                if (i != 0)
                { builder.Append(','); }

                builder.Append("{'key':'" + keyValueOption[i].Key + "','value':'" + keyValueOption[i].Value + "'}");
            }

            builder.Append("]})");
        }
        private static void BuildKeyValueOptionTpl(StringBuilder builder, string name, LibTextOptionCollection keyValueOption, string fontName = "")
        {
            builder.Append(@",xtype:'templatecolumn',tpl:'");

            for (int i = 0; i < keyValueOption.Count; i++)
            {
                if (string.IsNullOrEmpty(fontName))
                { builder.AppendFormat("<tpl if=\"{0} == &quot;{1}&quot;\">{2}</tpl>", name, keyValueOption[i].Key, keyValueOption[i].Value); }

                else
                { builder.AppendFormat("<tpl if=\"{0} == &quot;{1}&quot;\"><span style=\"font-family:{3}\">{2}</span></tpl>", name, keyValueOption[i].Key, keyValueOption[i].Value, fontName); }
            }

            builder.Append("'");
        }
        private static void BuildSearchfieldTpl(StringBuilder builder, string name, string relName, string fontName = null)
        {
            builder.Append(@",xtype:'templatecolumn',tpl:");
            //builder.Append("'<tpl if=\"" + name + " != &quot;&quot;\">{" + name + "},{" + relName + "}</tpl>'");
            builder.Append("'<tpl if=\"");

            if (string.IsNullOrEmpty(relName))
            {
                builder.AppendFormat("{0} != &quot;&quot; && {0} != undefined", name);

                if (string.IsNullOrEmpty(fontName))
                { builder.Append("\">{" + name + "}</tpl>'"); }

                else
                { builder.Append("\"><span style=\"font-family:" + fontName + "\">{" + name + "}</span></tpl>'"); }

            }
            else
            {
                builder.AppendFormat("{0} != &quot;&quot; && {0} != undefined && {1} !=&quot;&quot; && {1} != undefined", name, relName);

                if (string.IsNullOrEmpty(fontName))
                { builder.Append("\">{" + name + "},{" + relName + "}</tpl>"); }

                else
                { builder.Append("\"><span style=\"font-family:" + fontName + "\">{" + name + "},{" + relName + "}</span></tpl>"); }

                builder.Append("<tpl if=\"");
                builder.AppendFormat("{0} != &quot;&quot; && {0} != undefined && ({1} ==&quot;&quot; || {1} == undefined)", name, relName);

                if (string.IsNullOrEmpty(fontName))
                { builder.Append("\">{" + name + "}</tpl>'"); }

                else
                { builder.Append("\"><span style=\"font-family:" + fontName + "\">{" + name + "}</span></tpl>'"); }
            }
        }

        public static string BuildGrid(List<LayoutField> layoutFields, bool addAutoRowNo)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in layoutFields)
            {
                builder.AppendFormat("{0},", BuildColumn(item));
            }

            if (builder.Length > 0)
            { builder.Remove(builder.Length - 1, 1); }

            if (addAutoRowNo)
            { return @"[{xtype: 'rownumberer',width:50}," + builder.ToString() + "]"; }

            else
            { return "[" + builder.ToString() + "]"; }
        }

        public static string BuildBandGrid(IList<BandLayoutField> bandColumn, bool addAutoRowNo)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in bandColumn)
            {
                if (item.Columns != null && item.Columns.Count > 0)
                {
                    builder.AppendFormat("{0},", BuildBandColumn(item));

                }
                else
                {
                    builder.AppendFormat("{0},", BuildColumn(item.Field));
                }
            }

            if (builder.Length > 0)
            { builder.Remove(builder.Length - 1, 1); }

            if (addAutoRowNo)
            { return @"[{xtype: 'rownumberer',width:50}," + builder.ToString() + "]"; }

            else
            { return "[" + builder.ToString() + "]"; }
        }

        public static string BuildBandColumn(BandLayoutField bandColumn)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("text: '{0}',", bandColumn.Name);
            builder.Append("columns: [");

            foreach (var item in bandColumn.Columns)
            {
                if (item.Columns != null && item.Columns.Count > 0)
                {
                    builder.AppendFormat("{0},", BuildBandColumn(item));

                }
                else
                {
                    builder.AppendFormat("{0},", BuildColumn(item.Field));
                }
            }

            if (builder.Length > 0)
            { builder.Remove(builder.Length - 1, 1); }

            builder.Append("]");
            return "{" + builder.ToString() + "}";
        }

        public static string BuildColumn(LayoutField field)
        {
            StringBuilder builder = new StringBuilder();

            if (field.ReadOnly)
            { field.DisplayText = string.Format("({0})", field.DisplayText); }

            if (!field.AllowBlank)
            { builder.AppendFormat("style:'color:#a7392e',"); }

            builder.AppendFormat("text: '{0}',", field.DisplayText);
            builder.AppendFormat("dataIndex: '{0}',tableIndex:{1}", field.Name, field.TableIndex);

            #region 查询语句和查询字段
            builder.AppendFormat(",selectSql:'{0}'", field.SelectSql);
            builder.AppendFormat(",selectFields:'{0}'", field.SelectFields);
            #endregion

            if (field.Width != 100)
            { builder.AppendFormat(",width:{0}", field.Width); }

            if (field.Hidden)
            { builder.Append(",hidden:true"); }

            builder.AppendFormat(",filter: '{0}'", field.FilterType);

            if (field.Summary != LibSummary.None)
            {
                switch (field.Summary)
                {
                    case LibSummary.Count:
                        builder.Append(",summaryType:'count'");
                        break;

                    case LibSummary.Sum:
                        builder.Append(",summaryType:'sum'");
                        break;

                    case LibSummary.Min:
                        builder.Append(",summaryType:'min'");
                        break;

                    case LibSummary.Max:
                        builder.Append(",summaryType:'max'");
                        break;

                    case LibSummary.Average:
                        builder.Append(",summaryType:'average'");
                        break;
                }
            }

            if (!string.IsNullOrEmpty(field.SummaryRenderer))
            { builder.AppendFormat(",summaryRenderer:'{0}'", field.SummaryRenderer); }

            string xtype = field.XType == null ? "libTextField" : field.XType;
            string showType = string.Empty;

            switch (xtype)
            {
                case "libCheckboxField":
                    showType = "libCheckcolumn";

                    if (field.ReadOnly)
                    { builder.Append(",readOnly:true"); }

                    builder.Append(",filterable:true");
                    break;

                case "libComboboxField":
                    if (field.TextOption != null && field.TextOption.Length > 0)
                    {
                        BuildTextOptionTpl(builder, field.Name, field.TextOption, field.FontName);
                        BuildTextOptionFilter(builder, field.TextOption);
                    }

                    if (field.KeyValueOption != null && field.KeyValueOption.Count > 0)
                    {
                        BuildKeyValueOptionTpl(builder, field.Name, field.KeyValueOption, field.FontName);
                        BuildKeyValueOptionFilter(builder, field.KeyValueOption);
                    }

                    break;

                case "libNumberField":
                    showType = "libNumbercolumn";

                    if (!string.IsNullOrEmpty(field.Format))
                    { builder.AppendFormat(",formatField:'{0}'", field.Format); }

                    switch (field.ControlType)
                    {
                        case LibControlType.Number:
                            builder.Append(",allowDecimals:false");
                            break;

                        case LibControlType.Double:
                            if (field.Precision != 2)
                            { builder.AppendFormat(",decimalPrecision:{0}", field.Precision); }

                            break;

                        case LibControlType.Rate:
                            builder.Append(",numType:1");
                            break;

                        case LibControlType.Price:
                            builder.Append(",numType:2");
                            break;

                        case LibControlType.Amount:
                            builder.Append(",numType:3");
                            break;

                        case LibControlType.TaxRate:
                            builder.Append(",numType:4");
                            break;
                    }

                    builder.Append(",filterable:true");
                    break;

                case "libSearchfield":
                case "libSearchfieldTree":
                    BuildSearchfieldTpl(builder, field.Name, field.RelName, field.FontName);

                    if (!string.IsNullOrEmpty(field.RelSource))
                    {
                        builder.AppendFormat(",relSource: {0},relName:'{1}',relPk:'{2}',selParams:{3}", field.RelSource, field.RelName, field.RelPk, field.SelParams);
                    }

                    builder.Append(",filterable:true");
                    break;

                case "libDatetimefield":
                    showType = "libDatecolumn";
                    builder.Append(",axT:0");
                    builder.Append(",filterable:true");
                    break;

                case "libDateField":
                    showType = "libDatecolumn";
                    builder.Append(",axT:1");
                    builder.Append(",filterable:true");
                    break;

                case "libHourMinuteField":
                    //showType = "libDatecolumn";
                    builder.Append(",axT:2");
                    builder.Append(",filterable:true");
                    break;

                case "libTimeField":
                    showType = "libDatecolumn";
                    builder.Append(",axT:3");
                    builder.Append(",filterable:true");
                    break;

                case "libAttributeDescField":
                    builder.AppendFormat(",attrField:'{0}',attrCode:'{1}'", field.AttributeField, field.Name.Replace("DESC", "CODE"));
                    builder.Append(",filterable:true");
                    break;

                default:
                    builder.Append(",filterable:true");
                    break;
            }

            if (!string.IsNullOrEmpty(showType))
            {
                builder.AppendFormat(",xtype: '{0}'", showType);
            }

            if (field.InputType == InputType.Password)
            {
                builder.Append(@",xtype:'templatecolumn',tpl:'<tpl>******<tpl>'");
            }

            if (!field.ReadOnly)
            {
                builder.Append(",editor: {xtype:'");
                builder.AppendFormat("{0}'", xtype);

                if (field.InputType != InputType.Text)
                {
                    switch (field.InputType)
                    {
                        case InputType.Password:
                            builder.Append(",inputType:'password'");
                            break;

                        case InputType.File:
                            builder.Append(",inputType:'file'");
                            break;

                        case InputType.Url:
                            builder.Append(",inputType:'url'");
                            break;

                        case InputType.Email:
                            builder.Append(",inputType:'email'");
                            break;
                    }
                }

                builder.AppendFormat(",tableIndex:{0}", field.TableIndex);
                builder.AppendFormat(",selectSql:'{0}'", field.SelectSql);
                builder.AppendFormat(",selectFields:'{0}'", field.SelectFields);

                switch (field.ControlType)
                {
                    case LibControlType.Quantity:
                        builder.AppendFormat(",hideTrigger:true,formatField:'{0}'", field.Format);
                        break;

                    case LibControlType.Number:
                        builder.Append(",allowDecimals:false");
                        break;

                    case LibControlType.Double:
                        if (field.Precision != 2)
                        { builder.AppendFormat(",decimalPrecision:{0}", field.Precision); }

                        break;

                    case LibControlType.Rate:
                        builder.Append(",numType:1");
                        break;

                    case LibControlType.Price:
                        builder.Append(",numType:2");
                        break;

                    case LibControlType.Amount:
                        builder.Append(",numType:3");
                        break;

                    case LibControlType.TaxRate:
                        builder.Append(",numType:4");
                        break;
                }

                if (!string.IsNullOrEmpty(field.RelProgId))
                { builder.AppendFormat(",relProgId:'{0}'", field.RelProgId); }

                if (!string.IsNullOrEmpty(field.RelTableIndex))
                { builder.AppendFormat(",relTableIndex:'{0}'", field.RelTableIndex); }

                if (field.GridAttribute != null)
                {
                    var json = JsonConvert.SerializeObject(field.GridAttribute);
                    builder.AppendFormat(",gridAttribute:'{0}'", json);
                }

                if (!string.IsNullOrEmpty(field.RelSource))
                { builder.AppendFormat(",relSource: {0},relName:'{1}',relPk:'{2}',selParams:{3}", field.RelSource, field.RelName, field.RelPk, field.SelParams); }

                if (field.IsDynamic)
                { builder.AppendFormat(",isDynamic:true"); }

                switch (xtype)
                {
                    case "libAttributeCodeField":
                        builder.AppendFormat(",attrField:'{0}',attrDesc:'{1}'", field.AttributeField, field.Name.Replace("CODE", "DESC"));
                        break;

                    case "libAttributeDescField":
                        builder.AppendFormat(",attrField:'{0}',attrCode:'{1}'", field.AttributeField, field.Name.Replace("DESC", "CODE"));
                        break;

                    case "libSearchfield":
                    case "libSearchfieldTree":
                        if (string.IsNullOrEmpty(field.FontName) == false)
                        {
                            builder.AppendFormat(",fontName: '{0}'", field.FontName);
                        }

                        break;
                }

                if (field.TextOption != null && field.TextOption.Length > 0)
                {
                    builder.Append(",");
                    BuildTextOption(builder, field.TextOption, field.FontName);
                }

                if (field.KeyValueOption != null && field.KeyValueOption.Count > 0)
                {
                    builder.Append(",");
                    BuildKeyValueOption(builder, field.KeyValueOption, field.FontName);
                }

                builder.Append("}");
            }

            return "{" + builder.ToString() + "}";
        }


        private static string BuildButtonCore(FunButton button, bool isSub)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");

            if (!isSub)
            { builder.Append("xtype: 'button',"); }

            builder.AppendFormat("text: '{0}',", button.DisplayText);
            builder.AppendFormat("btnId: '{0}',", button.Name);
            builder.AppendFormat("useCondition: {0},", button.UseCondition);
            builder.Append("handler:function(self){vcl=self.up('window').vcl, vcl.vclHandler(self, {libEventType:LibEventTypeEnum.ButtonClick, dataInfo: {fieldName:'" + button.Name + "'}});}");
            builder.Append("}");
            return builder.ToString();
        }

        public static string BuildButton(IList<FunButton> buttonList)
        {
            StringBuilder builder = new StringBuilder();

            foreach (FunButton button in buttonList)
            {
                if (button.FunButtonList.Count > 0)
                {
                    builder.Append("{xtype: 'splitbutton',");
                    builder.AppendFormat("text: '{0}',", button.DisplayText);
                    builder.AppendFormat("btnId: '{0}',", button.Name);
                    builder.AppendFormat("useCondition: {0},menu: [", button.UseCondition);

                    foreach (var menu in button.FunButtonList)
                    {
                        builder.AppendFormat("{0},", BuildButtonCore(menu, true));
                    }

                    if (builder.Length > 0)
                    { builder.Remove(builder.Length - 1, 1); }

                    builder.Append("]},");

                }
                else
                {
                    builder.AppendFormat("{0},", BuildButtonCore(button, false));
                }
            }

            if (builder.Length > 0)
            { builder.Remove(builder.Length - 1, 1); }

            return string.Format("[{0}]", builder.ToString());
        }

        public static string BuildFilterFieldJs(IList<FilterField> filterFieldList)
        {
            int count = 1;
            StringBuilder sb0 = new StringBuilder();
            string jsStr = "(function (){{{0};var _panel1=Ext.create('Ext.form.Panel',{{layout:{{type:'table',columns:4}},border:false,items:[{1}]}});return Ext.create('Ext.form.Panel',{{ layout:{{type:'hbox',align:'middle'}},items:[_panel1,_button],maxHeight:60,autoScroll:true}})}})();";

            foreach (var item in filterFieldList)
            {
                sb0.AppendFormat("var _{0}=Ext.create('{1}',{{fieldLabel:'{2}',name:'{3}',labelAlign:'right',margin:'5 0'}});", count, item.Xtype, item.DisplayName, item.Name);
                count++;
            }

            if (string.IsNullOrEmpty(sb0.ToString()))
            { return string.Empty; }

            StringBuilder sb1 = new StringBuilder();

            for (int i = 1; i < count; i++)
            {
                sb1.AppendFormat("var _value{0}=_{0}.value;if(_{0}.value&&_{0}.xtype==='datefield')_value{0}=(_value{0}.getYear()+1900)*10000+(_value{0}.getMonth()+1)*100+_value{0}.getDate(); ", i);
            }

            StringBuilder sb3 = new StringBuilder();
            sb3.Append("var _obj={QueryFields:[]};");

            for (int i = 1; i < count; i++)
            {
                sb3.AppendFormat("if(_value{0}!==undefined&&_value{0}!==null&&_value{0}!== '')_obj.QueryFields.push({{Name:_{0}.name,QueryChar:2,Value:[_value{0}]}});", i);
            }

            sb1.AppendFormat("{0}vcl.showRpt(_obj);", sb3);
            sb0.AppendFormat("var _button=Ext.create('Ext.Button',{{text:'搜索',iconCls:'fa fa-search',margin:'0 30',style:{{ color:'#f5f5f5'}},listeners:{{click:function(){{{0}}}}}}});", sb1);
            StringBuilder sb2 = new StringBuilder();

            for (int i = 1; i < count; i++)
            {
                sb2.Append("_" + i + ",");
            }

            return string.Format(jsStr, sb0, sb2);
        }

        public static string BuildFilterFieldJsForBillListing(IList<FilterField> filterFieldList)
        {
            int count = 1;
            StringBuilder sb0 = new StringBuilder();
            string jsStr = "(function(){{var store=Ext.create('Ext.data.Store',{{fields:['id','name'],data:[{{'id':0,'name':'等于空'}},{{'id':1,'name':'等于'}},{{'id':2,'name':'包含'}},{{'id': 4,'name':'大于等于'}},{{'id':5,'name':'小于等于'}},{{'id':6,'name':'大于'}},{{'id':7,'name':'小于'}},{{'id':8,'name':'不等于'}},{{'id':9,'name':'包括'}}]}});{0};var _panel1=Ext.create('Ext.form.Panel',{{layout:{{type:'table',columns:3}},border:false,items:[{1}]}});return Ext.create('Ext.form.Panel',{{ layout:{{type:'hbox',align:'middle'}},items:[_panel1,_button],maxHeight:80,autoScroll:true}})}})();";

            foreach (var item in filterFieldList)
            {
                sb0.AppendFormat("var _{0}_comboBox=Ext.create('Ax.ux.form.LibComboboxField',{{name:'{1}',margin:'5 5',width:{4},labelWidth:{5},height:25,fieldLabel:'{2}',labelAlign:'right',displayField:'name',valueField:'id',store:store,value:2,flex:1}});var _{0}_text=Ext.create('{3}',{{fieldLabel:'',name:'{1}_text',margin:'5 0',flex:1}});var _{0}=Ext.create('Ext.Panel',{{layout:{{type:'hbox',align:'stretch'}},items:[_{0}_comboBox,_{0}_text],border: false}});", count, item.Name, item.DisplayName, item.Xtype, item.DisplayName.Length > 7 ? 200 + (item.DisplayName.Length - 7) * 10 : 200, item.DisplayName.Length > 7 ? 105 + (item.DisplayName.Length - 7) * 10 : 105);
                count++;
            }

            if (string.IsNullOrEmpty(sb0.ToString()))
            { return string.Empty; }

            StringBuilder sb1 = new StringBuilder();

            for (int i = 1; i < count; i++)
            {
                sb1.AppendFormat("var _value{0}=_{0}_text.value;if(_{0}_text.value&&_{0}_text.xtype=='datefield')_value{0}=(_value{0}.getYear()+1900)*10000+(_value{0}.getMonth()+1)*100+_value{0}.getDate();", i);
            }

            StringBuilder sb3 = new StringBuilder();
            sb3.Append("var _obj = [];");

            for (int i = 1; i < count; i++)
            {
                sb3.AppendFormat("if(_{0}_comboBox.value===0)_obj.push({{Name:_{0}_comboBox.name,QueryChar:1,Value:['']}});else if(_value{0}!==undefined&&_value{0}!==null && _value{0}!=='')_obj.push({{Name:_{0}_comboBox.name,QueryChar:_{0}_comboBox.value,Value:[_value{0}]}});", i);
            }

            sb1.AppendFormat("{0}reloadGridPanelBillListing(_obj);", sb3);
            sb0.AppendFormat("var _button=Ext.create('Ext.Button',{{text:'搜索',iconCls:'fa fa-search',margin:'0 30',style:{{color:'#f5f5f5'}},listeners:{{click:function(){{{0}}}}}}});", sb1);
            StringBuilder sb2 = new StringBuilder();

            for (int i = 1; i < count; i++)
            {
                sb2.Append("_" + i + ",");
            }

            return string.Format(jsStr, sb0, sb2);
        }
    }

    public class LayoutField
    {
        private string _Name;

        public string Name
        {
            get { return _Name; }

            set { _Name = value; }
        }
        private string _DisplayText;

        public string DisplayText
        {
            get { return _DisplayText; }

            set { _DisplayText = value; }
        }
        private bool _AllowBlank = true;

        public bool AllowBlank
        {
            get { return _AllowBlank; }

            set { _AllowBlank = value; }
        }
        private string _XType;

        public string XType
        {
            get { return _XType; }

            set { _XType = value; }
        }

        private double _StepValue = 1.0;
        /// <summary>
        /// 步进值设置项，默认为1.0
        /// Zhangkj 20170227 增加
        /// </summary>
        public double StepValue
        {
            get { return _StepValue; }

            set { _StepValue = value; }
        }
        /// <summary>
        /// 字体设置
        /// </summary>
        public string FontName { get; set; }

        private int _Precision = 2;

        public int Precision
        {
            get { return _Precision; }

            set { _Precision = value; }
        }

        private bool _ReadOnly;

        public bool ReadOnly
        {
            get { return _ReadOnly; }

            set { _ReadOnly = value; }
        }

        private string _SelParams = "[]";

        public string SelParams
        {
            get { return _SelParams; }

            set { _SelParams = value; }
        }

        private string _RelSource;

        public string RelSource
        {
            get { return _RelSource; }

            set { _RelSource = value; }
        }

        private int _TableIndex;

        public int TableIndex
        {
            get { return _TableIndex; }

            set { _TableIndex = value; }
        }
        private string _RelName;

        public string RelName
        {
            get { return _RelName; }

            set { _RelName = value; }
        }

        private string _RelPk;

        public string RelPk
        {
            get { return _RelPk; }

            set { _RelPk = value; }
        }

        private string[] _TextOption;

        public string[] TextOption
        {
            get { return _TextOption; }

            set { _TextOption = value; }
        }

        private LibTextOptionCollection _KeyValueOption;

        public LibTextOptionCollection KeyValueOption
        {
            get { return _KeyValueOption; }

            set { _KeyValueOption = value; }
        }

        private LibControlType _ControlType;

        public LibControlType ControlType
        {
            get { return _ControlType; }

            set { _ControlType = value; }
        }
        private string _Format = string.Empty;

        public string Format
        {
            get { return _Format; }

            set { _Format = value; }
        }

        private int _ColumnSpan = 1;

        public int ColumnSpan
        {
            get { return _ColumnSpan; }

            set { _ColumnSpan = value; }
        }

        private int _RowSpan = 1;

        public int RowSpan
        {
            get { return _RowSpan; }

            set { _RowSpan = value; }
        }

        private string _RelProgId;

        public string RelProgId
        {
            get { return _RelProgId; }

            set { _RelProgId = value; }
        }

        private string _RelTableIndex;

        public string RelTableIndex
        {
            get { return _RelTableIndex; }

            set { _RelTableIndex = value; }
        }

        #region 搜索框的查询

        #region 查询语句
        private string _SelectSql = string.Empty;

        public string SelectSql
        {
            get { return _SelectSql; }
            set { _SelectSql = value; }
        }
        #endregion

        #region 查询字段，多个字段间用逗号(,)分隔
        private string _SelectFields = string.Empty;

        public string SelectFields
        {
            get { return _SelectFields; }
            set { _SelectFields = value; }
        }
        #endregion

        #endregion

        private int _Width = 100;

        public int Width
        {
            get { return _Width; }

            set { _Width = value; }
        }

        private bool _Hidden = false;

        public bool Hidden
        {
            get { return _Hidden; }

            set { _Hidden = value; }
        }

        private bool _IsDynamic = false;

        public bool IsDynamic
        {
            get { return _IsDynamic; }

            set { _IsDynamic = value; }
        }

        private LibSummary _Summary = LibSummary.None;

        public LibSummary Summary
        {
            get { return _Summary; }

            set { _Summary = value; }
        }

        private string _SummaryRenderer;

        public string SummaryRenderer
        {
            get { return _SummaryRenderer; }

            set { _SummaryRenderer = value; }
        }

        private string _AttributeField;

        public string AttributeField
        {
            get { return _AttributeField; }

            set { _AttributeField = value; }
        }

        private InputType _InputType = InputType.Text;

        public InputType InputType
        {
            get { return _InputType; }

            set { _InputType = value; }
        }

        private string _FilterType;

        public string FilterType
        {
            get { return _FilterType; }

            set { _FilterType = value; }
        }

        private GridAttribute _GridAttribute;
        public GridAttribute GridAttribute
        {
            get { return _GridAttribute; }

            set { _GridAttribute = value; }
        }

        public LayoutField()
        {
        }

        public LayoutField(DataColumn column, int tableIndex)
        {
            this.TableIndex = tableIndex;
            this.Name = column.ColumnName;
            this.DisplayText = column.Caption;

            if (column.ExtendedProperties.ContainsKey(FieldProperty.ReadOnly))
            { this.ReadOnly = (bool)column.ExtendedProperties[FieldProperty.ReadOnly]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.AllowEmpty))
            { this.AllowBlank = (bool)column.ExtendedProperties[FieldProperty.AllowEmpty]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.ColumnSpan))
            { this.ColumnSpan = (int)column.ExtendedProperties[FieldProperty.ColumnSpan]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.RowSpan))
            { this.RowSpan = (int)column.ExtendedProperties[FieldProperty.RowSpan]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.IsDynamic))
            { this.IsDynamic = (bool)column.ExtendedProperties[FieldProperty.IsDynamic]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.Summary))
            { this.Summary = (LibSummary)column.ExtendedProperties[FieldProperty.Summary]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.SummaryRenderer))
            { this.SummaryRenderer = (string)column.ExtendedProperties[FieldProperty.SummaryRenderer]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.AttributeField))
            { this.AttributeField = (string)column.ExtendedProperties[FieldProperty.AttributeField]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.InputType))
            { this.InputType = (InputType)column.ExtendedProperties[FieldProperty.InputType]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.Precision))
            { this.Precision = (int)column.ExtendedProperties[FieldProperty.Precision]; }

            LibControlType controlType = LibControlType.Text;

            if (column.ExtendedProperties.ContainsKey(FieldProperty.ControlType))
            { controlType = (LibControlType)column.ExtendedProperties[FieldProperty.ControlType]; }

            this.ControlType = controlType;

            if (column.ExtendedProperties.ContainsKey(FieldProperty.SelectSql))
            { SelectSql = (string)column.ExtendedProperties[FieldProperty.SelectSql]; }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.SelectFields))
            { SelectFields = (string)column.ExtendedProperties[FieldProperty.SelectFields]; }

            //Zhangkj 20170227 增加数值控件步进值设置支持
            double stepValue = 1.0;

            if (column.ExtendedProperties.ContainsKey(FieldProperty.StepValue))
            {
                if (double.TryParse(column.ExtendedProperties[FieldProperty.StepValue].ToString(), out stepValue))
                { this.StepValue = stepValue; }
            }

            if (column.ExtendedProperties.ContainsKey(FieldProperty.FontName))
            { this.FontName = column.ExtendedProperties[FieldProperty.FontName].ToString(); }

            switch (controlType)
            {
                case LibControlType.Text:
                case LibControlType.NText:
                    if (this.RowSpan > 1)
                    { this.XType = "libTextAreaField"; }

                    else
                    { this.XType = "libTextField"; }

                    this.FilterType = "string";
                    break;
                case LibControlType.Image:
                    this.XType = "libImageField";
                    this.FilterType = "string";
                    break;
                case LibControlType.HtmlEditor:
                    this.XType = "libHtmlEditorField";
                    this.FilterType = "string";
                    break;
                case LibControlType.Id:
                case LibControlType.IdName:
                case LibControlType.IdNameTree://Zhangkj 20170314 Added
                    this.XType = "libSearchfield";

                    if (controlType == LibControlType.IdNameTree)
                    { this.XType = "libSearchfieldTree"; }

                    RelativeSource relSource = null;
                    RelativeSourceCollection relSourceList = (RelativeSourceCollection)column.ExtendedProperties[FieldProperty.RelativeSource];

                    if (relSourceList == null)
                    {
                        int count = 20;

                        while (relSourceList == null && --count > 0)
                        {
                            //说明是关联出来的Id字段，要取得对应的RelativeSourceCollection
                            DataTable table = column.Table;
                            Dictionary<string, FieldAddr> fieldDic = table.ExtendedProperties[TableProperty.FieldAddrDic] as Dictionary<string, FieldAddr>;

                            if (fieldDic != null)
                            {
                                FieldAddr addr = fieldDic[this.Name];
                                RelativeSourceCollection list = table.Columns[addr.FieldIndex].ExtendedProperties[FieldProperty.RelativeSource] as RelativeSourceCollection;

                                if (list != null)
                                {
                                    RelativeSource source = list[addr.RelSourceIndex];
                                    LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel(source.RelSource);
                                    relSourceList = (RelativeSourceCollection)sqlModel.Tables[source.TableIndex].Columns[source.RelFields[addr.RelFieldIndex].Name].ExtendedProperties[FieldProperty.RelativeSource];

                                    if (relSourceList == null)
                                    { continue; }

                                    relSource = relSourceList[0];

                                    if (relSource.RelFields.Count > 0)
                                    {
                                        //系统约束关联名称为ID替换为NAME
                                        this.RelName = string.Format("{0}NAME", this.Name.Remove(this.Name.Length - 2, 2));
                                    }

                                    this.RelSource = "{" + string.Format("'{0}':''", relSource.RelSource) + "}";
                                }
                            }
                        }

                    }
                    else
                    {
                        relSource = relSourceList[0];
                        this.RelPk = LibSysUtils.ToString(relSource.RelPK);

                        if (relSource.RelFields.Count > 0 && (controlType == LibControlType.IdName || controlType == LibControlType.IdNameTree))
                        {
                            RelField relField = relSource.RelFields[0];
                            this.RelName = string.IsNullOrEmpty(relField.AsName) ? relField.Name : relField.AsName;
                        }

                        HashSet<int> groupHashSet = new HashSet<int>();
                        StringBuilder builder = new StringBuilder();

                        foreach (RelativeSource item in relSourceList)
                        {
                            if (!groupHashSet.Contains(item.GroupIndex))
                            {
                                groupHashSet.Add(item.GroupIndex);
                                builder.AppendFormat("'{0}':'{1}',", item.RelSource, LibSysUtils.ToString(item.GroupCondation));
                            }

                            if (item.SelConditions.Count > 0)
                            {
                                StringBuilder selBuilder = new StringBuilder();

                                foreach (SelCondition selCondition in item.SelConditions)
                                {
                                    string condition = selCondition.Condition;

                                    if (!string.IsNullOrEmpty(condition))
                                    {
                                        int startIndex = condition.IndexOf('@', 0);

                                        while (startIndex != -1)
                                        {
                                            string name = string.Empty;
                                            int endIndex = condition.IndexOf(' ', startIndex);

                                            if (endIndex == -1)
                                            { name = condition.Substring(startIndex + 1, condition.Length - startIndex - 1); }

                                            else
                                            { name = condition.Substring(startIndex + 1, endIndex - startIndex - 1); }

                                            if (name[name.Length - 1] == ']') //存在exists[]的时候，有可能出现最后一个字符为]
                                            { name = name.Remove(name.Length - 1, 1); }

                                            startIndex = condition.IndexOf('@', startIndex + name.Length);
                                            selBuilder.AppendFormat("'{0}',", name.Trim());
                                        }
                                    }
                                }

                                if (selBuilder.Length > 0)
                                { selBuilder.Remove(selBuilder.Length - 1, 1); }

                                this.SelParams = string.Format("[{0}]", selBuilder.ToString());
                            }
                        }

                        if (builder.Length > 0)
                        { builder.Remove(builder.Length - 1, 1); }

                        this.RelSource = "{" + builder.ToString() + "}";
                    }

                    //if ((LibDataType)column.ExtendedProperties[FieldProperty.DataType] != LibDataType.Text)
                    //    this.RelName = string.Empty;
                    this.FilterType = "string";
                    break;

                case LibControlType.Quantity:
                    this.XType = "libNumberField";

                    if (column.ExtendedProperties.ContainsKey(FieldProperty.Format))
                    { this.Format = (string)column.ExtendedProperties[FieldProperty.Format]; }

                    this.FilterType = "number";
                    break;

                case LibControlType.Number:
                case LibControlType.Double:
                case LibControlType.Rate:
                case LibControlType.Price:
                case LibControlType.Amount:
                case LibControlType.TaxRate:
                    this.XType = "libNumberField";
                    this.FilterType = "number";
                    break;

                case LibControlType.TextOption:
                    this.XType = "libComboboxField";
                    this.TextOption = (string[])column.ExtendedProperties[FieldProperty.Option];
                    this.FilterType = "string";
                    break;

                case LibControlType.KeyValueOption:
                    this.XType = "libComboboxField";
                    this.KeyValueOption = (LibTextOptionCollection)column.ExtendedProperties[FieldProperty.KeyValueOption];
                    this.FilterType = "string";
                    break;

                case LibControlType.YesNo:
                    this.XType = "libCheckboxField";
                    this.FilterType = "boolean";
                    break;

                case LibControlType.Date:
                    this.XType = "libDateField";
                    this.FilterType = "number";
                    break;

                case LibControlType.DateTime:
                    this.XType = "libDatetimefield";
                    this.FilterType = "number";
                    break;

                case LibControlType.HourMinute:
                    this.XType = "libHourMinuteField";
                    this.FilterType = "string";
                    break;

                case LibControlType.Time:
                    this.XType = "libTimeField";
                    this.FilterType = "number";
                    break;

                case LibControlType.FieldControl:
                    this.XType = "libFieldControl";

                    if (column.ExtendedProperties.ContainsKey(FieldProperty.RelProgId))
                    { this.RelProgId = (string)column.ExtendedProperties[FieldProperty.RelProgId]; }

                    if (column.ExtendedProperties.ContainsKey(FieldProperty.RelTableIndex))
                    { this.RelTableIndex = (string)column.ExtendedProperties[FieldProperty.RelTableIndex]; }

                    this.FilterType = "string";
                    break;

                case LibControlType.AttributeCodeField:
                    this.XType = "libAttributeCodeField";
                    this.FilterType = "string";
                    break;

                case LibControlType.AttributeDescField:
                    this.XType = "libAttributeDescField";
                    this.FilterType = "string";
                    break;

                case LibControlType.FieldOption:
                    this.XType = "libFieldOption";

                    if (column.ExtendedProperties.ContainsKey(FieldProperty.GridAttribute))
                    { this.GridAttribute = (GridAttribute)column.ExtendedProperties[FieldProperty.GridAttribute]; }

                    this.FilterType = "string";
                    break;

                default:
                    break;
            }
        }
    }

    public class BandColumn
    {
        public BandColumn(string name)
        {
            this._Name = name;
        }
        public BandColumn(string name, IList<BandColumn> columns)
        {
            this._Name = name;
            this._Columns = columns;
        }
        private string _Name;

        public string Name
        {
            get { return _Name; }

            set { _Name = value; }
        }

        private IList<BandColumn> _Columns;

        public IList<BandColumn> Columns
        {
            get
            {
                return _Columns;
            }
        }
    }

    public class BandLayoutField
    {
        private LayoutField _Field;

        public LayoutField Field
        {
            get { return _Field; }

            set { _Field = value; }
        }
        public BandLayoutField(string name)
        {
            this._Name = name;
        }
        public BandLayoutField(string name, LayoutField field)
        {
            this._Name = name;
            this._Field = field;
        }
        public BandLayoutField(string name, IList<BandLayoutField> columns)
        {
            this._Name = name;
            this._Columns = columns;
        }
        private string _Name;

        public string Name
        {
            get { return _Name; }

            set { _Name = value; }
        }

        private IList<BandLayoutField> _Columns;

        public IList<BandLayoutField> Columns
        {
            get
            {
                return _Columns;
            }
        }
    }

    public class FilterField
    {
        public string Xtype { get; set; }
        public string Name { set; get; }
        public string DisplayName { set; get; }

        public FilterField(LibControlType type, string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;

            switch (type)
            {
                case LibControlType.Text:
                case LibControlType.NText:
                case LibControlType.Id:
                case LibControlType.IdName:
                case LibControlType.IdNameTree://Zhangkj 20170314 Added
                case LibControlType.KeyValueOption:
                case LibControlType.TextOption:
                    this.Xtype = "Ext.form.field.Text";
                    break;

                case LibControlType.Quantity:
                case LibControlType.Number:
                case LibControlType.Double:
                case LibControlType.Rate:
                case LibControlType.Price:
                case LibControlType.Amount:
                case LibControlType.TaxRate:
                    this.Xtype = "Ext.form.field.Number";
                    break;

                case LibControlType.YesNo:
                    this.Xtype = "Ext.form.field.Checkbox";
                    break;

                case LibControlType.Date:
                    this.Xtype = "Ext.form.field.Date";
                    break;

                case LibControlType.DateTime:
                    this.Xtype = "Ax.ux.LibDateTimeField";
                    break;

                case LibControlType.HourMinute:
                    this.Xtype = "Ax.ux.form.LibHourMinuteField";
                    break;

                case LibControlType.Time:
                    this.Xtype = "Ext.form.field.Time";
                    break;

                case LibControlType.FieldControl:
                    this.Xtype = "Ext.form.field.ComboBox";
                    break;

                case LibControlType.AttributeCodeField:
                    this.Xtype = "Ext.form.field.Text";
                    break;

                case LibControlType.AttributeDescField:
                    this.Xtype = "Ext.form.field.TextArea";
                    break;

                default:
                    break;
            }
        }
    }
}
