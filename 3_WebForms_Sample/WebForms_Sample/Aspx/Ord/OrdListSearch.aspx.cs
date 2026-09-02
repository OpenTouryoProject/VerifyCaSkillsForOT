//**********************************************************************************
//* 受注管理（Ord）：画面Ａ＝条件検索一覧（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdListSearch
//* クラス日本語名  ：受注 条件検索一覧（画面Ａ）
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
    /// <summary>受注 条件検索一覧（画面Ａ）</summary>
    /// <remarks>
    /// 仕様：
    /// ・検索条件を入力可能にし、Ｂ層（Ｄ層は共通Dao）の条件検索を実行して一覧に表示する。
    ///   一覧の表示値はＳＱＬでマスタ・テーブルと JOIN して変換済み。
    /// ・［追加］（画面遷移ボタン）で画面Ｂを「追加＝Ｃ」モードで開く。
    /// ・行の［詳細］で画面Ｂを「詳細＝Ｒ／更新・削除＝ＵＤ」モードで開く。
    /// </remarks>
    public partial class OrdListSearch : MyBaseController
    {
        /// <summary>1ページの表示件数</summary>
        private const int PageSize = 20;

        #region Session のキー

        /// <summary>ＤＤＬ用のマスタ（DataSet）</summary>
        private const string SessionKeyMasters = "OrdMasters";

        /// <summary>現在のページ番号</summary>
        private const string SessionKeyPage = "OrdPageIndex";

        /// <summary>総件数</summary>
        private const string SessionKeyTotal = "OrdTotalCount";

        /// <summary>検索条件（画面Ｂから戻ったときに復元する）</summary>
        private const string SessionKeyCondition = "OrdCondition";

        /// <summary>画面Ｂに渡す対象 OrderID（空＝追加モード）</summary>
        private const string SessionKeyTargetId = "OrdTargetOrderID";

        #endregion

        #region Session の出し入れ

        /// <summary>ＤＤＬ用のマスタ</summary>
        private DataSet Masters
        {
            get { return this.Session[OrdListSearch.SessionKeyMasters] as DataSet; }
            set { this.Session[OrdListSearch.SessionKeyMasters] = value; }
        }

        /// <summary>現在のページ番号（1 起算）</summary>
        private int PageIndex
        {
            get
            {
                object v = this.Session[OrdListSearch.SessionKeyPage];
                return (v == null) ? 1 : (int)v;
            }
            set { this.Session[OrdListSearch.SessionKeyPage] = value; }
        }

        /// <summary>総件数</summary>
        private int TotalCount
        {
            get
            {
                object v = this.Session[OrdListSearch.SessionKeyTotal];
                return (v == null) ? 0 : (int)v;
            }
            set { this.Session[OrdListSearch.SessionKeyTotal] = value; }
        }

        /// <summary>総ページ数</summary>
        private int TotalPages
        {
            get { return (this.TotalCount + OrdListSearch.PageSize - 1) / OrdListSearch.PageSize; }
        }

        #endregion

        #region ページ ロードの共通処理（UOC メソッド）

        /// <summary>初期表示時の処理</summary>
        /// <remarks>
        /// ★ 画面Ｂから戻ってきたときは Session の検索条件を復元して再検索する
        ///   （Web は画面遷移で入力値が消えるため）。
        /// </remarks>
        protected override void UOC_FormInit()
        {
            // ＤＤＬ用のマスタを取得して検索条件の ＤＤＬ を作る
            OrdReturnValue returnValue = this.CallLayerB("OrdMasters", null, null);
            if (returnValue != null)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(returnValue.Customers.Copy());
                ds.Tables.Add(returnValue.Employees.Copy());
                ds.Tables.Add(returnValue.Shippers.Copy());
                this.Masters = ds;
            }

            this.BindSearchConditionDdl();

            string[] condition = this.Session[OrdListSearch.SessionKeyCondition] as string[];
            if (condition == null)
            {
                this.PageIndex = 1;
                this.TotalCount = 0;
            }
            else
            {
                OrdListSearch.SelectValue(this.ddlCustomerID, condition[0]);
                OrdListSearch.SelectValue(this.ddlEmployeeID, condition[1]);
                OrdListSearch.SelectValue(this.ddlShipVia, condition[2]);
                this.txtShipCountry.Text = condition[3];
                this.SearchPage("最新の一覧を取得しました");
            }

            this.SetMainButtons();
        }

        /// <summary>ポストバック時の処理</summary>
        protected override void UOC_FormInit_PostBack()
        {
            this.SetMainButtons();
        }

        /// <summary>共通仕様：フッタ部のメイン ボタン5つを設定する</summary>
        private void SetMainButtons()
        {
            WebCustomButton btn1 = (WebCustomButton)this.GetMasterWebControl("btnMButton1");
            WebCustomButton btn2 = (WebCustomButton)this.GetMasterWebControl("btnMButton2");
            WebCustomButton btn3 = (WebCustomButton)this.GetMasterWebControl("btnMButton3");
            WebCustomButton btn4 = (WebCustomButton)this.GetMasterWebControl("btnMButton4");
            WebCustomButton btn5 = (WebCustomButton)this.GetMasterWebControl("btnMButton5");

            btn1.Text = "検索";       btn1.Enabled = true;
            btn2.Text = "追加";       btn2.Enabled = true;

            // 不要なボタンは disable にする（先頭／最終ページではページ移動できない）
            btn3.Text = "前ページ";   btn3.Enabled = this.PageIndex > 1;
            btn4.Text = "次ページ";   btn4.Enabled = this.PageIndex < this.TotalPages;

            btn5.Text = "メニューへ"; btn5.Enabled = true;
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

        /// <summary>btnMButton2（追加＝画面Ｂへ遷移／Ｃモード）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL</returns>
        /// <remarks>仕様：画面Ａの追加ボタンから遷移した場合、画面ＢのＣ（追加）を活性にする。</remarks>
        protected string UOC_testBlankScreen_btnMButton2_Click(FxEventArgs fxEventArgs)
        {
            this.SaveCondition();

            // 対象 OrderID を空にする＝画面Ｂは「追加」モードになる
            this.Session[OrdListSearch.SessionKeyTargetId] = "";

            // 遷移先は SCDefinition（画面遷移定義）に定義しておく必要がある
            // （FxScreenTransitionCheck = on のため、未定義の遷移は拒否される）。
            return "~/Aspx/Ord/OrdDetailedView.aspx";
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

        /// <summary>btnMButton5（メニューへ）のクリック イベント</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL</returns>
        protected string UOC_testBlankScreen_btnMButton5_Click(FxEventArgs fxEventArgs)
        {
            this.Session.Remove(OrdListSearch.SessionKeyCondition);
            return "~/Aspx/start/menu.aspx";
        }

        /// <summary>ページを移動する</summary>
        /// <param name="targetPage">移動先のページ番号（1 起算）</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        private string MovePage(int targetPage)
        {
            if (targetPage < 1) { targetPage = 1; }
            this.PageIndex = targetPage;
            this.SearchPage("ページを移動しました");
            return string.Empty;
        }

        #endregion

        #region GridView のイベント（行の［詳細］）

        /// <summary>gvwOrders の RowCommand イベント（行の［詳細］）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL</returns>
        /// <remarks>仕様：画面Ａの詳細ボタンから遷移した場合、画面ＢのＵＤ（更新・削除）を活性にする。</remarks>
        protected string UOC_gvwOrders_RowCommand(FxEventArgs fxEventArgs)
        {
            if (fxEventArgs.InnerButtonID != "Detail") { return string.Empty; }

            int displayIndex = int.Parse(fxEventArgs.PostBackValue);
            if (displayIndex < 0 || this.gvwOrders.Rows.Count <= displayIndex) { return string.Empty; }

            // 一覧の1列目（OrderID）を対象キーとして画面Ｂへ渡す
            string orderId = this.gvwOrders.Rows[displayIndex].Cells[0].Text;

            this.SaveCondition();
            this.Session[OrdListSearch.SessionKeyTargetId] = orderId;

            return "~/Aspx/Ord/OrdDetailedView.aspx";
        }

        #endregion

        #region 検索・バインド

        /// <summary>条件検索を実行してページを表示する</summary>
        /// <param name="message">表示するメッセージ</param>
        private void SearchPage(string message)
        {
            OrdReturnValue returnValue = this.CallLayerB("OrdListSearch", null, null);
            if (returnValue == null) { return; }

            this.TotalCount = returnValue.TotalCount;

            this.gvwOrders.DataSource = returnValue.Orders;
            this.gvwOrders.DataBind();

            this.lblPager.Text = "全 " + this.TotalCount + " 件／" + this.PageIndex + " / "
                + Math.Max(this.TotalPages, 1) + " ページ（" + OrdListSearch.PageSize + " 件ずつ）";
            this.lblMessage.Text = message + "（" + this.TotalCount + " 件中 "
                + this.PageIndex + " / " + Math.Max(this.TotalPages, 1) + " ページ）。";

            this.SetMainButtons();
        }

        /// <summary>検索条件を Session に退避する（画面Ｂから戻ったときに復元する）</summary>
        private void SaveCondition()
        {
            this.Session[OrdListSearch.SessionKeyCondition] = new string[]
            {
                this.ddlCustomerID.SelectedValue,
                this.ddlEmployeeID.SelectedValue,
                this.ddlShipVia.SelectedValue,
                this.txtShipCountry.Text
            };
        }

        /// <summary>検索条件の ＤＤＬ を作る</summary>
        private void BindSearchConditionDdl()
        {
            DataSet masters = this.Masters;
            if (masters == null) { return; }

            OrdListSearch.BindDdl(this.ddlCustomerID, masters.Tables["Customers"], "CustomerID", "CompanyName");
            OrdListSearch.BindDdl(this.ddlEmployeeID, masters.Tables["Employees"], "EmployeeID", "EmployeeName");
            OrdListSearch.BindDdl(this.ddlShipVia, masters.Tables["Shippers"], "ShipperID", "CompanyName");
        }

        /// <summary>検索条件の ＤＤＬ にマスタをバインドする（先頭は「（すべて）」）</summary>
        /// <param name="ddl">対象の ＤＤＬ</param>
        /// <param name="master">マスタ</param>
        /// <param name="valueField">値の列</param>
        /// <param name="textField">表示の列</param>
        private static void BindDdl(DropDownList ddl, DataTable master, string valueField, string textField)
        {
            if (ddl == null || master == null) { return; }

            ddl.Items.Clear();
            ddl.Items.Add(new System.Web.UI.WebControls.ListItem("（すべて）", ""));

            foreach (DataRow r in master.Rows)
            {
                ddl.Items.Add(new System.Web.UI.WebControls.ListItem(
                    Convert.ToString(r[textField]), Convert.ToString(r[valueField]).Trim()));
            }
            ddl.SelectedIndex = 0;
        }

        /// <summary>ＤＤＬ を値で選択する</summary>
        /// <param name="ddl">対象の ＤＤＬ</param>
        /// <param name="value">値</param>
        private static void SelectValue(DropDownList ddl, string value)
        {
            System.Web.UI.WebControls.ListItem hit = ddl.Items.FindByValue(value ?? "");
            if (hit != null) { ddl.SelectedValue = hit.Value; }
        }

        #endregion

        #region Ｂ層呼び出し

        /// <summary>Ｂ層を呼び出す</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="orderId">対象の OrderID（使わないなら null）</param>
        /// <param name="order">ＣＵＤの対象（使わないなら null）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private OrdReturnValue CallLayerB(string methodName, string orderId, DataTable order)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdParameterValue parameterValue = new OrdParameterValue(
                this.ContentPageFileNoEx, "-", methodName, "SQL", this.UserInfo);

            parameterValue.CustomerID = (this.ddlCustomerID == null) ? "" : this.ddlCustomerID.SelectedValue;
            parameterValue.EmployeeID = (this.ddlEmployeeID == null) ? "" : this.ddlEmployeeID.SelectedValue;
            parameterValue.ShipVia = (this.ddlShipVia == null) ? "" : this.ddlShipVia.SelectedValue;
            parameterValue.ShipCountry = (this.txtShipCountry == null) ? "" : this.txtShipCountry.Text;
            parameterValue.PageIndex = this.PageIndex;
            parameterValue.PageSize = OrdListSearch.PageSize;
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
