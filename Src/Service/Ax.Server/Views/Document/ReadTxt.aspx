<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>ReadTxt</title>
    <script type="text/javascript">
        <%if ((bool)ViewData["canPrint"])
        { %>
        function preview() {
            var bdhtml = window.document.body.innerHTML;
            var pnthtml = document.getElementById("content").innerHTML;
            window.document.body.innerHTML = pnthtml;
            window.print();
            window.document.body.innerHTML = bdhtml;
        }
        <%} %>
    </script>
</head>
<body>
    <%if ((bool)ViewData["canPrint"])
    { %>
    <button onclick ="preview();">打印并预览</button>
    <hr />
    <%} %>
    <div id = "content">
        <code style="white-space:pre;"><%=ViewData["content"]%></code>
    </div>
</body>
</html>
