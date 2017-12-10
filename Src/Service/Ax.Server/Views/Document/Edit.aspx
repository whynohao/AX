<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Register Assembly="PageOffice, Version=3.0.0.1, Culture=neutral, PublicKeyToken=1d75ee5788809228"
    Namespace="PageOffice" TagPrefix="po" %>

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        string filePath = ViewData["FileFullPath"].ToString();
        PageOfficeCtrl1.ServerPage = Request.ApplicationPath + "/pageoffice/server.aspx";
        PageOfficeCtrl1.Caption = "编辑文件";
        PageOfficeCtrl1.CustomToolbar = true;
        PageOfficeCtrl1.AddCustomToolButton("保存", "Save()", 1);
        PageOfficeCtrl1.SaveFilePage = "Save?fileId=" + ViewData["FileId"] + "&userHandle=" + ViewData["UserHandle"]+"&filename="+ ViewData["NewFileName"];   //处理文件保存
        PageOfficeCtrl1.OfficeVendor = OfficeVendorType.AutoSelect;
        //打开文件
        PageOfficeCtrl1.WebOpen(filePath, (OpenModeType)ViewData["OpenModeType"], ViewData["UserName"].ToString());
        PageOfficeCtrl1.JsFunction_AfterDocumentSaved = "Saved()";
    }
</script>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Edit</title>
    <script src="../../Scripts/jquery-3.1.1.min.js"></script>
    <script src="../../Scripts/desk/DocumentManage/DMSaveDoc.js"></script>
    <script type="text/javascript">
        window.onunload = function () {
            //var str = prompt("随便写点儿啥吧", "比如我叫啥");
            if (SaveSuccess)
                saveDoc("<%=ViewData["NewFileName"]%>", false, "<%=ViewData["FileId"]%>", "<%=ViewData["DirId"]%>", "<%=ViewData["DirType"]%>", "<%=ViewData["UserHandle"]%>", "");
        };
        var SaveSuccess = false;
        function Saved() {
            SaveSuccess = true;
            if ("<%=ViewData["IsChrome"]%>" == "true") {
                window.onunload();
                window.external.close();
            }
        }
        function Save() {
            document.getElementById("PageOfficeCtrl1").WebSave();
        }
    </script>
</head>
<body>
    <div>
        <div style=" width:auto; height:850px;">
            <po:PageOfficeCtrl ID="PageOfficeCtrl1" runat="server">
            </po:PageOfficeCtrl>
        </div>
    </div>
</body>
</html>
