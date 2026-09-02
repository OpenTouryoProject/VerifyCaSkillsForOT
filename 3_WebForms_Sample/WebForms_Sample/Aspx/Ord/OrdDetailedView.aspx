<%@ Page Language="C#" MasterPageFile="~/Aspx/Common/Master/testBlankScreen.master" AutoEventWireup="true" Inherits="WebForms_Sample.Aspx.Ord.OrdDetailedView" Codebehind="OrdDetailedView.aspx.cs" %>
<%@ Register Assembly="OpenTouryo.CustomControl" Namespace="Touryo.Infrastructure.CustomControl" TagPrefix="cc1" %>

<asp:Content ID="cphHeaderScripts" ContentPlaceHolderID="cphHeaderScripts" Runat="Server">
    <!-- Head 部の ContentPlaceHolder -->
</asp:Content>

<asp:Content ID="ContentPlaceHolder_A" ContentPlaceHolderID="ContentPlaceHolder_A" Runat="Server">
    <h4>受注管理（Ord）：詳細・更新（画面Ｂ）</h4>
    <p>
        初期処理でマスタ・テーブルを取得して入力用 ＤＤＬ を生成します。<br />
        画面Ａの［詳細］から来たときは自動生成Dao の参照（Ｒ）で表示し［更新］［削除］を、<br />
        画面Ａの［追加］から来たときは［追加］を活性にします。
    </p>

    <table>
        <tr>
            <td>OrderID</td>
            <td><cc1:WebCustomTextBox ID="txtOrderID" runat="server" Width="120px" ReadOnly="true" /></td>
        </tr>
        <tr>
            <td>得意先（Customers）</td>
            <td><cc1:WebCustomDropDownList ID="ddlCustomerID" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>担当者（Employees）</td>
            <td><cc1:WebCustomDropDownList ID="ddlEmployeeID" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>受注日（OrderDate）</td>
            <td><cc1:WebCustomTextBox ID="txtOrderDate" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>要求日（RequiredDate）</td>
            <td><cc1:WebCustomTextBox ID="txtRequiredDate" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>出荷日（ShippedDate）</td>
            <td><cc1:WebCustomTextBox ID="txtShippedDate" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>配送業者（Shippers）</td>
            <td><cc1:WebCustomDropDownList ID="ddlShipVia" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>運賃（Freight）</td>
            <td><cc1:WebCustomTextBox ID="txtFreight" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>出荷先名（ShipName）</td>
            <td><cc1:WebCustomTextBox ID="txtShipName" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>出荷先住所（ShipAddress）</td>
            <td><cc1:WebCustomTextBox ID="txtShipAddress" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>出荷先市（ShipCity）</td>
            <td><cc1:WebCustomTextBox ID="txtShipCity" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>出荷先地域（ShipRegion）</td>
            <td><cc1:WebCustomTextBox ID="txtShipRegion" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>出荷先郵便番号（ShipPostalCode）</td>
            <td><cc1:WebCustomTextBox ID="txtShipPostalCode" runat="server" Width="300px" /></td>
        </tr>
        <tr>
            <td>出荷先国（ShipCountry）</td>
            <td><cc1:WebCustomTextBox ID="txtShipCountry" runat="server" Width="300px" /></td>
        </tr>
    </table>

    <cc1:WebCustomLabel ID="lblMessage" runat="server" Width="700px"></cc1:WebCustomLabel>
</asp:Content>

<asp:Content ID="cphFooterScripts" ContentPlaceHolderID="cphFooterScripts" Runat="Server">
    <!-- Footer 部の ContentPlaceHolder -->
</asp:Content>
