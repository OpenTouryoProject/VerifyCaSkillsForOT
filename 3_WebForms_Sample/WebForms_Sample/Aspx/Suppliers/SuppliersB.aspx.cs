//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：画面Ｂ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersB
//* クラス日本語名  ：Suppliers 画面Ｂ（一覧＆更新：RowState バッチ）
//*
//* 作成日時        ：2026/08/27
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/27  生技              新規作成
//**********************************************************************************

using System;
using System.Data;
using System.Web.UI.WebControls;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.CustomControl;
using Touryo.Infrastructure.Framework.Presentation;
using Touryo.Infrastructure.Framework.Util;
using Touryo.Infrastructure.Public.Db;

namespace WebForms_Sample.Aspx.Suppliers
{
    /// <summary>Suppliers 画面Ｂ（一覧表示とバッチ更新）</summary>
    /// <remarks>
    /// グリッド中で行の追加・更新・削除を行い、DataTable の RowState に覚えさせて、
    /// ［バッチ更新］で Ｂ層 → 自動生成Dao 経由で一括反映する。
    /// </remarks>
    public partial class SuppliersB : MyBaseController
    {
        /// <summary>編集中の DataTable を置く Session のキー</summary>
        /// <remarks>
        /// 一覧取得〜編集〜更新が複数ポストバックに跨るので保持する。
        /// ★ YES/NO 確認ダイアログの後処理は「次のポストバック」で走るので、
        ///   ダイアログを出す時点で編集内容が確定して持ち回られている必要がある。
        /// ★ net48 は DataTable をそのまま Session に置ける（binary 直列化）。
        /// </remarks>
        private const string SessionKey = "SuppliersEditing";

        #region Session（編集中 DataTable）の出し入れ

        /// <summary>編集中の DataTable を Session から取得する</summary>
        private DataTable LoadEditingTable()
        {
            return this.Session[SuppliersB.SessionKey] as DataTable;
        }

        /// <summary>編集中の DataTable を Session に保持する（null で破棄）</summary>
        private void SaveEditingTable(DataTable dt)
        {
            if (dt == null) { this.Session.Remove(SuppliersB.SessionKey); }
            else { this.Session[SuppliersB.SessionKey] = dt; }
        }

        #endregion

        #region ページ ロードの共通処理（UOC メソッド）

        /// <summary>初期表示時の処理</summary>
        protected override void UOC_FormInit()
        {
            // 開き直したら編集内容は破棄する
            this.SaveEditingTable(null);
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

            btn1.Text = "一覧取得";   btn1.Enabled = true;
            btn2.Text = "バッチ更新"; btn2.Enabled = true;
            btn3.Text = "戻る";       btn3.Enabled = true;

            // 使わないボタンは disable にする（共通仕様）
            btn4.Text = "－"; btn4.Enabled = false;
            btn5.Text = "－"; btn5.Enabled = false;
        }

        #endregion

        #region マスタ ページ上のボタンのイベント

        /// <summary>btnMButton1 のクリック イベント（一覧取得）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_testBlankScreen_btnMButton1_Click(FxEventArgs fxEventArgs)
        {
            // ↓Ｂ層実行：Suppliers の一覧取得----------------------------------
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.ContentPageFileNoEx, fxEventArgs.ButtonID, "SuppliersSelectAll", "SQL", this.UserInfo);

            SuppliersReturnValue returnValue = (SuppliersReturnValue)new LayerB()
                .DoBusinessLogic(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行：Suppliers の一覧取得----------------------------------

            if (returnValue.ErrorFlag)
            {
                this.lblMessage.Text = returnValue.ErrorMessage;
                return string.Empty;
            }

            this.SaveEditingTable(returnValue.Suppliers);
            this.BindGrid(returnValue.Suppliers);
            this.lblMessage.Text = "一覧を取得しました（" + returnValue.Suppliers.Rows.Count + " 件）。";

            return string.Empty;
        }

        /// <summary>btnMButton2 のクリック イベント（バッチ更新＝確認ダイアログを出す）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        /// <remarks>
        /// ★ ここでは確認ダイアログを出すだけ。実際の更新は YES を押した「次のポストバック」で
        ///   UOC_YesNoDialog_Yes_Click が呼ばれてから行う。
        ///   そのため、編集内容はこの時点で読み戻して Session に確定させておく。
        /// </remarks>
        protected string UOC_testBlankScreen_btnMButton2_Click(FxEventArgs fxEventArgs)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { this.lblMessage.Text = "先に［一覧取得］を実行して下さい。"; return string.Empty; }

            // 追加行を読み戻して確定しておく（-1＝追加行のみ）
            this.ReadGridIntoTable(dt, -1);
            this.SaveEditingTable(dt);

            // 共通仕様：確認ダイアログはフレームワークの ShowYesNoMessageDialog を使う
            this.ShowYesNoMessageDialog(
                "SuppliersBatchUpdate", "バッチ更新します。よろしいですか？", "確認");

            return string.Empty;
        }

        /// <summary>btnMButton3 のクリック イベント（画面Ａへ戻る）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL</returns>
        protected string UOC_testBlankScreen_btnMButton3_Click(FxEventArgs fxEventArgs)
        {
            return "~/Aspx/Suppliers/SuppliersA.aspx";
        }

        #endregion

        #region 確認ダイアログの後処理

        /// <summary>YES/NO 確認ダイアログで YES が押されたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        /// <remarks>
        /// ★ 戻り値は void（UOC_（コントロール名）_（イベント名）の string とは違う）。
        /// ★ 別ポストバックで走るので、編集内容は Session から取り直す。
        /// </remarks>
        protected override void UOC_YesNoDialog_Yes_Click(FxEventArgs parentFxEventArgs)
        {
            // 1画面に確認ダイアログが複数ある場合に備え、どのボタンから開いたかで振り分ける
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
            this.BindGrid(this.LoadEditingTable());
        }

        /// <summary>YES/NO 確認ダイアログが×で閉じられたときの処理</summary>
        /// <param name="parentFxEventArgs">ダイアログを開いたボタンのイベント引数</param>
        protected override void UOC_YesNoDialog_X_Click(FxEventArgs parentFxEventArgs)
        {
            this.BindGrid(this.LoadEditingTable());
        }

        /// <summary>バッチ更新（CUD をＢ層＋自動生成Dao 経由で一括反映）</summary>
        private void BatchUpdate()
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { this.lblMessage.Text = "先に［一覧取得］を実行して下さい。"; return; }

            // ↓Ｂ層実行：Suppliers のバッチ更新--------------------------------
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.ContentPageFileNoEx, "btnMButton2", "SuppliersBatchUpdate", "SQL", this.UserInfo);
            parameterValue.Suppliers = dt;

            SuppliersReturnValue returnValue = (SuppliersReturnValue)new LayerB()
                .DoBusinessLogic(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行：Suppliers のバッチ更新--------------------------------

            if (returnValue.ErrorFlag)
            {
                // 業務例外＝ロールバック済み。RowState を残してやり直せるようにする。
                this.lblMessage.Text = returnValue.ErrorMessage;
                this.BindGrid(dt);
                return;
            }

            // 反映できたので確定（RowState を Unchanged に戻す）
            dt.AcceptChanges();

            // ★ IDENTITY の採番値は DataTable に戻らないので、一覧を取り直す。
            SuppliersParameterValue reloadPv = new SuppliersParameterValue(
                this.ContentPageFileNoEx, "btnMButton2", "SuppliersSelectAll", "SQL", this.UserInfo);
            SuppliersReturnValue reloadRv = (SuppliersReturnValue)new LayerB()
                .DoBusinessLogic(reloadPv, DbEnum.IsolationLevelEnum.ReadCommitted);

            this.SaveEditingTable(reloadRv.Suppliers);
            this.BindGrid(reloadRv.Suppliers);

            this.lblMessage.Text = "更新しました（挿入 " + returnValue.InsertCount
                + " 件／更新 " + returnValue.UpdateCount
                + " 件／削除 " + returnValue.DeleteCount + " 件）。";
        }

        #endregion

        #region コンテンツ ページ上のボタンのイベント

        /// <summary>btnAddRow のクリック イベント（行追加＝空行を足す）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        protected string UOC_btnAddRow_Click(FxEventArgs fxEventArgs)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { this.lblMessage.Text = "先に［一覧取得］を実行して下さい。"; return string.Empty; }

            // -1＝追加行だけ読み戻す（既存行は各行の［更新］で確定する）
            this.ReadGridIntoTable(dt, -1);

            DataRow nr = dt.NewRow();

            // ★ DB 側 NOT NULL の列は空文字で初期化する。
            //   DBNull のまま INSERT すると SqlException 515 になる。
            nr["CompanyName"] = "";
            dt.Rows.Add(nr);

            this.SaveEditingTable(dt);
            this.BindGrid(dt);
            this.lblMessage.Text = "行を追加しました（［バッチ更新］でDBに反映されます）。";

            return string.Empty;
        }

        #endregion

        #region GridView のイベント

        /// <summary>gvwSuppliers の RowCommand イベント（行ごとの［更新］［削除］）</summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        /// <remarks>
        /// 行ボタンは RowState を作るだけ。実際の CUD はグリッド外の［バッチ更新］で一括して行う。
        /// </remarks>
        protected string UOC_gvwSuppliers_RowCommand(FxEventArgs fxEventArgs)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { this.lblMessage.Text = "先に［一覧取得］を実行して下さい。"; return string.Empty; }

            // 一覧表示系コントロールでは PostBackValue がアイテムの index
            int displayIndex = int.Parse(fxEventArgs.PostBackValue);

            switch (fxEventArgs.InnerButtonID)
            {
                case "Update":

                    // 当該の既存行＋追加行を読み戻す（＝Modified になる）
                    this.ReadGridIntoTable(dt, displayIndex);
                    this.lblMessage.Text = "行を更新しました（［バッチ更新］でDBに反映されます）。";
                    break;

                case "Delete":

                    this.ReadGridIntoTable(dt, -1);

                    DataRow target = SuppliersB.GetDataRowForDisplayIndex(dt, displayIndex);
                    if (target != null)
                    {
                        // ★ Delete()。Rows.Remove() だと Deleted にならず DELETE が出ない。
                        target.Delete();
                    }
                    this.lblMessage.Text = "行を削除しました（［バッチ更新］でDBに反映されます）。";
                    break;

                default:
                    break;
            }

            this.SaveEditingTable(dt);
            this.BindGrid(dt);

            return string.Empty;
        }

        #endregion

        #region グリッド ⇔ DataTable

        /// <summary>GridView に DataTable をバインドする</summary>
        /// <param name="dt">編集中の DataTable</param>
        private void BindGrid(DataTable dt)
        {
            this.gvwSuppliers.DataSource = dt;
            this.gvwSuppliers.DataBind();
        }

        /// <summary>グリッドのセル値を DataTable へ読み戻す</summary>
        /// <param name="dt">編集中の DataTable</param>
        /// <param name="targetDisplayIndex">確定する既存行の表示 index（-1＝追加行のみ）</param>
        /// <remarks>
        /// ★ Web はポストバックでセルが自動的に DataTable に入らないので読み戻しが要る。
        /// ★ 追加行は常に読み戻す（DB に戻す値が無く、落とすと再バインドで空行に戻るため）。
        /// ★ 既存行はその行の［更新］が押されたときだけ（無駄な Modified＝無駄な UPDATE を防ぐ）。
        /// </remarks>
        private void ReadGridIntoTable(DataTable dt, int targetDisplayIndex)
        {
            foreach (GridViewRow gvr in this.gvwSuppliers.Rows)
            {
                if (gvr.RowType != DataControlRowType.DataRow) { continue; }

                DataRow dr = SuppliersB.GetDataRowForDisplayIndex(dt, gvr.RowIndex);
                if (dr == null) { continue; }
                if (dr.RowState != DataRowState.Added && gvr.RowIndex != targetDisplayIndex) { continue; }

                // CompanyName は DB 側 NOT NULL＝空欄でも DBNull にしない
                SuppliersB.SetIfChanged(dr, "CompanyName", SuppliersB.GetCellText(gvr, "CompanyName"), true);
                SuppliersB.SetIfChanged(dr, "ContactName", SuppliersB.GetCellText(gvr, "ContactName"), false);
                SuppliersB.SetIfChanged(dr, "ContactTitle", SuppliersB.GetCellText(gvr, "ContactTitle"), false);
                SuppliersB.SetIfChanged(dr, "Address", SuppliersB.GetCellText(gvr, "Address"), false);
                SuppliersB.SetIfChanged(dr, "City", SuppliersB.GetCellText(gvr, "City"), false);
                SuppliersB.SetIfChanged(dr, "Region", SuppliersB.GetCellText(gvr, "Region"), false);
                SuppliersB.SetIfChanged(dr, "PostalCode", SuppliersB.GetCellText(gvr, "PostalCode"), false);
                SuppliersB.SetIfChanged(dr, "Country", SuppliersB.GetCellText(gvr, "Country"), false);
                SuppliersB.SetIfChanged(dr, "Phone", SuppliersB.GetCellText(gvr, "Phone"), false);
                SuppliersB.SetIfChanged(dr, "Fax", SuppliersB.GetCellText(gvr, "Fax"), false);
                SuppliersB.SetIfChanged(dr, "HomePage", SuppliersB.GetCellText(gvr, "HomePage"), false);
            }
        }

        /// <summary>行内の TextBox の値を取得する</summary>
        /// <param name="gvr">GridView の行</param>
        /// <param name="controlId">コントロール ID</param>
        /// <returns>入力値</returns>
        private static string GetCellText(GridViewRow gvr, string controlId)
        {
            TextBox tb = (TextBox)gvr.FindControl(controlId);
            return (tb == null) ? "" : tb.Text;
        }

        /// <summary>表示 index に対応する DataRow を返す</summary>
        /// <param name="dt">編集中の DataTable</param>
        /// <param name="displayIndex">グリッド上の表示 index</param>
        /// <returns>対応する DataRow（無ければ null）</returns>
        /// <remarks>
        /// ★ Deleted 行は DefaultView から外れてグリッドに表示されないため、
        ///   グリッドの RowIndex と dt.Rows[i] はずれる。Deleted を飛ばしながら数える。
        ///   （素朴に dt.Rows[gvr.RowIndex] としてはいけない）
        /// </remarks>
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

        /// <summary>値が変わっているときだけ代入する（無駄な Modified を作らない）</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <param name="newValue">画面の値</param>
        /// <param name="notNull">DB 側が NOT NULL か</param>
        private static void SetIfChanged(DataRow dr, string columnName, string newValue, bool notNull)
        {
            string current = (dr[columnName] == DBNull.Value) ? "" : Convert.ToString(dr[columnName]);
            string edited = newValue ?? "";

            if (current == edited) { return; }

            dr[columnName] = (edited.Length == 0 && !notNull) ? (object)DBNull.Value : (object)edited;
        }

        #endregion
    }
}
