//**********************************************************************************
//* 受注管理（Ord）：画面Ｂ＝詳細・更新（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdDetailedView
//* クラス日本語名  ：受注 詳細・更新（画面Ｂ）
//*
//* 作成日時        ：2026/09/02
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/09/02  生技              新規作成
//**********************************************************************************

using System;
using System.Globalization;
using System.Data;
using System.Web.UI.WebControls;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.CustomControl;
using Touryo.Infrastructure.Framework.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace WebForms_Sample.Aspx.Ord
{
    /// <summary>受注 詳細・更新（画面Ｂ）</summary>
    /// <remarks>
    /// 仕様：
    /// ・初期処理でマスタ・テーブルを取得し「マスタ・テーブル値入力用ＤＤＬ」を生成する。
    /// ・画面Ａの詳細ボタンから来たときは自動生成Dao の参照（Ｒ）で詳細表示し、
    ///   ＵＤ（更新・削除）を活性にする。追加ボタンから来たときはＣ（追加）を活性にする。
    /// ・ＣＵＤボタンで YES/NO 確認ダイアログを表示し、YES 押下後に処理を実行する。
    /// ★ 編集中の1行は DataTable のまま Session に持つ（net48 は binary 直列化でそのまま置ける）。
    ///   取得時の値が DataRowVersion.Original に残るので、Ｂ層で楽観排他が成立する。
    /// ★ 確認ダイアログの後処理は「次のポストバック」で走るので、
    ///   ダイアログを出す前に画面の入力値を DataTable へ確定しておく。
    /// </remarks>
    public partial class OrdDetailedView : MyBaseController
    {
        #region Session のキー

        /// <summary>編集中の1行（DataTable）</summary>
        private const string SessionKeyOrder = "OrdEditingOrder";

        /// <summary>編集中の明細（DataTable）</summary>
        private const string SessionKeyDetails = "OrdEditingDetails";

        /// <summary>ＤＤＬ用のマスタ（DataSet）</summary>
        private const string SessionKeyMasters = "OrdMasters";

        /// <summary>画面Ａから渡された対象 OrderID（空＝追加モード）</summary>
        private const string SessionKeyTargetId = "OrdTargetOrderID";

        /// <summary>追加モードか</summary>
        private const string SessionKeyIsNew = "OrdIsNew";

        /// <summary>ＣＵＤが済んで、この画面での操作を止めたか</summary>
        private const string SessionKeyDone = "OrdCudDone";

        #endregion

        #region Session の出し入れ

        /// <summary>編集中の1行</summary>
        private DataTable EditingOrder
        {
            get { return this.Session[OrdDetailedView.SessionKeyOrder] as DataTable; }
            set
            {
                if (value == null) { this.Session.Remove(OrdDetailedView.SessionKeyOrder); }
                else { this.Session[OrdDetailedView.SessionKeyOrder] = value; }
            }
        }

        /// <summary>編集中の明細</summary>
        private DataTable EditingDetails
        {
            get { return this.Session[OrdDetailedView.SessionKeyDetails] as DataTable; }
            set
            {
                if (value == null) { this.Session.Remove(OrdDetailedView.SessionKeyDetails); }
                else { this.Session[OrdDetailedView.SessionKeyDetails] = value; }
            }
        }

        /// <summary>ＤＤＬ用のマスタ</summary>
        private DataSet Masters
        {
            get { return this.Session[OrdDetailedView.SessionKeyMasters] as DataSet; }
            set { this.Session[OrdDetailedView.SessionKeyMasters] = value; }
        }

        /// <summary>対象 OrderID（空＝追加モード）</summary>
        private string TargetOrderId
        {
            get { return Convert.ToString(this.Session[OrdDetailedView.SessionKeyTargetId]); }
        }

        /// <summary>追加モードか</summary>
        private bool IsNew
        {
            get
            {
                object v = this.Session[OrdDetailedView.SessionKeyIsNew];
                return (v != null) && (bool)v;
            }
            set { this.Session[OrdDetailedView.SessionKeyIsNew] = value; }
        }

        /// <summary>ＣＵＤが済んだか</summary>
        private bool CudDone
        {
            get
            {
                object v = this.Session[OrdDetailedView.SessionKeyDone];
                return (v != null) && (bool)v;
            }
            set { this.Session[OrdDetailedView.SessionKeyDone] = value; }
        }

        #endregion

        #region ページ ロードの共通処理（UOC メソッド）

        /// <summary>初期表示時の処理</summary>
        protected override void UOC_FormInit()
        {
            this.CudDone = false;

            // --- ① マスタ・テーブルの取得 → 入力用ＤＤＬ 生成（仕様） ---
            OrdReturnValue masters = this.CallLayerB("OrdMasters", null, null, null);
            if (masters != null)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(masters.Customers.Copy());
                ds.Tables.Add(masters.Employees.Copy());
                ds.Tables.Add(masters.Shippers.Copy());
                ds.Tables.Add(masters.Products.Copy());
                this.Masters = ds;
            }
            this.BindInputDdl();

            // --- ② 詳細（自動生成Dao の参照＝Ｒ）。追加モードは 0 件＝スキーマだけ戻る ---
            OrdReturnValue detail = this.CallLayerB("OrdDetailedView", this.TargetOrderId, null, null);
            if (detail == null) { this.SetMainButtons(); return; }

            DataTable dt = detail.Order;

            if (dt.Rows.Count == 0)
            {
                // 追加モード：空行を1行足す（RowState＝Added）
                this.IsNew = true;
                dt.Rows.Add(dt.NewRow());
            }
            else
            {
                this.IsNew = false;
            }

            this.EditingOrder = dt;
            this.RowToScreen(dt.Rows[0]);

            // --- ③ 明細（Order Details）の参照（Ｒ）結果をグリッドにバインドする ---
            this.EditingDetails = detail.OrderDetails;
            this.BindDetailGrid(detail.OrderDetails);

            this.SetMainButtons();
        }

        /// <summary>ポストバック時の処理</summary>
        protected override void UOC_FormInit_PostBack()
        {
            // ＤＤＬ は ViewState で復元されるので作り直さない。
            // ★ 明細グリッドも「ポストバックのたびに再バインドしてはいけない」。
            //   DataBind() は行内コントロールを作り直すので、ポストされた入力値が
            //   読み戻し（ReadDetailGrid）より前に捨てられる。
            //   ＝実測では明細2行が既定値のまま INSERT され PRIMARY KEY 違反になった。
            //   行内コントロールの値は ViewState ＋ ポスト値から復元されるので、
            //   バインドは「行追加／行削除／反映後の再表示」など明示的な場面だけで行う。
            this.SetMainButtons();
        }

        /// <summary>共通仕様：フッタ部のメイン ボタン5つを設定する</summary>
        /// <remarks>
        /// 仕様：詳細ボタンから遷移＝ＵＤ（更新・削除）を活性、
        ///       追加ボタンから遷移＝Ｃ（追加）を活性にする。
        /// </remarks>
        private void SetMainButtons()
        {
            WebCustomButton btn1 = (WebCustomButton)this.GetMasterWebControl("btnMButton1");
            WebCustomButton btn2 = (WebCustomButton)this.GetMasterWebControl("btnMButton2");
            WebCustomButton btn3 = (WebCustomButton)this.GetMasterWebControl("btnMButton3");
            WebCustomButton btn4 = (WebCustomButton)this.GetMasterWebControl("btnMButton4");
            WebCustomButton btn5 = (WebCustomButton)this.GetMasterWebControl("btnMButton5");

            bool isNew = this.IsNew;
            bool done = this.CudDone;

            btn1.Text = "追加"; btn1.Enabled = isNew && !done;
            btn2.Text = "更新"; btn2.Enabled = !isNew && !done;
            btn3.Text = "削除"; btn3.Enabled = !isNew && !done;

            // 使わないボタンは disable にする（共通仕様）
            btn4.Text = "－";   btn4.Enabled = false;

            btn5.Text = "戻る"; btn5.Enabled = true;
        }

        #endregion

        #region マスタ ページ上のボタンのイベント

        /// <summary>btnMButton1（追加＝Ｃ）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton1_Click(FxEventArgs fxEventArgs)
        {
            return this.ConfirmCud("追加します。よろしいですか？");
        }

        /// <summary>btnMButton2（更新＝Ｕ）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton2_Click(FxEventArgs fxEventArgs)
        {
            return this.ConfirmCud("更新します。よろしいですか？");
        }

        /// <summary>btnMButton3（削除＝Ｄ）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton3_Click(FxEventArgs fxEventArgs)
        {
            return this.ConfirmCud("削除します。よろしいですか？");
        }

        /// <summary>btnMButton5（戻る＝画面Ａへ）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL</returns>
        protected string UOC_testBlankScreen_btnMButton5_Click(FxEventArgs fxEventArgs)
        {
            this.EditingOrder = null;
            return "~/Aspx/Ord/OrdListSearch.aspx";
        }

        /// <summary>ＣＵＤの確認ダイアログを出す</summary>
        /// <param name="question">確認ダイアログの本文</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        private string ConfirmCud(string question)
        {
            DataTable dt = this.EditingOrder;
            if (dt == null || dt.Rows.Count == 0)
            {
                this.lblMessage.Text = "処理対象のデータがありません。";
                return string.Empty;
            }

            // ★ 確認ダイアログの後処理は「次のポストバック」で走る＝ここで入力値を確定しておく。
            this.ScreenToRow(dt.Rows[0]);
            this.EditingOrder = dt;

            DataTable details = this.EditingDetails;
            this.ReadDetailGrid(details, -1);
            this.EditingDetails = details;

            // 共通仕様：確認ダイアログはフレームワークの ShowYesNoMessageDialog を使う
            this.ShowYesNoMessageDialog("OrdCud", question, "確認");
            return string.Empty;
        }

        #endregion

        #region 確認ダイアログの後処理

        /// <summary>YES/NO 確認ダイアログで YES が押されたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        /// <remarks>
        /// ★ どのボタンから開いたかは ButtonID で判別する
        ///   （ボタン履歴記録機能＝FxButtonhistoryMaxQueueLength が正の値であること）。
        /// </remarks>
        protected override void UOC_YesNoDialog_Yes_Click(FxEventArgs parentFxEventArgs)
        {
            switch (parentFxEventArgs.ButtonID)
            {
                case "btnMButton1":
                    this.ExecuteCud("OrdInsert", "追加");
                    break;

                case "btnMButton2":
                    this.ExecuteCud("OrdUpdate", "更新");
                    break;

                case "btnMButton3":
                    this.ExecuteCud("OrdDelete", "削除");
                    break;

                default:
                    break;
            }
        }

        /// <summary>YES/NO 確認ダイアログで NO が押されたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        protected override void UOC_YesNoDialog_No_Click(FxEventArgs parentFxEventArgs)
        {
            this.lblMessage.Text = "処理を中止しました。";
        }

        /// <summary>YES/NO 確認ダイアログが×で閉じられたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        protected override void UOC_YesNoDialog_X_Click(FxEventArgs parentFxEventArgs)
        {
            this.lblMessage.Text = "処理を中止しました。";
        }

        /// <summary>ＣＵＤを実行する</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="caption">結果メッセージ用の見出し</param>
        private void ExecuteCud(string methodName, string caption)
        {
            DataTable dt = this.EditingOrder;
            if (dt == null || dt.Rows.Count == 0)
            {
                this.lblMessage.Text = "処理対象のデータがありません。";
                return;
            }

            DataTable details = this.EditingDetails;

            OrdReturnValue returnValue = this.CallLayerB(methodName, this.TargetOrderId, dt, details);
            if (returnValue == null)
            {
                // 業務例外＝ロールバック済み。入力を残してやり直せるようにする。
                return;
            }

            dt.AcceptChanges();
            this.EditingOrder = dt;
            if (details != null) { details.AcceptChanges(); }
            this.EditingDetails = details;
            this.BindDetailGrid(details);

            string message = caption + "しました（"
                + (returnValue.InsertCount + returnValue.UpdateCount + returnValue.DeleteCount) + " 件"
                + "／明細 追加 " + returnValue.DetailInsertCount + " 件・更新 " + returnValue.DetailUpdateCount
                + " 件・削除 " + returnValue.DetailDeleteCount + " 件）。";

            // ★ 追加・削除の後は、この画面のＣＵＤを止める（同じ行を二重に追加／削除できないように）。
            //   更新は続けて行える（AcceptChanges で Original が現在値に揃うので楽観排他も成立する）。
            if (methodName != "OrdUpdate")
            {
                this.CudDone = true;
                message += "［戻る］で一覧に戻って下さい。";
            }

            this.lblMessage.Text = message;
            this.SetMainButtons();
        }

        #endregion

        #region 画面 ⇔ DataRow

        /// <summary>DataRow の値を画面に反映する</summary>
        /// <param name="dr">対象の DataRow</param>
        private void RowToScreen(DataRow dr)
        {
            this.txtOrderID.Text = OrdDetailedView.CellText(dr, "OrderID");

            OrdDetailedView.SelectValue(this.ddlCustomerID, OrdDetailedView.CellText(dr, "CustomerID"));
            OrdDetailedView.SelectValue(this.ddlEmployeeID, OrdDetailedView.CellText(dr, "EmployeeID"));
            OrdDetailedView.SelectValue(this.ddlShipVia, OrdDetailedView.CellText(dr, "ShipVia"));

            this.txtOrderDate.Text = OrdDetailedView.CellText(dr, "OrderDate");
            this.txtRequiredDate.Text = OrdDetailedView.CellText(dr, "RequiredDate");
            this.txtShippedDate.Text = OrdDetailedView.CellText(dr, "ShippedDate");
            this.txtFreight.Text = OrdDetailedView.CellText(dr, "Freight");
            this.txtShipName.Text = OrdDetailedView.CellText(dr, "ShipName");
            this.txtShipAddress.Text = OrdDetailedView.CellText(dr, "ShipAddress");
            this.txtShipCity.Text = OrdDetailedView.CellText(dr, "ShipCity");
            this.txtShipRegion.Text = OrdDetailedView.CellText(dr, "ShipRegion");
            this.txtShipPostalCode.Text = OrdDetailedView.CellText(dr, "ShipPostalCode");
            this.txtShipCountry.Text = OrdDetailedView.CellText(dr, "ShipCountry");
        }

        /// <summary>画面の入力値を DataRow に読み戻す</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <remarks>
        /// ★ 値が変わったときだけ代入する＝Original（取得時の値）を壊さず、
        ///   無駄な Modified も作らない。
        /// </remarks>
        private void ScreenToRow(DataRow dr)
        {
            OrdDetailedView.SetIfChanged(dr, "CustomerID", this.ddlCustomerID.SelectedValue);
            OrdDetailedView.SetIfChanged(dr, "EmployeeID", this.ddlEmployeeID.SelectedValue);
            OrdDetailedView.SetIfChanged(dr, "ShipVia", this.ddlShipVia.SelectedValue);

            OrdDetailedView.SetIfChanged(dr, "OrderDate", this.txtOrderDate.Text);
            OrdDetailedView.SetIfChanged(dr, "RequiredDate", this.txtRequiredDate.Text);
            OrdDetailedView.SetIfChanged(dr, "ShippedDate", this.txtShippedDate.Text);
            OrdDetailedView.SetIfChanged(dr, "Freight", this.txtFreight.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipName", this.txtShipName.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipAddress", this.txtShipAddress.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipCity", this.txtShipCity.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipRegion", this.txtShipRegion.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipPostalCode", this.txtShipPostalCode.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipCountry", this.txtShipCountry.Text);
        }

        /// <summary>DataRow の値を画面表示用の文字列にする</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <returns>表示用の文字列</returns>
        private static string CellText(DataRow dr, string columnName)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return ""; }

            object v = dr[columnName];
            if (v == null || v == DBNull.Value) { return ""; }
            if (v is DateTime) { return ((DateTime)v).ToString("yyyy/MM/dd"); }
            return Convert.ToString(v).Trim();
        }

        /// <summary>値が変わっているときだけ、列の型に変換して代入する</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <param name="newValue">画面の値（文字列）</param>
        /// <remarks>
        /// ★ Orders の DataTable は型付き（int / DateTime / decimal）。
        ///   文字列をそのまま代入すると例外になるので列の型へ変換する。
        /// ★ Orders は OrderID 以外すべて NULL 許容なので、空欄は DBNull にする。
        /// </remarks>
        private static void SetIfChanged(DataRow dr, string columnName, string newValue)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return; }

            string current = OrdDetailedView.CellText(dr, columnName);
            string edited = (newValue ?? "").Trim();

            if (current == edited) { return; }

            if (edited.Length == 0) { dr[columnName] = DBNull.Value; return; }

            Type t = dr.Table.Columns[columnName].DataType;

            try
            {
                // ★ 明細（Order Details）は short（Quantity）／float（Discount）／decimal（UnitPrice）
                //   なので、int / decimal の決め打ちでは足りない。列の型へ一般化して変換する。
                if (t == typeof(DateTime)) { dr[columnName] = DateTime.Parse(edited); }
                else if (t == typeof(string)) { dr[columnName] = edited; }
                else { dr[columnName] = Convert.ChangeType(edited, t, CultureInfo.InvariantCulture); }
            }
            catch (FormatException)
            {
                // 変換できない入力は無視する（元の値のまま）
            }
            catch (InvalidCastException)
            {
                // 変換できない入力は無視する（元の値のまま）
            }
            catch (OverflowException)
            {
                // 変換できない入力は無視する（元の値のまま）
            }
        }

        #endregion

        #region 明細（Order Details）

        /// <summary>btnAddDetail（明細行追加＝空行を足す）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_btnAddDetail_Click(FxEventArgs fxEventArgs)
        {
            DataTable details = this.EditingDetails;
            if (details == null) { this.lblMessage.Text = "画面を開き直して下さい。"; return string.Empty; }

            this.ReadDetailGrid(details, -1);

            // ★ Order Details は全列 NOT NULL、かつ CHECK 制約
            //   （Quantity > 0／Discount 0〜1／UnitPrice >= 0）がある。
            //   空行のままバッチ更新すると SqlException（515／547）になるので既定値を入れる。
            DataRow nr = details.NewRow();
            nr["OrderID"] = string.IsNullOrEmpty(this.TargetOrderId) ? 0 : int.Parse(this.TargetOrderId);
            nr["ProductID"] = OrdDetailedView.FirstProductId(this.Masters);
            nr["UnitPrice"] = 0m;
            nr["Quantity"] = (short)1;
            nr["Discount"] = 0f;
            details.Rows.Add(nr);

            this.EditingDetails = details;
            this.BindDetailGrid(details);
            this.lblMessage.Text = "明細行を追加しました（［追加］／［更新］でDBに反映されます）。";
            return string.Empty;
        }

        /// <summary>gvwDetails の RowCommand イベント（行ごとの［更新］［削除］）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_gvwDetails_RowCommand(FxEventArgs fxEventArgs)
        {
            DataTable details = this.EditingDetails;
            if (details == null) { this.lblMessage.Text = "画面を開き直して下さい。"; return string.Empty; }

            int displayIndex = int.Parse(fxEventArgs.PostBackValue);

            switch (fxEventArgs.InnerButtonID)
            {
                case "Update":
                    this.ReadDetailGrid(details, displayIndex);
                    this.lblMessage.Text = "明細行を更新しました（［追加］／［更新］でDBに反映されます）。";
                    break;

                case "Delete":
                    this.ReadDetailGrid(details, -1);
                    DataRow target = OrdDetailedView.GetDetailRowForDisplayIndex(details, displayIndex);
                    if (target != null) { target.Delete(); }   // ★ Rows.Remove ではない
                    this.lblMessage.Text = "明細行を削除しました（［追加］／［更新］でDBに反映されます）。";
                    break;

                default:
                    break;
            }

            this.EditingDetails = details;
            this.BindDetailGrid(details);
            return string.Empty;
        }

        /// <summary>gvwDetails の RowDataBound イベント（行内 ＤＤＬ とテキストに値を入れる）</summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        /// <remarks>
        /// ★ RowDataBound はフレームワークの自動結線対象外なので .aspx の OnRowDataBound で結線する
        ///   （表示専用の処理なので、フレームワークの例外処理・ログを通らなくても影響が小さい）。
        /// </remarks>
        protected void gvwDetails_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) { return; }

            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) { return; }

            DataSet masters = this.Masters;
            if (masters != null)
            {
                DropDownList ddl = (DropDownList)e.Row.FindControl("ProductID");
                OrdDetailedView.BindDdl(ddl, masters.Tables["Products"], "ProductID", "ProductName");
                OrdDetailedView.SelectValue(ddl, OrdDetailedView.CellText(drv.Row, "ProductID"));
            }

            OrdDetailedView.SetTextBox(e.Row, "UnitPrice", OrdDetailedView.CellText(drv.Row, "UnitPrice"));
            OrdDetailedView.SetTextBox(e.Row, "Quantity", OrdDetailedView.CellText(drv.Row, "Quantity"));
            OrdDetailedView.SetTextBox(e.Row, "Discount", OrdDetailedView.CellText(drv.Row, "Discount"));
        }

        /// <summary>明細グリッドに DataTable をバインドする</summary>
        /// <param name="details">編集中の明細</param>
        private void BindDetailGrid(DataTable details)
        {
            this.gvwDetails.DataSource = details;
            this.gvwDetails.DataBind();
        }

        /// <summary>グリッドのセル値を明細 DataTable へ読み戻す</summary>
        /// <param name="details">編集中の明細</param>
        /// <param name="targetDisplayIndex">確定する既存行の表示 index（-1＝追加行のみ）</param>
        /// <remarks>
        /// ★ 読み戻す行は「追加行は常に／既存行はその行の［更新］のとき／削除行は対象外」
        ///   （opentouryo-batch-update の Web 共通ルール）。
        /// </remarks>
        private void ReadDetailGrid(DataTable details, int targetDisplayIndex)
        {
            if (details == null) { return; }

            foreach (GridViewRow gvr in this.gvwDetails.Rows)
            {
                if (gvr.RowType != DataControlRowType.DataRow) { continue; }

                DataRow dr = OrdDetailedView.GetDetailRowForDisplayIndex(details, gvr.RowIndex);
                if (dr == null) { continue; }
                if (dr.RowState != DataRowState.Added && gvr.RowIndex != targetDisplayIndex) { continue; }

                OrdDetailedView.SetIfChanged(dr, "ProductID", OrdDetailedView.GetDdlValue(gvr, "ProductID"));
                OrdDetailedView.SetIfChanged(dr, "UnitPrice", OrdDetailedView.GetCellText(gvr, "UnitPrice"));
                OrdDetailedView.SetIfChanged(dr, "Quantity", OrdDetailedView.GetCellText(gvr, "Quantity"));
                OrdDetailedView.SetIfChanged(dr, "Discount", OrdDetailedView.GetCellText(gvr, "Discount"));
            }
        }

        /// <summary>表示 index に対応する DataRow を返す（Deleted を飛ばして数える）</summary>
        /// <param name="dt">明細</param>
        /// <param name="displayIndex">表示 index</param>
        /// <returns>DataRow（無ければ null）</returns>
        /// <remarks>★ Deleted 行は DefaultView から外れるので、そのままでは index がずれる。</remarks>
        private static DataRow GetDetailRowForDisplayIndex(DataTable dt, int displayIndex)
        {
            int i = -1;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState == DataRowState.Deleted) { continue; }
                if (++i == displayIndex) { return dr; }
            }
            return null;
        }

        /// <summary>行内の TextBox に値を設定する</summary>
        private static void SetTextBox(GridViewRow gvr, string controlId, string value)
        {
            TextBox tb = (TextBox)gvr.FindControl(controlId);
            if (tb != null) { tb.Text = value; }
        }

        /// <summary>行内の TextBox の値を取得する</summary>
        private static string GetCellText(GridViewRow gvr, string controlId)
        {
            TextBox tb = (TextBox)gvr.FindControl(controlId);
            return (tb == null) ? "" : tb.Text;
        }

        /// <summary>行内の ＤＤＬ の値を取得する</summary>
        private static string GetDdlValue(GridViewRow gvr, string controlId)
        {
            DropDownList ddl = (DropDownList)gvr.FindControl(controlId);
            return (ddl == null) ? "" : ddl.SelectedValue;
        }

        /// <summary>マスタの先頭の ProductID（新規明細行の既定値）</summary>
        private static object FirstProductId(DataSet masters)
        {
            if (masters == null || masters.Tables["Products"] == null
                || masters.Tables["Products"].Rows.Count == 0) { return DBNull.Value; }

            return masters.Tables["Products"].Rows[0]["ProductID"];
        }

        #endregion

        #region ＤＤＬ

        /// <summary>入力用の ＤＤＬ を作る</summary>
        private void BindInputDdl()
        {
            DataSet masters = this.Masters;
            if (masters == null) { return; }

            OrdDetailedView.BindDdl(this.ddlCustomerID, masters.Tables["Customers"], "CustomerID", "CompanyName");
            OrdDetailedView.BindDdl(this.ddlEmployeeID, masters.Tables["Employees"], "EmployeeID", "EmployeeName");
            OrdDetailedView.BindDdl(this.ddlShipVia, masters.Tables["Shippers"], "ShipperID", "CompanyName");
        }

        /// <summary>ＤＤＬ にマスタをバインドする（先頭は「（未設定）」＝NULL）</summary>
        /// <param name="ddl">対象の ＤＤＬ</param>
        /// <param name="master">マスタ</param>
        /// <param name="valueField">値の列</param>
        /// <param name="textField">表示の列</param>
        private static void BindDdl(DropDownList ddl, DataTable master, string valueField, string textField)
        {
            if (ddl == null || master == null) { return; }

            ddl.Items.Clear();
            ddl.Items.Add(new System.Web.UI.WebControls.ListItem("（未設定）", ""));

            foreach (DataRow r in master.Rows)
            {
                ddl.Items.Add(new System.Web.UI.WebControls.ListItem(
                    Convert.ToString(r[textField]), Convert.ToString(r[valueField]).Trim()));
            }
            ddl.SelectedIndex = 0;
        }

        /// <summary>ＤＤＬ を値で選択する（index ではない＝並び順はマスタの名称順のため）</summary>
        /// <param name="ddl">対象の ＤＤＬ</param>
        /// <param name="value">値</param>
        private static void SelectValue(DropDownList ddl, string value)
        {
            System.Web.UI.WebControls.ListItem hit = ddl.Items.FindByValue(value ?? "");
            ddl.SelectedValue = (hit != null) ? hit.Value : "";
        }

        #endregion

        #region Ｂ層呼び出し

        /// <summary>Ｂ層を呼び出す</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="orderId">対象の OrderID</param>
        /// <param name="order">ＣＵＤの対象（参照系は null）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private OrdReturnValue CallLayerB(string methodName, string orderId, DataTable order, DataTable details)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdParameterValue parameterValue = new OrdParameterValue(
                this.ContentPageFileNoEx, "-", methodName, "SQL", this.UserInfo);

            parameterValue.OrderID = orderId;
            parameterValue.Order = order;
            parameterValue.OrderDetails = details;

            // Ｂ層呼出し（Web はインプロセス直呼び）
            OrdReturnValue returnValue = (OrdReturnValue)new LayerB()
                .DoBusinessLogic(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行---------------------------------------------------------

            if (returnValue.ErrorFlag)
            {
                // 業務例外（業務続行可能なエラー）
                this.lblMessage.Text = returnValue.ErrorMessage;
                return null;
            }
            return returnValue;
        }

        #endregion
    }
}
