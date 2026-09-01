//**********************************************************************************
//* トランザクション・テーブル（Orders）保守：画面Ｂ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersB
//* クラス日本語名  ：Orders 画面Ｂ（条件検索・ページング・バッチ更新）
//*
//* 作成日時        ：2026/08/28
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/28  生技              新規作成
//**********************************************************************************

using System;
using System.Data;
using System.Web.UI.WebControls;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.CustomControl;
using Touryo.Infrastructure.Framework.Presentation;
using Touryo.Infrastructure.Framework.Util;
using Touryo.Infrastructure.Public.Db;

namespace WebForms_Sample.Aspx.Orders
{
    /// <summary>Orders 画面Ｂ（条件検索・ページング・バッチ更新）</summary>
    public partial class OrdersB : MyBaseController
    {
        #region Session のキー

        /// <summary>編集中の DataTable</summary>
        private const string SessionKeyTable = "OrdersEditing";

        /// <summary>DDL 用のマスタ（DataSet）</summary>
        private const string SessionKeyMasters = "OrdersMasters";

        /// <summary>現在のページ番号</summary>
        private const string SessionKeyPage = "OrdersPageIndex";

        #endregion

        /// <summary>1ページの表示件数</summary>
        private const int PageSize = 20;

        #region Session の出し入れ

        /// <summary>編集中の DataTable を取得する</summary>
        private DataTable EditingTable
        {
            get { return this.Session[OrdersB.SessionKeyTable] as DataTable; }
            set
            {
                if (value == null) { this.Session.Remove(OrdersB.SessionKeyTable); }
                else { this.Session[OrdersB.SessionKeyTable] = value; }
            }
        }

        /// <summary>DDL 用のマスタを取得する</summary>
        private DataSet Masters
        {
            get { return this.Session[OrdersB.SessionKeyMasters] as DataSet; }
            set { this.Session[OrdersB.SessionKeyMasters] = value; }
        }

        /// <summary>現在のページ番号（1 起算）</summary>
        private int PageIndex
        {
            get
            {
                object v = this.Session[OrdersB.SessionKeyPage];
                return (v == null) ? 1 : (int)v;
            }
            set { this.Session[OrdersB.SessionKeyPage] = value; }
        }

        #endregion

        #region ページ ロードの共通処理（UOC メソッド）

        /// <summary>初期表示時の処理</summary>
        protected override void UOC_FormInit()
        {
            // 開き直したら編集内容は破棄する
            this.EditingTable = null;
            this.PageIndex = 1;

            // DDL 用のマスタを取得して検索条件の DDL を作る
            OrdersReturnValue returnValue = this.CallLayerB("OrdersMasters", null);
            if (returnValue != null)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(returnValue.Customers.Copy());
                ds.Tables.Add(returnValue.Employees.Copy());
                ds.Tables.Add(returnValue.Shippers.Copy());
                this.Masters = ds;
            }

            this.BindSearchConditionDdl();
            this.SetMainButtons();
        }

        /// <summary>ポストバック時の処理</summary>
        protected override void UOC_FormInit_PostBack()
        {
            this.SetMainButtons();
        }

        /// <summary>共通仕様：フッタ部のメイン ボタン5つを設定する</summary>
        /// <remarks>
        /// ★ 仕様：バッチ更新が開始されたらページングを止める。
        ///   ＝編集中は［前ページ］［次ページ］を非活性にする。
        /// </remarks>
        private void SetMainButtons()
        {
            WebCustomButton btn1 = (WebCustomButton)this.GetMasterWebControl("btnMButton1");
            WebCustomButton btn2 = (WebCustomButton)this.GetMasterWebControl("btnMButton2");
            WebCustomButton btn3 = (WebCustomButton)this.GetMasterWebControl("btnMButton3");
            WebCustomButton btn4 = (WebCustomButton)this.GetMasterWebControl("btnMButton4");
            WebCustomButton btn5 = (WebCustomButton)this.GetMasterWebControl("btnMButton5");

            bool editing = OrdersB.HasPendingChanges(this.EditingTable);
            int totalPages = this.TotalPages;

            btn1.Text = "検索";       btn1.Enabled = !editing;
            btn2.Text = "バッチ更新"; btn2.Enabled = true;
            btn3.Text = "前ページ";   btn3.Enabled = !editing && this.PageIndex > 1;
            btn4.Text = "次ページ";   btn4.Enabled = !editing && this.PageIndex < totalPages;
            btn5.Text = "戻る";       btn5.Enabled = true;
        }

        #endregion

        #region マスタ ページ上のボタンのイベント

        /// <summary>btnMButton1（検索）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton1_Click(FxEventArgs fxEventArgs)
        {
            this.PageIndex = 1;
            this.SearchPage("検索しました");
            return string.Empty;
        }

        /// <summary>btnMButton2（バッチ更新＝確認ダイアログを出す）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton2_Click(FxEventArgs fxEventArgs)
        {
            DataTable dt = this.EditingTable;
            if (dt == null) { this.lblMessage.Text = "先に［検索］を実行して下さい。"; return string.Empty; }

            // ★ 確認ダイアログの後処理は「次のポストバック」で走るので、ここで編集内容を確定しておく。
            this.ReadGridIntoTable(dt, -1);
            this.EditingTable = dt;

            this.ShowYesNoMessageDialog("OrdersBatchUpdate", "バッチ更新します。よろしいですか？", "確認");
            return string.Empty;
        }

        /// <summary>btnMButton3（前ページ）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton3_Click(FxEventArgs fxEventArgs)
        {
            return this.MovePage(this.PageIndex - 1);
        }

        /// <summary>btnMButton4（次ページ）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton4_Click(FxEventArgs fxEventArgs)
        {
            return this.MovePage(this.PageIndex + 1);
        }

        /// <summary>btnMButton5（画面Ａへ戻る）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL</returns>
        protected string UOC_testBlankScreen_btnMButton5_Click(FxEventArgs fxEventArgs)
        {
            return "~/Aspx/Orders/OrdersA.aspx";
        }

        /// <summary>ページを移動する</summary>
        /// <param name="targetPage">移動先のページ番号（1 起算）</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        private string MovePage(int targetPage)
        {
            // ★ 仕様：バッチ更新が開始されたらページングを止め、処理対象を当該結果セットに限定する。
            if (OrdersB.HasPendingChanges(this.EditingTable))
            {
                this.lblMessage.Text = "編集中はページングできません（［バッチ更新］で反映するか、画面を開き直して下さい）。";
                this.BindGrid(this.EditingTable);
                return string.Empty;
            }

            if (targetPage < 1) { targetPage = 1; }
            this.PageIndex = targetPage;
            this.SearchPage("ページを移動しました");
            return string.Empty;
        }

        #endregion

        #region 確認ダイアログの後処理

        /// <summary>YES/NO 確認ダイアログで YES が押されたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        protected override void UOC_YesNoDialog_Yes_Click(FxEventArgs parentFxEventArgs)
        {
            switch (parentFxEventArgs.ButtonID)
            {
                case "btnMButton2":
                    this.BatchUpdate();
                    break;

                default:
                    break;
            }
        }

        /// <summary>YES/NO 確認ダイアログで NO が押されたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        protected override void UOC_YesNoDialog_No_Click(FxEventArgs parentFxEventArgs)
        {
            this.lblMessage.Text = "バッチ更新を中止しました。";
            this.BindGrid(this.EditingTable);
        }

        /// <summary>YES/NO 確認ダイアログが×で閉じられたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        protected override void UOC_YesNoDialog_X_Click(FxEventArgs parentFxEventArgs)
        {
            this.BindGrid(this.EditingTable);
        }

        /// <summary>バッチ更新（CUD をＢ層＋自動生成Dao 経由で一括反映）</summary>
        private void BatchUpdate()
        {
            DataTable dt = this.EditingTable;
            if (dt == null) { this.lblMessage.Text = "先に［検索］を実行して下さい。"; return; }

            OrdersReturnValue returnValue = this.CallLayerB("OrdersBatchUpdate", dt);

            if (returnValue == null)
            {
                // 業務例外＝ロールバック済み。RowState を残してやり直せるようにする。
                this.BindGrid(dt);
                return;
            }

            dt.AcceptChanges();

            string message = "更新しました（挿入 " + returnValue.InsertCount
                + " 件／更新 " + returnValue.UpdateCount
                + " 件／削除 " + returnValue.DeleteCount + " 件）。";

            // ★ IDENTITY の採番値は DataTable に戻らないので、同じ条件・同じページで取り直す。
            this.SearchPage(message);
        }

        #endregion

        #region コンテンツ ページ上のボタンのイベント

        /// <summary>btnAddRow（行追加＝空行を足す）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_btnAddRow_Click(FxEventArgs fxEventArgs)
        {
            DataTable dt = this.EditingTable;
            if (dt == null) { this.lblMessage.Text = "先に［検索］を実行して下さい。"; return string.Empty; }

            this.ReadGridIntoTable(dt, -1);

            // Orders は OrderID（IDENTITY）以外すべて NULL 許容なので、空行のままでも INSERT できる。
            dt.Rows.Add(dt.NewRow());

            this.EditingTable = dt;
            this.BindGrid(dt);
            this.lblMessage.Text = "行を追加しました（［バッチ更新］でDBに反映されます）。";
            return string.Empty;
        }

        #endregion

        #region GridView のイベント

        /// <summary>gvwOrders の RowCommand イベント（行ごとの［更新］［削除］）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_gvwOrders_RowCommand(FxEventArgs fxEventArgs)
        {
            DataTable dt = this.EditingTable;
            if (dt == null) { this.lblMessage.Text = "先に［検索］を実行して下さい。"; return string.Empty; }

            int displayIndex = int.Parse(fxEventArgs.PostBackValue);

            switch (fxEventArgs.InnerButtonID)
            {
                case "Update":
                    this.ReadGridIntoTable(dt, displayIndex);
                    this.lblMessage.Text = "行を更新しました（［バッチ更新］でDBに反映されます）。";
                    break;

                case "Delete":
                    this.ReadGridIntoTable(dt, -1);
                    DataRow target = OrdersB.GetDataRowForDisplayIndex(dt, displayIndex);
                    if (target != null) { target.Delete(); }   // ★ Rows.Remove ではない
                    this.lblMessage.Text = "行を削除しました（［バッチ更新］でDBに反映されます）。";
                    break;

                default:
                    break;
            }

            this.EditingTable = dt;
            this.BindGrid(dt);
            return string.Empty;
        }

        /// <summary>
        /// gvwOrders の RowDataBound イベント（行内 DDL の選択肢と選択値を設定する）
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        /// <remarks>
        /// ★ RowDataBound はフレームワークの自動結線対象外なので .aspx の OnRowDataBound で結線する
        ///   （表示専用の処理なので、フレームワークの例外処理・ログを通らなくても影響が小さい）。
        /// </remarks>
        protected void gvwOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) { return; }

            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) { return; }

            DataSet masters = this.Masters;
            if (masters == null) { return; }

            OrdersB.BindDdl((DropDownList)e.Row.FindControl("CustomerID"), masters.Tables["Customers"],
                "CustomerID", "CompanyName", OrdersB.CellText(drv.Row, "CustomerID"));
            OrdersB.BindDdl((DropDownList)e.Row.FindControl("EmployeeID"), masters.Tables["Employees"],
                "EmployeeID", "EmployeeName", OrdersB.CellText(drv.Row, "EmployeeID"));
            OrdersB.BindDdl((DropDownList)e.Row.FindControl("ShipVia"), masters.Tables["Shippers"],
                "ShipperID", "CompanyName", OrdersB.CellText(drv.Row, "ShipVia"));

            OrdersB.SetTextBox(e.Row, "OrderDate", OrdersB.CellText(drv.Row, "OrderDate"));
            OrdersB.SetTextBox(e.Row, "RequiredDate", OrdersB.CellText(drv.Row, "RequiredDate"));
            OrdersB.SetTextBox(e.Row, "ShippedDate", OrdersB.CellText(drv.Row, "ShippedDate"));
            OrdersB.SetTextBox(e.Row, "Freight", OrdersB.CellText(drv.Row, "Freight"));
            OrdersB.SetTextBox(e.Row, "ShipName", OrdersB.CellText(drv.Row, "ShipName"));
            OrdersB.SetTextBox(e.Row, "ShipAddress", OrdersB.CellText(drv.Row, "ShipAddress"));
            OrdersB.SetTextBox(e.Row, "ShipCity", OrdersB.CellText(drv.Row, "ShipCity"));
            OrdersB.SetTextBox(e.Row, "ShipRegion", OrdersB.CellText(drv.Row, "ShipRegion"));
            OrdersB.SetTextBox(e.Row, "ShipPostalCode", OrdersB.CellText(drv.Row, "ShipPostalCode"));
            OrdersB.SetTextBox(e.Row, "ShipCountry", OrdersB.CellText(drv.Row, "ShipCountry"));
        }

        #endregion

        #region 検索・バインド

        /// <summary>条件検索を実行してページを表示する</summary>
        /// <param name="message">表示するメッセージ</param>
        private void SearchPage(string message)
        {
            OrdersReturnValue returnValue = this.CallLayerB("OrdersSearch", null);
            if (returnValue == null) { return; }

            this.EditingTable = returnValue.Orders;
            this.Session["OrdersTotalCount"] = returnValue.TotalCount;

            this.BindGrid(returnValue.Orders);
            this.lblMessage.Text = message + "（" + returnValue.TotalCount + " 件中 "
                + this.PageIndex + " / " + Math.Max(this.TotalPages, 1) + " ページ）。";
        }

        /// <summary>総ページ数</summary>
        private int TotalPages
        {
            get
            {
                object v = this.Session["OrdersTotalCount"];
                int total = (v == null) ? 0 : (int)v;
                return (total + OrdersB.PageSize - 1) / OrdersB.PageSize;
            }
        }

        /// <summary>GridView に DataTable をバインドする</summary>
        /// <param name="dt">編集中の DataTable</param>
        private void BindGrid(DataTable dt)
        {
            this.gvwOrders.DataSource = dt;
            this.gvwOrders.DataBind();

            object v = this.Session["OrdersTotalCount"];
            int total = (v == null) ? 0 : (int)v;

            this.lblPager.Text = "全 " + total + " 件／" + this.PageIndex + " / "
                + Math.Max(this.TotalPages, 1) + " ページ（" + OrdersB.PageSize + " 件ずつ）"
                + (OrdersB.HasPendingChanges(dt) ? "　※ 編集中のためページングは停止しています。" : "");

            this.SetMainButtons();
        }

        /// <summary>検索条件の DDL を作る</summary>
        private void BindSearchConditionDdl()
        {
            DataSet masters = this.Masters;
            if (masters == null) { return; }

            OrdersB.BindDdl(this.ddlCustomerID, masters.Tables["Customers"], "CustomerID", "CompanyName", "");
            OrdersB.BindDdl(this.ddlEmployeeID, masters.Tables["Employees"], "EmployeeID", "EmployeeName", "");
            OrdersB.BindDdl(this.ddlShipVia, masters.Tables["Shippers"], "ShipperID", "CompanyName", "");

            // ★ BindDdl は先頭に空の選択肢を入れるので、検索条件側はそれを
            //   「（すべて）」に置き換える（Insert すると空選択肢が二重になる）。
            OrdersB.ReplaceBlankWithAll(this.ddlCustomerID);
            OrdersB.ReplaceBlankWithAll(this.ddlEmployeeID);
            OrdersB.ReplaceBlankWithAll(this.ddlShipVia);
            this.ddlCustomerID.SelectedIndex = 0;
            this.ddlEmployeeID.SelectedIndex = 0;
            this.ddlShipVia.SelectedIndex = 0;
        }

        /// <summary>検索条件 DDL の先頭の空選択肢を「（すべて）」に置き換える</summary>
        /// <param name="ddl">対象の DDL</param>
        private static void ReplaceBlankWithAll(DropDownList ddl)
        {
            if (ddl == null || ddl.Items.Count == 0) { return; }

            if (ddl.Items[0].Value == "") { ddl.Items.RemoveAt(0); }
            ddl.Items.Insert(0, new System.Web.UI.WebControls.ListItem("（すべて）", ""));
        }

        /// <summary>DDL にマスタをバインドして選択値を設定する</summary>
        /// <param name="ddl">対象の DDL</param>
        /// <param name="master">マスタ</param>
        /// <param name="valueField">値の列</param>
        /// <param name="textField">表示の列</param>
        /// <param name="selectedValue">選択値</param>
        private static void BindDdl(DropDownList ddl, DataTable master, string valueField, string textField, string selectedValue)
        {
            if (ddl == null || master == null) { return; }

            ddl.Items.Clear();
            ddl.Items.Add(new System.Web.UI.WebControls.ListItem("", ""));

            foreach (DataRow r in master.Rows)
            {
                ddl.Items.Add(new System.Web.UI.WebControls.ListItem(Convert.ToString(r[textField]), Convert.ToString(r[valueField]).Trim()));
            }

            System.Web.UI.WebControls.ListItem hit = ddl.Items.FindByValue(selectedValue ?? "");
            if (hit != null) { ddl.SelectedValue = hit.Value; }
        }

        /// <summary>行内の TextBox に値を設定する</summary>
        /// <param name="gvr">GridView の行</param>
        /// <param name="controlId">コントロール ID</param>
        /// <param name="value">値</param>
        private static void SetTextBox(GridViewRow gvr, string controlId, string value)
        {
            TextBox tb = (TextBox)gvr.FindControl(controlId);
            if (tb != null) { tb.Text = value; }
        }

        #endregion

        #region グリッド ⇔ DataTable

        /// <summary>グリッドのセル値を DataTable へ読み戻す</summary>
        /// <param name="dt">編集中の DataTable</param>
        /// <param name="targetDisplayIndex">確定する既存行の表示 index（-1＝追加行のみ）</param>
        private void ReadGridIntoTable(DataTable dt, int targetDisplayIndex)
        {
            foreach (GridViewRow gvr in this.gvwOrders.Rows)
            {
                if (gvr.RowType != DataControlRowType.DataRow) { continue; }

                DataRow dr = OrdersB.GetDataRowForDisplayIndex(dt, gvr.RowIndex);
                if (dr == null) { continue; }
                if (dr.RowState != DataRowState.Added && gvr.RowIndex != targetDisplayIndex) { continue; }

                OrdersB.SetIfChanged(dr, "CustomerID", OrdersB.GetDdlValue(gvr, "CustomerID"));
                OrdersB.SetIfChanged(dr, "EmployeeID", OrdersB.GetDdlValue(gvr, "EmployeeID"));
                OrdersB.SetIfChanged(dr, "ShipVia", OrdersB.GetDdlValue(gvr, "ShipVia"));
                OrdersB.SetIfChanged(dr, "OrderDate", OrdersB.GetCellText(gvr, "OrderDate"));
                OrdersB.SetIfChanged(dr, "RequiredDate", OrdersB.GetCellText(gvr, "RequiredDate"));
                OrdersB.SetIfChanged(dr, "ShippedDate", OrdersB.GetCellText(gvr, "ShippedDate"));
                OrdersB.SetIfChanged(dr, "Freight", OrdersB.GetCellText(gvr, "Freight"));
                OrdersB.SetIfChanged(dr, "ShipName", OrdersB.GetCellText(gvr, "ShipName"));
                OrdersB.SetIfChanged(dr, "ShipAddress", OrdersB.GetCellText(gvr, "ShipAddress"));
                OrdersB.SetIfChanged(dr, "ShipCity", OrdersB.GetCellText(gvr, "ShipCity"));
                OrdersB.SetIfChanged(dr, "ShipRegion", OrdersB.GetCellText(gvr, "ShipRegion"));
                OrdersB.SetIfChanged(dr, "ShipPostalCode", OrdersB.GetCellText(gvr, "ShipPostalCode"));
                OrdersB.SetIfChanged(dr, "ShipCountry", OrdersB.GetCellText(gvr, "ShipCountry"));
            }
        }

        /// <summary>行内の TextBox の値を取得する</summary>
        private static string GetCellText(GridViewRow gvr, string controlId)
        {
            TextBox tb = (TextBox)gvr.FindControl(controlId);
            return (tb == null) ? "" : tb.Text;
        }

        /// <summary>行内の DDL の値を取得する</summary>
        private static string GetDdlValue(GridViewRow gvr, string controlId)
        {
            DropDownList ddl = (DropDownList)gvr.FindControl(controlId);
            return (ddl == null) ? "" : ddl.SelectedValue;
        }

        /// <summary>表示 index に対応する DataRow を返す（Deleted を飛ばして数える）</summary>
        private static DataRow GetDataRowForDisplayIndex(DataTable dt, int displayIndex)
        {
            int i = -1;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState == DataRowState.Deleted) { continue; }
                if (++i == displayIndex) { return dr; }
            }
            return null;
        }

        /// <summary>DataRow の値を画面表示用の文字列にする</summary>
        private static string CellText(DataRow dr, string columnName)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return ""; }

            object v = dr[columnName];
            if (v == null || v == DBNull.Value) { return ""; }
            if (v is DateTime) { return ((DateTime)v).ToString("yyyy/MM/dd"); }
            return Convert.ToString(v).Trim();
        }

        /// <summary>値が変わっているときだけ、列の型に変換して代入する</summary>
        /// <remarks>
        /// ★ Orders の DataTable は型付き（int / DateTime / decimal）。
        ///   文字列をそのまま代入すると例外になるので列の型へ変換する。
        /// </remarks>
        private static void SetIfChanged(DataRow dr, string columnName, string newValue)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return; }

            string current = OrdersB.CellText(dr, columnName);
            string edited = (newValue ?? "").Trim();

            if (current == edited) { return; }

            if (edited.Length == 0) { dr[columnName] = DBNull.Value; return; }

            Type t = dr.Table.Columns[columnName].DataType;

            try
            {
                if (t == typeof(int)) { dr[columnName] = int.Parse(edited); }
                else if (t == typeof(decimal)) { dr[columnName] = decimal.Parse(edited); }
                else if (t == typeof(DateTime)) { dr[columnName] = DateTime.Parse(edited); }
                else { dr[columnName] = edited; }
            }
            catch (FormatException)
            {
                // 変換できない入力は無視する（元の値のまま）
            }
        }

        /// <summary>編集中（未反映の変更がある）か</summary>
        private static bool HasPendingChanges(DataTable dt)
        {
            if (dt == null) { return false; }

            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState != DataRowState.Unchanged) { return true; }
            }
            return false;
        }

        #endregion

        #region Ｂ層呼び出し

        /// <summary>Ｂ層を呼び出す</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="orders">バッチ更新対象（参照系は null）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private OrdersReturnValue CallLayerB(string methodName, DataTable orders)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdersParameterValue parameterValue = new OrdersParameterValue(
                this.ContentPageFileNoEx, "-", methodName, "SQL", this.UserInfo);

            parameterValue.CustomerID = (this.ddlCustomerID == null) ? "" : this.ddlCustomerID.SelectedValue;
            parameterValue.EmployeeID = (this.ddlEmployeeID == null) ? "" : this.ddlEmployeeID.SelectedValue;
            parameterValue.ShipVia = (this.ddlShipVia == null) ? "" : this.ddlShipVia.SelectedValue;
            parameterValue.ShipCountry = (this.txtShipCountry == null) ? "" : this.txtShipCountry.Text;
            parameterValue.PageIndex = this.PageIndex;
            parameterValue.PageSize = OrdersB.PageSize;
            parameterValue.Orders = orders;

            OrdersReturnValue returnValue = (OrdersReturnValue)new LayerB()
                .DoBusinessLogic(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行---------------------------------------------------------

            if (returnValue.ErrorFlag)
            {
                this.lblMessage.Text = returnValue.ErrorMessage;
                return null;
            }
            return returnValue;
        }

        #endregion
    }
}
