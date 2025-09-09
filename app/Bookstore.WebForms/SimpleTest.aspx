<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SimpleTest.aspx.cs" Inherits="Bookstore.WebForms.SimpleTest" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Simple Test - No Authentication Required</title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="padding: 20px;">
            <h1>Simple Test Page</h1>
            <p>This page should load without any authentication or redirect issues.</p>
            <p><strong>Current Time:</strong> <%= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") %></p>
            <p><strong>Server:</strong> <%= Environment.MachineName %></p>
            <p><strong>Application Path:</strong> <%= Request.ApplicationPath %></p>
            
            <hr />
            
            <h2>Navigation Test</h2>
            <ul>
                <li><a href="SimpleTest.aspx">Reload This Page</a></li>
                <li><a href="Test.aspx">Test Page</a></li>
                <li><a href="Default.aspx">Default Page</a></li>
                <li><a href="Login.aspx">Login Page</a></li>
            </ul>
        </div>
    </form>
</body>
</html>