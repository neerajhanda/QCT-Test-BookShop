<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="Bookstore.WebForms.Test" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Test Page</h1>
            <p>This is a simple test page to check if the application is working.</p>
            <p>Current Time: <%= DateTime.Now.ToString() %></p>
        </div>
    </form>
</body>
</html>