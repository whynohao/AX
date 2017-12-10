<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Register Assembly="PageOffice, Version=3.0.0.1, Culture=neutral, PublicKeyToken=1d75ee5788809228"
    Namespace="PageOffice" TagPrefix="po" %>

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        string filePath = ViewData["docName"].ToString();
        PageOfficeCtrl1.ServerPage = Request.ApplicationPath + "/pageoffice/server.aspx";
        PageOfficeCtrl1.Caption = "文件在线安全浏览";
        PageOfficeCtrl1.JsFunction_AfterDocumentOpened = "AfterDocumentOpened()";
        PageOfficeCtrl1.AllowCopy = false;//禁止拷贝
        PageOfficeCtrl1.Menubar = true;
        PageOfficeCtrl1.OfficeToolbars = false;
        PageOfficeCtrl1.CustomToolbar = true;
        PageOfficeCtrl1.OfficeVendor = OfficeVendorType.AutoSelect;
        PageOfficeCtrl1.AddCustomToolButton("打印", "Print()", 6);

        //打开文件
        PageOfficeCtrl1.WebOpen(filePath, (OpenModeType)ViewData["OpenModeType"], ViewData["userName"].ToString());
    }
</script>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Print</title>
    <script>
        function AfterDocumentOpened() {
            <%if(!(bool)ViewData["canDownload"]){ %>
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(4, false);  //禁止另存
            <%}%>
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(5, true);  //允许打印
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(6, true);  //允许页面设置
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(8, true);  //允许打印预览
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(3, false);  //禁用保存
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(1, false);  //禁用打开
        }
        function Print() {
            document.getElementById("PageOfficeCtrl1").ShowDialog(4);
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
