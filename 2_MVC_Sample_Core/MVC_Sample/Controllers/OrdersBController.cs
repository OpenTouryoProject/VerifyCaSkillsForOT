//**********************************************************************************
//* トランザクション・テーブル（Orders）保守：画面Ｂ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersBController
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
    /// Orders 画面Ｂ（条件検索・ページング・バッチ更新）
    /// </summary>
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class OrdersBController : MyBaseMVControllerCore
    {
        /// <summary>編集中の DataTable を置く Session のキー</summary>
        /// <remarks>
        /// ★ .NET Core の ISession は byte[] / string しか置けないので、DataTable をそのまま
        ///   置ける net48 と違い DTTables（Public.Dto）で JSON 化して往復させる。
        ///   素の System.Text.Json では RowState も変更前値も落ちるため使えない。
        /// </remarks>
        private const string SessionKey = "OrdersEditing";

        /// <summary>編集中の DataTable のテーブル名</summary>
        private const string TableName = "Orders";

        #region Session（編集中 DataTable）の出し入れ

        /// <summary>編集中の DataTable を Session から取得する</summary>
        private DataTable LoadEditingTable()
        {
            string json = this.HttpContext.Session.GetString(OrdersBController.SessionKey);
            if (string.IsNullOrEmpty(json)) { return null; }

            return DTTables.JsonToDTTables(json).ToDataSet().Tables[OrdersBController.TableName];
        }

        /// <summary>編集中の DataTable を Session に保持する（null で破棄）</summary>
        private void SaveEditingTable(DataTable dt)
        {
            if (dt == null)
            {
                this.HttpContext.Session.Remove(OrdersBController.SessionKey);
                return;
            }

            // ★ keepOriginal: true ＝ 変更前の値（DataRowVersion.Original）も JSON に載せる。
            //   既定（false）だと往復で変更前値が落ち、Ｂ層の楽観排他が成立しない。
            DTTables dtts = new DTTables();
            dtts.Add(DTTable.FromDataTable(dt, true));

            this.HttpContext.Session.SetString(
                OrdersBController.SessionKey, DTTables.DTTablesToJson(dtts));
        }

        #endregion

        /// <summary>
        /// 画面の初期表示（開き直したら編集内容は破棄し、DDL 用のマスタだけ読む）
        /// GET: /OrdersB/
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <returns>初期表示状態の画面（ViewResult）</returns>
        [HttpGet]
        public async Task<IActionResult> Index(OrdersViewModel model)
        {
            this.SaveEditingTable(null);

            OrdersReturnValue returnValue = await this.CallLayerB("OrdersMasters", null, model);
            if (returnValue != null) { OrdersBController.SetMasters(model, returnValue); }

            return View(model);
        }

        /// <summary>
        /// 条件検索（1ページ目から）
        /// POST: /OrdersB/Search
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(OrdersViewModel model)
        {
            model.PageIndex = 1;
            return await this.SearchPage(model, "検索しました");
        }

        /// <summary>
        /// ページ移動
        /// POST: /OrdersB/Page
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <param name="pageIndex">移動先のページ番号（1 起算）</param>
        /// <returns>再描画（ViewResult）</returns>
        /// <remarks>
        /// ★ 仕様：バッチ更新が開始されたら（＝編集中の行がある）ページングを止める。
        ///   ページを切り替えると再検索になり RowState が失われるため。
        /// </remarks>
        /// <remarks>
        /// ★ 引数名を pageIndex にしてはいけない。フォームの hidden も PageIndex を持つため、
        ///   モデルバインドが大文字小文字を区別せず衝突し、フォーム側（現在ページ）が優先されて
        ///   クエリ文字列で渡した移動先が無視される（＝ページが動かない）。名前を分ける。
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Page(OrdersViewModel model, int targetPage)
        {
            int pageIndex = targetPage;
            DataTable current = this.LoadEditingTable();

            if (OrdersBController.HasPendingChanges(current))
            {
                model.Orders = current;
                model.IsEditing = true;
                model.Message = "編集中はページングできません（［バッチ更新］で反映するか、画面を開き直して下さい）。";
                await this.EnsureMasters(model);
                return View("Index", model);
            }

            model.PageIndex = pageIndex;
            return await this.SearchPage(model, null);
        }

        /// <summary>条件検索を実行してページを表示する</summary>
        /// <param name="model">OrdersViewModel</param>
        /// <param name="message">表示するメッセージ（null なら既定文言）</param>
        /// <returns>再描画（ViewResult）</returns>
        private async Task<IActionResult> SearchPage(OrdersViewModel model, string message)
        {
            OrdersReturnValue returnValue = await this.CallLayerB("OrdersSearch", null, model);
            if (returnValue == null) { return View("Index", model); }

            this.SaveEditingTable(returnValue.Orders);

            model.Orders = returnValue.Orders;
            model.TotalCount = returnValue.TotalCount;
            model.IsEditing = false;
            OrdersBController.SetMasters(model, returnValue);

            model.Message = (message ?? "ページを移動しました")
                + "（" + model.TotalCount + " 件中 "
                + model.PageIndex + " / " + Math.Max(model.TotalPages, 1) + " ページ）。";

            return View("Index", model);
        }

        /// <summary>
        /// 行を追加する（RowState = Added にする）
        /// POST: /OrdersB/AddRow
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRow(OrdersViewModel model)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { return await this.NoListYet(model); }

            this.ReadRowsIntoTable(dt, model, -1);

            // Orders は OrderID（IDENTITY）以外すべて NULL 許容なので、空行のままでも INSERT できる。
            dt.Rows.Add(dt.NewRow());

            this.SaveEditingTable(dt);
            return await this.Rebind(model, dt, "行を追加しました（［バッチ更新］でDBに反映されます）。");
        }

        /// <summary>
        /// 行を確定する（RowState = Modified にする）
        /// POST: /OrdersB/UpdateRow
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <param name="rowIndex">DataTable の行インデックス</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRow(OrdersViewModel model, int rowIndex)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { return await this.NoListYet(model); }

            this.ReadRowsIntoTable(dt, model, rowIndex);

            this.SaveEditingTable(dt);
            return await this.Rebind(model, dt, "行を更新しました（［バッチ更新］でDBに反映されます）。");
        }

        /// <summary>
        /// 行を削除する（RowState = Deleted にする）
        /// POST: /OrdersB/DeleteRow
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <param name="rowIndex">DataTable の行インデックス</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRow(OrdersViewModel model, int rowIndex)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { return await this.NoListYet(model); }

            this.ReadRowsIntoTable(dt, model, -1);

            if (0 <= rowIndex && rowIndex < dt.Rows.Count)
            {
                // ★ Delete()。Rows.Remove() だと Deleted にならず DELETE が出ない。
                dt.Rows[rowIndex].Delete();
            }

            this.SaveEditingTable(dt);
            return await this.Rebind(model, dt, "行を削除しました（［バッチ更新］でDBに反映されます）。");
        }

        /// <summary>
        /// バッチ更新（CUD をＢ層＋自動生成Dao 経由で一括反映）
        /// POST: /OrdersB/BatchUpdate
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchUpdate(OrdersViewModel model)
        {
            DataTable dt = this.LoadEditingTable();
            if (dt == null) { return await this.NoListYet(model); }

            this.ReadRowsIntoTable(dt, model, -1);

            OrdersReturnValue returnValue = await this.CallLayerB("OrdersBatchUpdate", dt, model);

            if (returnValue == null)
            {
                // 業務例外＝ロールバック済み。RowState を残してやり直せるようにする。
                this.SaveEditingTable(dt);
                return await this.Rebind(model, dt, null);
            }

            dt.AcceptChanges();

            string message = "更新しました（挿入 " + returnValue.InsertCount
                + " 件／更新 " + returnValue.UpdateCount
                + " 件／削除 " + returnValue.DeleteCount + " 件）。";

            // ★ IDENTITY の採番値は DataTable に戻らないので、同じ条件・同じページで取り直す。
            return await this.SearchPage(model, message);
        }

        #region 補助

        /// <summary>一覧未取得のときの応答</summary>
        private async Task<IActionResult> NoListYet(OrdersViewModel model)
        {
            model.Message = "先に［検索］を実行して下さい。";
            await this.EnsureMasters(model);
            return View("Index", model);
        }

        /// <summary>編集中の DataTable を再表示する</summary>
        private async Task<IActionResult> Rebind(OrdersViewModel model, DataTable dt, string message)
        {
            model.Orders = dt;
            model.IsEditing = OrdersBController.HasPendingChanges(dt);
            if (message != null) { model.Message = message; }
            await this.EnsureMasters(model);
            return View("Index", model);
        }

        /// <summary>DDL 用のマスタが未設定なら取得する</summary>
        private async Task EnsureMasters(OrdersViewModel model)
        {
            if (model.Customers != null) { return; }

            OrdersReturnValue rv = await this.CallLayerB("OrdersMasters", null, model);
            if (rv != null) { OrdersBController.SetMasters(model, rv); }
        }

        /// <summary>マスタを ViewModel に移す</summary>
        private static void SetMasters(OrdersViewModel model, OrdersReturnValue returnValue)
        {
            model.Customers = returnValue.Customers;
            model.Employees = returnValue.Employees;
            model.Shippers = returnValue.Shippers;
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

        /// <summary>Ｂ層を呼び出す</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="orders">バッチ更新対象（参照系は null）</param>
        /// <param name="model">OrdersViewModel（検索条件・ページを渡す）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private async Task<OrdersReturnValue> CallLayerB(string methodName, DataTable orders, OrdersViewModel model)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdersParameterValue parameterValue = new OrdersParameterValue(
                this.ControllerName, "-", methodName, "SQL", this.UserInfo);

            parameterValue.CustomerID = model.CustomerID;
            parameterValue.EmployeeID = model.EmployeeID;
            parameterValue.ShipVia = model.ShipVia;
            parameterValue.ShipCountry = model.ShipCountry;
            parameterValue.PageIndex = model.PageIndex;
            parameterValue.PageSize = (model.PageSize <= 0) ? 20 : model.PageSize;
            parameterValue.Orders = orders;

            OrdersReturnValue returnValue = (OrdersReturnValue)await (new LayerB())
                .DoBusinessLogicAsync(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行---------------------------------------------------------

            if (returnValue.ErrorFlag)
            {
                model.Message = returnValue.ErrorMessage;
                return null;
            }
            return returnValue;
        }

        #endregion

        #region 画面 → DataTable への読み戻し

        /// <summary>画面のセル値を DataTable へ読み戻す</summary>
        /// <param name="dt">編集中の DataTable</param>
        /// <param name="model">OrdersViewModel</param>
        /// <param name="targetRowIndex">確定する既存行の行インデックス（-1＝追加行のみ）</param>
        private void ReadRowsIntoTable(DataTable dt, OrdersViewModel model, int targetRowIndex)
        {
            if (model.Rows == null) { return; }

            foreach (OrderRowViewModel row in model.Rows)
            {
                if (row.RowIndex < 0 || dt.Rows.Count <= row.RowIndex) { continue; }

                DataRow dr = dt.Rows[row.RowIndex];
                if (dr.RowState == DataRowState.Deleted) { continue; }
                if (dr.RowState != DataRowState.Added && row.RowIndex != targetRowIndex) { continue; }

                OrdersBController.SetIfChanged(dr, "CustomerID", row.CustomerID);
                OrdersBController.SetIfChanged(dr, "EmployeeID", row.EmployeeID);
                OrdersBController.SetIfChanged(dr, "OrderDate", row.OrderDate);
                OrdersBController.SetIfChanged(dr, "RequiredDate", row.RequiredDate);
                OrdersBController.SetIfChanged(dr, "ShippedDate", row.ShippedDate);
                OrdersBController.SetIfChanged(dr, "ShipVia", row.ShipVia);
                OrdersBController.SetIfChanged(dr, "Freight", row.Freight);
                OrdersBController.SetIfChanged(dr, "ShipName", row.ShipName);
                OrdersBController.SetIfChanged(dr, "ShipAddress", row.ShipAddress);
                OrdersBController.SetIfChanged(dr, "ShipCity", row.ShipCity);
                OrdersBController.SetIfChanged(dr, "ShipRegion", row.ShipRegion);
                OrdersBController.SetIfChanged(dr, "ShipPostalCode", row.ShipPostalCode);
                OrdersBController.SetIfChanged(dr, "ShipCountry", row.ShipCountry);
            }
        }

        /// <summary>値が変わっているときだけ、列の型に変換して代入する</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <param name="newValue">画面の値（文字列）</param>
        /// <remarks>
        /// ★ Orders の DataTable は型付き（int / DateTime / decimal）。
        ///   文字列をそのまま代入すると ArgumentException になるので列の型へ変換する。
        ///   変換できない入力は「変更しない」で無視する（画面側の入力検証は別途）。
        /// ★ Orders は OrderID 以外すべて NULL 許容なので、空欄は DBNull にする。
        /// </remarks>
        private static void SetIfChanged(DataRow dr, string columnName, string newValue)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return; }

            string current = (dr[columnName] == DBNull.Value) ? "" : OrdersBController.ToText(dr[columnName]);
            string edited = (newValue ?? "").Trim();

            if (current == edited) { return; }

            if (edited.Length == 0)
            {
                dr[columnName] = DBNull.Value;
                return;
            }

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

        /// <summary>DataRow の値を画面表示用の文字列にする</summary>
        /// <param name="value">値</param>
        /// <returns>文字列</returns>
        private static string ToText(object value)
        {
            if (value == null || value == DBNull.Value) { return ""; }
            if (value is DateTime) { return ((DateTime)value).ToString("yyyy/MM/dd"); }
            return Convert.ToString(value);
        }

        #endregion
    }
}
