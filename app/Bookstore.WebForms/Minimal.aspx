<%@ Page Language="C#" %>

<!DOCTYPE html>
<html>
<head>
    <title>Minimal Test</title>
</head>
<body>
    <h1>Minimal Test Page</h1>
    <p>This is a completely minimal page with no code-behind, no master page, no authentication.</p>
    <p>Current time: <%= DateTime.Now.ToString() %></p>
    <p>If you can see this, the basic ASP.NET WebForms infrastructure is working.</p>
    
    <h2>Test Links</h2>
    <ul>
        <li><a href="Minimal.aspx">Reload this page</a></li>
        <li><a href="SimpleTest.aspx">Simple Test Page</a></li>
        <li><a href="Test.aspx">Test Page</a></li>
    </ul>
</body>
</html>