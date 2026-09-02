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
            OrdReturnValue masters = this.CallLayerB("OrdMasters", null, null);
            if (masters != null)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(masters.Customers.Copy());
                ds.Tables.Add(masters.Employees.Copy());
                ds.Tables.Add(masters.Shippers.Copy());
                this.Masters = ds;
            }
            this.BindInputDdl();

            // --- ② 詳細（自動生成Dao の参照＝Ｒ）。追加モードは 0 件＝スキーマだけ戻る ---
            OrdReturnValue detail = this.CallLayerB("OrdDetailedView", this.TargetOrderId, null);
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
            this.SetMainButtons();
        }

        /// <summary>ポストバック時の処理</summary>
        protected override void UOC_FormInit_PostBack()
        {
            // ＤＤＬ は ViewState で復元されるので作り直さない
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

            OrdReturnValue returnValue = this.CallLayerB(methodName, this.TargetOrderId, dt);
            if (returnValue == null)
            {
                // 業務例外＝ロールバック済み。入力を残してやり直せるようにする。
                return;
            }

            dt.AcceptChanges();
            this.EditingOrder = dt;

            string message = caption + "しました（"
                + (returnValue.InsertCount + returnValue.UpdateCount + returnValue.DeleteCount) + " 件）。";

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
        private OrdReturnValue CallLayerB(string methodName, string orderId, DataTable order)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdParameterValue parameterValue = new OrdParameterValue(
                this.ContentPageFileNoEx, "-", methodName, "SQL", this.UserInfo);

            parameterValue.OrderID = orderId;
            parameterValue.Order = order;

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
