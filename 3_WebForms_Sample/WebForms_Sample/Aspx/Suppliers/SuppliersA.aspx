<%@ Page Language="C#" MasterPageFile="~/Aspx/Common/Master/testBlankScreen.master" AutoEventWireup="true" Inherits="WebForms_Sample.Aspx.Suppliers.SuppliersA" Codebehind="SuppliersA.aspx.cs" %>
<%@ Register Assembly="OpenTouryo.CustomControl" Namespace="Touryo.Infrastructure.CustomControl" TagPrefix="cc1" %>

<asp:Content ID="cphHeaderScripts" ContentPlaceHolderID="cphHeaderScripts" Runat="Server">
    <!-- Head 部の ContentPlaceHolder -->
</asp:Content>

<asp:Content ID="ContentPlaceHolder_A" ContentPlaceHolderID="ContentPlaceHolder_A" Runat="Server">
    <h4>Suppliers 画面Ａ（件数確認）</h4>
    <p>
        ［件数確認］で Suppliers のデータ件数を共通Dao 経由で取得し、<br />
        OK メッセージ ダイアログで表示します。<br />
        ［一覧へ］で画面Ｂ（一覧＆バッチ更新）に遷移します。
    </p>
    <cc1:WebCustomLabel ID="lblMessage" runat="server" Width="500px"></cc1:WebCustomLabel>
</asp:Content>

<asp:Content ID="cphFooterScripts" ContentPlaceHolderID="cphFooterScripts" Runat="Server">
    <!-- Footer 部の ContentPlaceHolder -->
</asp:Content>
