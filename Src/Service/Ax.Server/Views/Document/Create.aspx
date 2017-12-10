<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Register Assembly="PageOffice, Version=3.0.0.1, Culture=neutral, PublicKeyToken=1d75ee5788809228"
    Namespace="PageOffice" TagPrefix="po" %>
<script runat="server">
    protected void PageOfficeCtrl1_Load(object sender, EventArgs e)
    {
        PageOfficeCtrl1.ServerPage = Request.ApplicationPath + "/pageoffice/server.aspx";
        PageOfficeCtrl1.SaveFilePage = "SaveNew?userHandle=" + ViewData["UserHandle"]+"&filename="+ ViewData["NewFileName"];
        PageOfficeCtrl1.Caption = "新增文件";
        PageOfficeCtrl1.CustomToolbar = true;
        PageOfficeCtrl1.AddCustomToolButton("保存", "Save()", 1);
        PageOfficeCtrl1.OfficeVendor = OfficeVendorType.AutoSelect;
        //创建文件
        PageOfficeCtrl1.WebCreateNew(ViewData["UserName"].ToString(), (DocumentVersion)ViewData["DocumentVersion"]);//可创建不同版本的word文件
        PageOfficeCtrl1.JsFunction_AfterDocumentSaved = "Saved()";
    }
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Create</title>
    <script src="../../Scripts/jquery-3.1.1.min.js"></script>
    <script type="text/javascript" src="../../Scripts/desk/DocumentManage/DMSaveDoc.js"></script>
    <script type="text/javascript">
        <%if (ViewData["IsChrome"].ToString() != "true") {%>
        window.onunload = function () {
            if (SaveSuccess)
                saveDoc("<%=ViewData["NewFileName"]%>", true, "", "<%=ViewData["DirId"]%>", "<%=ViewData["DirType"]%>", "<%=ViewData["UserHandle"]%>", "<%=ViewData["RealFileName"]%>");
        };
        <%}%>
        var SaveSuccess = false;
        function Saved() {
            SaveSuccess = true;
            <%if(ViewData["IsChrome"].ToString() == "true") {%>
                saveDoc("<%=ViewData["NewFileName"]%>", true, "", "<%=ViewData["DirId"]%>", "<%=ViewData["DirType"]%>", "<%=ViewData["UserHandle"]%>", "<%=ViewData["RealFileName"]%>");
                window.external.close();
            <%}%>
        }
        function Save(){
            document.getElementById("PageOfficeCtrl1").WebSave();
        }
    </script>

</head>
<body>
    <form id="form1"  action="Create" runat="server">
    <div id="content">
        <div id="textcontent" style="width: auto; height: 850px;">
            <po:PageOfficeCtrl ID="PageOfficeCtrl1" runat="server" CustomToolbar="False" Menubar="False" OnLoad="PageOfficeCtrl1_Load">
            </po:PageOfficeCtrl>
        </div>
    </div>
    </form>
</body>
</html>
