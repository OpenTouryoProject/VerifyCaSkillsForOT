<%@ Page Language="C#" MasterPageFile="~/Aspx/Common/Master/testBlankScreen.master" AutoEventWireup="true" Inherits="WebForms_Sample.Aspx.Suppliers.SuppliersB" Codebehind="SuppliersB.aspx.cs" EnableEventValidation="false" %>
<%@ Register Assembly="OpenTouryo.CustomControl" Namespace="Touryo.Infrastructure.CustomControl" TagPrefix="cc1" %>

<asp:Content ID="cphHeaderScripts" ContentPlaceHolderID="cphHeaderScripts" Runat="Server">
    <!-- Head 部の ContentPlaceHolder -->
</asp:Content>

<asp:Content ID="ContentPlaceHolder_A" ContentPlaceHolderID="ContentPlaceHolder_A" Runat="Server">
    <h4>Suppliers 画面Ｂ（一覧＆バッチ更新）</h4>
    <p>
        ［一覧取得］で自動生成Dao の参照処理をＢ層経由で実行します。<br />
        グリッド中で行の追加・更新・削除を行い、［バッチ更新］でまとめてDBに反映します。
    </p>

    <!-- ［行追加］はグリッド外のボタン（空行＝RowState:Added を足す） -->
    <cc1:WebCustomButton ID="btnAddRow" runat="server" Text="行追加" Width="100px" />

    <cc1:WebCustomLabel ID="lblMessage" runat="server" Width="600px"></cc1:WebCustomLabel>

    <!-- 共通仕様：一覧表示は GridView（DataSource にバインド）を使用する。
         ★ グリッド内のコントロールには自動結線の接頭辞（txt 等）を付けない。
           付けると行ごとに TextChanged 等が不要に結線されるため、FindControl で取る。
         ★ DataKeyNames は使わない（追加行の主キーが未採番＝DBNull のため成立しない）。 -->
    <asp:GridView ID="gvwSuppliers" runat="server" AutoGenerateColumns="False"
                  CssClass="table table-sm table-bordered" Width="100%">
        <Columns>
            <asp:TemplateField HeaderText="SupplierID">
                <ItemTemplate>
                    <asp:Label ID="lblSupplierID" runat="server" Text='<%# Eval("SupplierID") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="CompanyName">
                <ItemTemplate>
                    <asp:TextBox ID="CompanyName" runat="server" Text='<%# Eval("CompanyName") %>' Width="150px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ContactName">
                <ItemTemplate>
                    <asp:TextBox ID="ContactName" runat="server" Text='<%# Eval("ContactName") %>' Width="120px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ContactTitle">
                <ItemTemplate>
                    <asp:TextBox ID="ContactTitle" runat="server" Text='<%# Eval("ContactTitle") %>' Width="120px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>            <asp:TemplateField HeaderText="Address">
                <ItemTemplate>
                    <asp:TextBox ID="Address" runat="server" Text='<%# Eval("Address") %>' Width="160px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>            <asp:TemplateField HeaderText="City">
                <ItemTemplate>
                    <asp:TextBox ID="City" runat="server" Text='<%# Eval("City") %>' Width="100px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>            <asp:TemplateField HeaderText="Region">
                <ItemTemplate>
                    <asp:TextBox ID="Region" runat="server" Text='<%# Eval("Region") %>' Width="80px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>            <asp:TemplateField HeaderText="PostalCode">
                <ItemTemplate>
                    <asp:TextBox ID="PostalCode" runat="server" Text='<%# Eval("PostalCode") %>' Width="90px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Country">
                <ItemTemplate>
                    <asp:TextBox ID="Country" runat="server" Text='<%# Eval("Country") %>' Width="100px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Phone">
                <ItemTemplate>
                    <asp:TextBox ID="Phone" runat="server" Text='<%# Eval("Phone") %>' Width="120px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>            <asp:TemplateField HeaderText="Fax">
                <ItemTemplate>
                    <asp:TextBox ID="Fax" runat="server" Text='<%# Eval("Fax") %>' Width="120px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>            <asp:TemplateField HeaderText="HomePage">
                <ItemTemplate>
                    <asp:TextBox ID="HomePage" runat="server" Text='<%# Eval("HomePage") %>' Width="160px"></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <%-- 行ごとの［更新］［削除］。RowCommand で InnerButtonID を見て振り分ける。 --%>
            <asp:ButtonField CommandName="Update" Text="更新" ButtonType="Button" />
            <asp:ButtonField CommandName="Delete" Text="削除" ButtonType="Button" />
        </Columns>
    </asp:GridView>
</asp:Content>

<asp:Content ID="cphFooterScripts" ContentPlaceHolderID="cphFooterScripts" Runat="Server">
    <!-- Footer 部の ContentPlaceHolder -->
</asp:Content>
