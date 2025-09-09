<%@ Page Language="C#" %>

<!DOCTYPE html>
<html>
<head>
    <title>Simple Default Page</title>
</head>
<body>
    <h1>Simple Default Page</h1>
    <p>This is a simplified version of the default page without any authentication or complex logic.</p>
    <p>Current time: <%= DateTime.Now.ToString() %></p>
    
    <h2>Navigation</h2>
    <ul>
        <li><a href="Minimal.aspx">Minimal Test</a></li>
        <li><a href="SimpleTest.aspx">Simple Test</a></li>
        <li><a href="DefaultSimple.aspx">Reload This Page</a></li>
    </ul>
    
    <h2>Status</h2>
    <p>If you can see this page, the basic WebForms infrastructure is working correctly.</p>
</body>
</html>