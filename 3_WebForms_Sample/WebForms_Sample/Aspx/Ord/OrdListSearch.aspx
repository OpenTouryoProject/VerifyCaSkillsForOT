<%@ Page Language="C#" MasterPageFile="~/Aspx/Common/Master/testBlankScreen.master" AutoEventWireup="true" Inherits="WebForms_Sample.Aspx.Ord.OrdListSearch" Codebehind="OrdListSearch.aspx.cs" EnableEventValidation="false" %>
<%@ Register Assembly="OpenTouryo.CustomControl" Namespace="Touryo.Infrastructure.CustomControl" TagPrefix="cc1" %>

<asp:Content ID="cphHeaderScripts" ContentPlaceHolderID="cphHeaderScripts" Runat="Server">
    <!-- Head 部の ContentPlaceHolder -->
</asp:Content>

<asp:Content ID="ContentPlaceHolder_A" ContentPlaceHolderID="ContentPlaceHolder_A" Runat="Server">
    <h4>受注管理（Ord）：条件検索一覧（画面Ａ）</h4>
    <p>
        ［検索］で受注（Orders）を条件検索します（Ｄ層は共通Dao。表示値はＳＱＬでマスタと JOIN 済み）。<br />
        ［追加］で画面Ｂを「追加」モードで、行の［詳細］で画面Ｂを「詳細（更新・削除）」モードで開きます。
    </p>

    <%-- 検索条件。マスタ・テーブル関連項目は ＤＤＬ 化する。 --%>
    <table>
        <tr>
            <td>得意先（Customers）</td>
            <td><cc1:WebCustomDropDownList ID="ddlCustomerID" runat="server" Width="200px" /></td>
            <td>担当者（Employees）</td>
            <td><cc1:WebCustomDropDownList ID="ddlEmployeeID" runat="server" Width="160px" /></td>
        </tr>
        <tr>
            <td>配送業者（Shippers）</td>
            <td><cc1:WebCustomDropDownList ID="ddlShipVia" runat="server" Width="200px" /></td>
            <td>出荷先国（前方一致）</td>
            <td><cc1:WebCustomTextBox ID="txtShipCountry" runat="server" Width="160px" /></td>
        </tr>
    </table>

    <cc1:WebCustomLabel ID="lblPager" runat="server" Width="700px"></cc1:WebCustomLabel><br />
    <cc1:WebCustomLabel ID="lblMessage" runat="server" Width="700px"></cc1:WebCustomLabel><br />

    <%-- 共通仕様：一覧表示は GridView（DataSource にバインド）。
         一覧は参照のみ（追加・更新・削除は画面Ｂ）。 --%>
    <div style="overflow-x: auto;">
    <asp:GridView ID="gvwOrders" runat="server" AutoGenerateColumns="False"
                  CssClass="table table-sm table-bordered">
        <Columns>
            <asp:BoundField DataField="OrderID" HeaderText="OrderID" />
            <asp:BoundField DataField="CustomerName" HeaderText="得意先" />
            <asp:BoundField DataField="EmployeeName" HeaderText="担当者" />
            <asp:BoundField DataField="OrderDate" HeaderText="受注日" DataFormatString="{0:yyyy/MM/dd}" />
            <asp:BoundField DataField="RequiredDate" HeaderText="要求日" DataFormatString="{0:yyyy/MM/dd}" />
            <asp:BoundField DataField="ShippedDate" HeaderText="出荷日" DataFormatString="{0:yyyy/MM/dd}" />
            <asp:BoundField DataField="ShipperName" HeaderText="配送業者" />
            <asp:BoundField DataField="Freight" HeaderText="運賃" />
            <asp:BoundField DataField="ShipName" HeaderText="出荷先" />
            <asp:BoundField DataField="ShipCity" HeaderText="出荷先市" />
            <asp:BoundField DataField="ShipCountry" HeaderText="出荷先国" />
            <%-- 行ごとの［詳細］。RowCommand で InnerButtonID を見て振り分ける。 --%>
            <asp:ButtonField CommandName="Detail" Text="詳細" ButtonType="Button" />
        </Columns>
    </asp:GridView>
    </div>
</asp:Content>

<asp:Content ID="cphFooterScripts" ContentPlaceHolderID="cphFooterScripts" Runat="Server">
    <!-- Footer 部の ContentPlaceHolder -->
</asp:Content>
