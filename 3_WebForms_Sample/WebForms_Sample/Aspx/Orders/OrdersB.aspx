<%@ Page Language="C#" MasterPageFile="~/Aspx/Common/Master/testBlankScreen.master" AutoEventWireup="true" Inherits="WebForms_Sample.Aspx.Orders.OrdersB" Codebehind="OrdersB.aspx.cs" EnableEventValidation="false" %>
<%@ Register Assembly="OpenTouryo.CustomControl" Namespace="Touryo.Infrastructure.CustomControl" TagPrefix="cc1" %>

<asp:Content ID="cphHeaderScripts" ContentPlaceHolderID="cphHeaderScripts" Runat="Server">
    <!-- Head 部の ContentPlaceHolder -->
</asp:Content>

<asp:Content ID="ContentPlaceHolder_A" ContentPlaceHolderID="ContentPlaceHolder_A" Runat="Server">
    <h4>Orders 画面Ｂ（条件検索・ページング・バッチ更新）</h4>

    <%-- 検索条件。マスタ・テーブル関連項目は DDL 化する。 --%>
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

    <cc1:WebCustomLabel ID="lblPager" runat="server" Width="600px"></cc1:WebCustomLabel><br />
    <cc1:WebCustomLabel ID="lblMessage" runat="server" Width="700px"></cc1:WebCustomLabel><br />

    <%-- ［行追加］はグリッド外のボタン（空行＝RowState:Added を足す） --%>
    <cc1:WebCustomButton ID="btnAddRow" runat="server" Text="行追加" Width="100px" />

    <%-- 共通仕様：一覧表示は GridView（DataSource にバインド）。
         ★ グリッド内のコントロールには自動結線の接頭辞（ddl/txt 等）を付けない。
           付けると行ごとに SelectedIndexChanged 等が不要に自動結線されるため。
         ★ DDL の選択値の設定は RowDataBound（.NET 標準イベント）で行う。
           フレームワークの自動結線対象外なので OnRowDataBound で明示的に結線する。 --%>
    <div style="overflow-x: auto;">
    <asp:GridView ID="gvwOrders" runat="server" AutoGenerateColumns="False"
                  CssClass="table table-sm table-bordered" OnRowDataBound="gvwOrders_RowDataBound">
        <Columns>
            <asp:TemplateField HeaderText="OrderID">
                <ItemTemplate><asp:Label ID="lblOrderID" runat="server" Text='<%# Eval("OrderID") %>'></asp:Label></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="得意先">
                <ItemTemplate><asp:DropDownList ID="CustomerID" runat="server" Width="150px"></asp:DropDownList></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="担当者">
                <ItemTemplate><asp:DropDownList ID="EmployeeID" runat="server" Width="130px"></asp:DropDownList></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="OrderDate">
                <ItemTemplate><asp:TextBox ID="OrderDate" runat="server" Width="90px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="RequiredDate">
                <ItemTemplate><asp:TextBox ID="RequiredDate" runat="server" Width="90px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ShippedDate">
                <ItemTemplate><asp:TextBox ID="ShippedDate" runat="server" Width="90px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="配送業者">
                <ItemTemplate><asp:DropDownList ID="ShipVia" runat="server" Width="130px"></asp:DropDownList></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Freight">
                <ItemTemplate><asp:TextBox ID="Freight" runat="server" Width="70px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ShipName">
                <ItemTemplate><asp:TextBox ID="ShipName" runat="server" Width="150px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ShipAddress">
                <ItemTemplate><asp:TextBox ID="ShipAddress" runat="server" Width="150px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ShipCity">
                <ItemTemplate><asp:TextBox ID="ShipCity" runat="server" Width="100px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ShipRegion">
                <ItemTemplate><asp:TextBox ID="ShipRegion" runat="server" Width="80px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ShipPostalCode">
                <ItemTemplate><asp:TextBox ID="ShipPostalCode" runat="server" Width="90px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ShipCountry">
                <ItemTemplate><asp:TextBox ID="ShipCountry" runat="server" Width="100px"></asp:TextBox></ItemTemplate>
            </asp:TemplateField>
            <%-- 行ごとの［更新］［削除］。RowCommand で InnerButtonID を見て振り分ける。 --%>
            <asp:ButtonField CommandName="Update" Text="更新" ButtonType="Button" />
            <asp:ButtonField CommandName="Delete" Text="削除" ButtonType="Button" />
        </Columns>
    </asp:GridView>
    </div>
</asp:Content>

<asp:Content ID="cphFooterScripts" ContentPlaceHolderID="cphFooterScripts" Runat="Server">
    <!-- Footer 部の ContentPlaceHolder -->
</asp:Content>
