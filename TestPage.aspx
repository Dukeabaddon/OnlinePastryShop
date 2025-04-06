<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Test Page</h1>
            <p>This is a test page to verify that the application is working correctly.</p>
            <p>Current Time: <%= DateTime.Now.ToString() %></p>
        </div>
    </form>
</body>
</html>
