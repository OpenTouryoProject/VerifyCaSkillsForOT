//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：画面Ｂ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersBController
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

using MVC_Sample.Logic.Business;
using MVC_Sample.Logic.Common;
using MVC_Sample.Models.ViewModels;

using System;
using System.Data;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.Public.Db;
using Touryo.Infrastructure.Public.Dto;

namespace MVC_Sample.Controllers
{
    /// <summary>
    /// Suppliers 画面Ｂ（一覧表示とバッチ更新）
    /// </summary>
    /// <remarks>
    /// グリッド中で行の追加・更新・削除を行い、DataTable の RowState に覚えさせて、
    /// ［バッチ更新］で Ｂ層 → 自動生成Dao 経由で一括反映する。
    /// </remarks>
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class SuppliersBController : MyBaseMVControllerCore
    {
        /// <summary>編集中の DataTable を置く Session のキー</summary>
        private const string SessionKey = "SuppliersEditing";

        /// <summary>編集中の DataTable のテーブル名</summary>
        private const string TableName = "Suppliers";

        #region Session（編集中 DataTable）の出し入れ

        /// <summary>編集中の DataTable を Session から取得する</summary>
        /// <returns>編集中の DataTable（無ければ null）</returns>
        /// <remarks>
        /// ★ .NET Core の ISession は byte[] / string しか置けないので、DataTable を
        ///   そのまま置ける net48 と違い DTTables（Public.Dto）で JSON 化して往復させる。
        ///   素の System.Text.Json では RowState も変更前値も落ちるため使えない。
        /// </remarks>
        private DataTable LoadEditingTable()
        {
            string json = this.HttpContext.Session.GetString(SuppliersBController.SessionKey);
            if (string.IsNullOrEmpty(json)) { return null; }

            return DTTables.JsonToDTTables(json).ToDataSet().Tables[SuppliersBController.TableName];
        }

        /// <summary>編集中の DataTable を Session に保持する（null で破棄）</summary>
        /// <param name="dt">編集中の DataTable</param>
        private void SaveEditingTable(DataTable dt)
        {
            if (dt == null)
            {
                this.HttpContext.Session.Remove(SuppliersBController.SessionKey);
                return;
            }

            // ★ keepOriginal: true ＝ 変更前の値（DataRowVersion.Original）も JSON に載せる。
            //   既定（false）だと往復で変更前値が落ち、Ｂ層の楽観排他（WHERE に取得時の値）が成立しない。
            //   DTTables.FromDataSet には keepOriginal が無いので、表ごとに DTTable を組んで足す。
            DTTables dtts = new DTTables();
            dtts.Add(DTTable.FromDataTable(dt, true));

            this.HttpContext.Session.SetString(
                SuppliersBController.SessionKey, DTTables.DTTablesToJson(dtts));
        }

        #endregion

        /// <summary>
        /// 画面の初期表示（開き直したら編集内容は破棄する）
        /// GET: /SuppliersB/
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <returns>初期表示状態の画面（ViewResult）</returns>
        [HttpGet]
        public IActionResult Index(SuppliersViewModel model)
        {
            this.SaveEditingTable(null);
            return View(model);
        }

        /// <summary>
        /// Suppliers の一覧を取得する（自動生成Dao の参照処理をＢ層経由で実行）
        /// POST: /SuppliersB/SelectAll
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectAll(SuppliersViewModel model)
        {
            // ↓Ｂ層実行：Suppliers の一覧取得----------------------------------
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.ControllerName, "-", "SuppliersSelectAll", "SQL", this.UserInfo);

            SuppliersReturnValue returnValue = (SuppliersReturnValue)await (new LayerB())
                .DoBusinessLogicAsync(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行：Suppliers の一覧取得----------------------------------

            if (returnValue.ErrorFlag)
            {
                model.Message = returnValue.ErrorMessage;
            }
            else
            {
                this.SaveEditingTable(returnValue.Suppliers);
                model.Suppliers = returnValue.Suppliers;
                model.Message = "一覧を取得しました（" + returnValue.Suppliers.Rows.Count + " 件）。";
            }

            return View("Index", model);
        }

        /// <summary>
        /// 行を追加する（RowState = Added にする）
        /// POST: /SuppliersB/AddRow
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddRow(SuppliersViewModel model)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { model.Message = "先に［一覧取得］を実行して下さい。"; return View("Index", model); }

            // -1＝追加行だけ読み戻す（既存行は各行の［更新］で確定する）
            this.ReadRowsIntoTable(dt, model, -1);

            DataRow nr = dt.NewRow();

            // ★ DB 側 NOT NULL の列は空文字で初期化する。
            //   DBNull のまま INSERT すると SqlException 515 になる。
            nr["CompanyName"] = "";
            dt.Rows.Add(nr);

            this.SaveEditingTable(dt);
            model.Suppliers = dt;
            model.Message = "行を追加しました（［バッチ更新］でDBに反映されます）。";
            return View("Index", model);
        }

        /// <summary>
        /// 行を確定する（RowState = Modified にする）
        /// POST: /SuppliersB/UpdateRow
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <param name="rowIndex">DataTable の行インデックス</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateRow(SuppliersViewModel model, int rowIndex)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { model.Message = "先に［一覧取得］を実行して下さい。"; return View("Index", model); }

            // 当該の既存行＋追加行を読み戻す
            this.ReadRowsIntoTable(dt, model, rowIndex);

            this.SaveEditingTable(dt);
            model.Suppliers = dt;
            model.Message = "行を更新しました（［バッチ更新］でDBに反映されます）。";
            return View("Index", model);
        }

        /// <summary>
        /// 行を削除する（RowState = Deleted にする）
        /// POST: /SuppliersB/DeleteRow
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <param name="rowIndex">DataTable の行インデックス</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteRow(SuppliersViewModel model, int rowIndex)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { model.Message = "先に［一覧取得］を実行して下さい。"; return View("Index", model); }

            this.ReadRowsIntoTable(dt, model, -1);

            if (0 <= rowIndex && rowIndex < dt.Rows.Count)
            {
                // ★ Delete()。Rows.Remove() だと Deleted にならず DELETE が出ない。
                dt.Rows[rowIndex].Delete();
            }

            this.SaveEditingTable(dt);
            model.Suppliers = dt;
            model.Message = "行を削除しました（［バッチ更新］でDBに反映されます）。";
            return View("Index", model);
        }

        /// <summary>
        /// バッチ更新（CUD をＢ層＋自動生成Dao 経由で一括反映）
        /// POST: /SuppliersB/BatchUpdate
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchUpdate(SuppliersViewModel model)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { model.Message = "先に［一覧取得］を実行して下さい。"; return View("Index", model); }

            this.ReadRowsIntoTable(dt, model, -1);

            // ↓Ｂ層実行：Suppliers のバッチ更新--------------------------------
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.ControllerName, "-", "SuppliersBatchUpdate", "SQL", this.UserInfo);
            parameterValue.Suppliers = dt;

            SuppliersReturnValue returnValue = (SuppliersReturnValue)await (new LayerB())
                .DoBusinessLogicAsync(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行：Suppliers のバッチ更新--------------------------------

            if (returnValue.ErrorFlag)
            {
                // 業務例外＝ロールバック済み。RowState を残してやり直せるようにする。
                this.SaveEditingTable(dt);
                model.Suppliers = dt;
                model.Message = returnValue.ErrorMessage;
                return View("Index", model);
            }

            // 反映できたので確定（RowState を Unchanged に戻す）
            dt.AcceptChanges();

            // ★ IDENTITY の採番値は DataTable に戻らないので、一覧を取り直す。
            SuppliersParameterValue reloadPv = new SuppliersParameterValue(
                this.ControllerName, "-", "SuppliersSelectAll", "SQL", this.UserInfo);
            SuppliersReturnValue reloadRv = (SuppliersReturnValue)await (new LayerB())
                .DoBusinessLogicAsync(reloadPv, DbEnum.IsolationLevelEnum.ReadCommitted);

            this.SaveEditingTable(reloadRv.Suppliers);
            model.Suppliers = reloadRv.Suppliers;
            model.Message = "更新しました（挿入 " + returnValue.InsertCount
                + " 件／更新 " + returnValue.UpdateCount
                + " 件／削除 " + returnValue.DeleteCount + " 件）。";
            return View("Index", model);
        }

        #region 画面 → DataTable への読み戻し

        /// <summary>画面のセル値を DataTable へ読み戻す</summary>
        /// <param name="dt">編集中の DataTable</param>
        /// <param name="model">SuppliersViewModel</param>
        /// <param name="targetRowIndex">確定する既存行の行インデックス（-1＝追加行のみ）</param>
        /// <remarks>
        /// ★ 追加行は常に読み戻す（DB に戻す値が無く、落とすと再描画で空行に戻るため）。
        /// ★ 既存行はその行の［更新］が押されたときだけ（無駄な Modified＝無駄な UPDATE を防ぐ）。
        /// ★ 削除行は対象外（Deleted 行は現在値を読めない）。
        /// </remarks>
        private void ReadRowsIntoTable(DataTable dt, SuppliersViewModel model, int targetRowIndex)
        {
            if (model.Rows == null) { return; }

            foreach (SupplierRowViewModel row in model.Rows)
            {
                if (row.RowIndex < 0 || dt.Rows.Count <= row.RowIndex) { continue; }

                DataRow dr = dt.Rows[row.RowIndex];
                if (dr.RowState == DataRowState.Deleted) { continue; }
                if (dr.RowState != DataRowState.Added && row.RowIndex != targetRowIndex) { continue; }

                // CompanyName は DB 側 NOT NULL＝空欄でも DBNull にしない
                SuppliersBController.SetIfChanged(dr, "CompanyName", row.CompanyName, true);
                SuppliersBController.SetIfChanged(dr, "ContactName", row.ContactName, false);
                SuppliersBController.SetIfChanged(dr, "ContactTitle", row.ContactTitle, false);
                SuppliersBController.SetIfChanged(dr, "Address", row.Address, false);
                SuppliersBController.SetIfChanged(dr, "City", row.City, false);
                SuppliersBController.SetIfChanged(dr, "Region", row.Region, false);
                SuppliersBController.SetIfChanged(dr, "PostalCode", row.PostalCode, false);
                SuppliersBController.SetIfChanged(dr, "Country", row.Country, false);
                SuppliersBController.SetIfChanged(dr, "Phone", row.Phone, false);
                SuppliersBController.SetIfChanged(dr, "Fax", row.Fax, false);
                SuppliersBController.SetIfChanged(dr, "HomePage", row.HomePage, false);
            }
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
