<%@ Page Language="C#" MasterPageFile="~/Aspx/Common/Master/testBlankScreen.master" AutoEventWireup="true" Inherits="WebForms_Sample.Aspx.Start.menu" Codebehind="menu.aspx.cs" %>

<asp:Content ID="cphHeaderScripts" ContentPlaceHolderID="cphHeaderScripts" Runat="Server">
    <!-- Head 部の ContentPlaceHolder -->
</asp:Content>

<asp:Content ID="ContentPlaceHolder_A" ContentPlaceHolderID="ContentPlaceHolder_A" Runat="Server">
    <!-- 最小骨格：サンプル/テスト画面へのリンクは minimize で除去済み。
         業務画面を追加したら、ここにメニュー リンクを足す。
         （リンク切れはビルドでも aspnet_compiler でも検出されず、実行時も
           Forms 認証／画面遷移チェックが先に効いて 302 になるだけなので、
           画面を削ったら必ずこのメニューも手で掃除する。） -->
    <ul>
        <li>マスタ保守
            <ul>
                <li><a href="<%= this.ResolveUrl("~/Aspx/Suppliers/SuppliersA.aspx") %>">Suppliers（件数確認・一覧＆バッチ更新）</a></li>
                <li><a href="<%= this.ResolveUrl("~/Aspx/Orders/OrdersA.aspx") %>">Orders（条件検索・ページング・バッチ更新）</a></li>
            </ul>
        </li>
    </ul>
</asp:Content>

<asp:Content ID="cphFooterScripts" ContentPlaceHolderID="cphFooterScripts" Runat="Server">
    <!-- Footer 部の ContentPlaceHolder -->
</asp:Content>
