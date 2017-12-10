<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Register Assembly="PageOffice, Version=3.0.0.1, Culture=neutral, PublicKeyToken=1d75ee5788809228"
    Namespace="PageOffice" TagPrefix="po" %>

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        string filePath = ViewData["docName"].ToString();
        PageOfficeCtrl1.ServerPage = Request.ApplicationPath + "/pageoffice/server.aspx";
        PageOfficeCtrl1.Caption = "阅读文件";
        PageOfficeCtrl1.JsFunction_AfterDocumentOpened = "AfterDocumentOpened()";
        PageOfficeCtrl1.AllowCopy = false;//禁止拷贝
        PageOfficeCtrl1.Menubar = false;
        PageOfficeCtrl1.OfficeToolbars = false;
        PageOfficeCtrl1.CustomToolbar = false;
        PageOfficeCtrl1.OfficeVendor = OfficeVendorType.AutoSelect;
        //打开文件
        PageOfficeCtrl1.WebOpen(filePath, (OpenModeType)ViewData["OpenModeType"], ViewData["userName"].ToString());
    }
</script>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>ReadOnly</title>
    <script>
        function AfterDocumentOpened() {
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(4, false);  //禁止另存
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(5, false);  //禁止打印
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(6, false);  //禁止页面设置
            document.getElementById("PageOfficeCtrl1").SetEnableFileCommand(8, false);  //禁止打印预览
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
