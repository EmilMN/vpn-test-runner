<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="VPN_Status_Checker.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>VPN Status Checker | Test Runner</title>
    </head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <center>
        <asp:ScriptManager ID="scrManager" runat="server">
            </asp:ScriptManager>
            <br />
        <br />
        VPN Server Status Checker | Test Runner (v.0.1)<br />
        <br />
        <asp:Button ID="btnRunTests" runat="server" OnClick="btnRunTests_Click" Text="Run Tests" />
        &nbsp;<asp:Button ID="btnPing" runat="server" OnClick="btnPing_Click" Text="Ping Servers" />
        &nbsp;<br />
            <br />
            <asp:Button ID="btnDownloadReport" runat="server" OnClick="btnDownloadReport_Click" Text="Download Report" />
&nbsp;<asp:Button ID="btnEmail" runat="server" OnClick="btnEmail_Click" Text="Send Alert to NOC" />
            &nbsp;<asp:Button ID="btnClear" runat="server" OnClick="btnClear_Click" Text="Clear Results" />
            <br />
            <asp:Label ID="lblEmailAlert" runat="server" ForeColor="#990000"></asp:Label>
            <br />
            <br />
            <asp:CheckBox ID="chkAutoRun" runat="server" Text="Auto-run tests" />
&nbsp;
            <asp:CheckBox ID="chkAutoAlert" runat="server" AutoPostBack="true" Text="Automatically send alert if server is down" />
            <br />
            <br />
            Auto-run interval: <asp:DropDownList ID="dlAutoInterval" runat="server">
                <asp:ListItem Value="5">5 minutes</asp:ListItem>
                <asp:ListItem Value="15">15 minutes</asp:ListItem>
                <asp:ListItem Value="30">30 minutes</asp:ListItem>
                <asp:ListItem Value="60">1 Hour</asp:ListItem>
                <asp:ListItem Value="180">3 Hours</asp:ListItem>
                <asp:ListItem Selected="True" Value="360">6 Hours</asp:ListItem>
                <asp:ListItem Value="720">12 Hours</asp:ListItem>
                <asp:ListItem Value="1440">24 Hours</asp:ListItem>
            </asp:DropDownList>
            &nbsp;Note: Click on &quot;Run Tests&quot; to save your configuration.</center>

        <br />
        <br />
        <asp:Label ID="lblDate" runat="server"></asp:Label>
        <br />
        <br />
        <asp:Label ID="lblDownServers" runat="server"></asp:Label>
        <br />
        <br />
                    <asp:Label ID="lblActiveServers" runat="server"></asp:Label>
                <br />
        <br />
        <asp:Label ID="lblHighLoadServers" runat="server"></asp:Label>
        <br />
        <br />
        <asp:Label ID="lblInvalidInfo" runat="server"></asp:Label>
        <br />
        <br />
        <asp:Label ID="lblError" runat="server"></asp:Label>
        <br />
        <br />
        <asp:Timer ID="tmrAutoRun" runat="server" Enabled="False" Interval="30000" OnTick="tmrAutoRun_Tick">
        </asp:Timer>
        <br />
    
    </div>
    </form>
</body>
</html>
