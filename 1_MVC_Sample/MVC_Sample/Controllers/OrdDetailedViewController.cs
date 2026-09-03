//**********************************************************************************
//* 受注管理（Ord）：画面Ｂ＝詳細・更新（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdDetailedViewController
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

using MVC_Sample.Logic.Business;
using MVC_Sample.Logic.Common;
using MVC_Sample.Models.ViewModels;

using System;
using System.Globalization;
using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace MVC_Sample.Controllers
{
    /// <summary>受注 詳細・更新（画面Ｂ）</summary>
    /// <remarks>
    /// 仕様：
    /// ・初期処理でマスタ・テーブルを取得し「マスタ・テーブル値入力用ＤＤＬ」を生成する。
    /// ・画面Ａの詳細ボタンから来たときは自動生成Dao の参照（Ｒ）で詳細表示し、
    ///   ＵＤ（更新・削除）を活性にする。追加ボタンから来たときはＣ（追加）を活性にする。
    /// ・ＣＵＤボタンで YES/NO 確認ダイアログ（JavaScript の window.confirm）を出し、
    ///   YES 押下後に処理を実行する。
    /// ★ 編集中の1行は DataTable のまま Session に持つ（net48 は binary 直列化でそのまま置ける）。
    ///   取得時の値が DataRowVersion.Original に残るので、Ｂ層で楽観排他が成立する。
    /// </remarks>
    [Authorize]
    public class OrdDetailedViewController : MyBaseMVController
    {
        /// <summary>編集中の1行（DataTable）を置く Session のキー</summary>
        private const string SessionKey = "OrdEditingOrder";

        /// <summary>編集中の明細（DataTable）を置く Session のキー</summary>
        private const string SessionKeyDetails = "OrdEditingDetails";

        #region Session（編集中 DataTable）の出し入れ

        /// <summary>編集中の1行を Session から取得する</summary>
        private DataTable LoadEditingRow()
        {
            return this.Session[OrdDetailedViewController.SessionKey] as DataTable;
        }

        /// <summary>編集中の1行を Session に保持する（null で破棄）</summary>
        private void SaveEditingRow(DataTable dt)
        {
            if (dt == null) { this.Session.Remove(OrdDetailedViewController.SessionKey); }
            else { this.Session[OrdDetailedViewController.SessionKey] = dt; }
        }

        /// <summary>編集中の明細を Session から取得する</summary>
        private DataTable LoadEditingDetails()
        {
            return this.Session[OrdDetailedViewController.SessionKeyDetails] as DataTable;
        }

        /// <summary>編集中の明細を Session に保持する（null で破棄）</summary>
        private void SaveEditingDetails(DataTable dt)
        {
            if (dt == null) { this.Session.Remove(OrdDetailedViewController.SessionKeyDetails); }
            else { this.Session[OrdDetailedViewController.SessionKeyDetails] = dt; }
        }

        #endregion

        /// <summary>
        /// 画面の初期表示（マスタ→ＤＤＬ／自動生成Dao の参照＝Ｒ）
        /// GET: /OrdDetailedView/?orderId=...
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <returns>初期表示状態の画面（ViewResult）</returns>
        [HttpGet]
        public async Task<ActionResult> Index(OrdViewModel model)
        {
            // --- ① マスタ・テーブルの取得 → 入力用ＤＤＬ 生成（仕様） ---
            await this.LoadMasters(model);

            // --- ② 詳細（自動生成Dao の参照＝Ｒ）。追加モードは 0 件＝スキーマだけ戻る ---
            OrdReturnValue detail = await this.CallLayerB("OrdDetailedView", model, null, null);
            if (detail == null) { return View(model); }

            DataTable dt = detail.Order;

            if (dt.Rows.Count == 0)
            {
                // 追加モード：空行を1行足す（RowState＝Added）
                model.IsNew = true;
                dt.Rows.Add(dt.NewRow());
            }
            else
            {
                model.IsNew = false;
            }

            this.SaveEditingRow(dt);
            OrdDetailedViewController.RowToModel(dt.Rows[0], model);

            // --- ③ 明細（Order Details）の参照（Ｒ）結果を保持する ---
            DataTable details = detail.OrderDetails;
            this.SaveEditingDetails(details);
            model.OrderDetails = details;

            return View(model);
        }

        /// <summary>
        /// 明細の行を追加する（RowState = Added にする）
        /// POST: /OrdDetailedView/AddDetailRow
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDetailRow(OrdViewModel model)
        {
            await this.LoadMasters(model);

            DataTable details = this.LoadEditingDetails();
            if (details == null) { return this.NoTarget(model); }

            this.ReadDetailRows(details, model, -1);

            // ★ Order Details は全列 NOT NULL、かつ CHECK 制約
            //   （Quantity > 0／Discount 0〜1／UnitPrice >= 0）がある。
            //   空行のままバッチ更新すると SqlException（515／547）になるので既定値を入れる。
            DataRow nr = details.NewRow();
            nr["OrderID"] = string.IsNullOrEmpty(model.OrderID) ? 0 : int.Parse(model.OrderID);
            nr["ProductID"] = OrdDetailedViewController.FirstProductId(model.Products);
            nr["UnitPrice"] = 0m;
            nr["Quantity"] = (short)1;
            nr["Discount"] = 0f;
            details.Rows.Add(nr);

            this.SaveEditingDetails(details);
            model.OrderDetails = details;
            model.Message = "明細行を追加しました（［追加］／［更新］でDBに反映されます）。";

            return View("Index", model);
        }

        /// <summary>
        /// 明細の行を確定する（RowState = Modified にする）
        /// POST: /OrdDetailedView/UpdateDetailRow
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <param name="rowIndex">DataTable の行インデックス</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateDetailRow(OrdViewModel model, int rowIndex)
        {
            await this.LoadMasters(model);

            DataTable details = this.LoadEditingDetails();
            if (details == null) { return this.NoTarget(model); }

            this.ReadDetailRows(details, model, rowIndex);

            this.SaveEditingDetails(details);
            model.OrderDetails = details;
            model.Message = "明細行を更新しました（［追加］／［更新］でDBに反映されます）。";

            return View("Index", model);
        }

        /// <summary>
        /// 明細の行を削除する（RowState = Deleted にする）
        /// POST: /OrdDetailedView/DeleteDetailRow
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <param name="rowIndex">DataTable の行インデックス</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDetailRow(OrdViewModel model, int rowIndex)
        {
            await this.LoadMasters(model);

            DataTable details = this.LoadEditingDetails();
            if (details == null) { return this.NoTarget(model); }

            this.ReadDetailRows(details, model, -1);

            if (0 <= rowIndex && rowIndex < details.Rows.Count)
            {
                // ★ Delete()。Rows.Remove() だと Deleted にならず DELETE が出ない。
                details.Rows[rowIndex].Delete();
            }

            this.SaveEditingDetails(details);
            model.OrderDetails = details;
            model.Message = "明細行を削除しました（［追加］／［更新］でDBに反映されます）。";

            return View("Index", model);
        }

        /// <summary>対象が無いときの応答</summary>
        /// <param name="model">OrdViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        private ActionResult NoTarget(OrdViewModel model)
        {
            model.Message = "処理対象のデータがありません（画面を開き直して下さい）。";
            return View("Index", model);
        }

        /// <summary>マスタの先頭の ProductID（新規明細行の既定値）</summary>
        /// <param name="products">Products マスタ</param>
        /// <returns>ProductID</returns>
        private static object FirstProductId(DataTable products)
        {
            if (products == null || products.Rows.Count == 0) { return DBNull.Value; }
            return products.Rows[0]["ProductID"];
        }

        /// <summary>
        /// 追加（Ｃ）
        /// POST: /OrdDetailedView/Insert
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Insert(OrdViewModel model)
        {
            return await this.ExecuteCud("OrdInsert", "追加", model);
        }

        /// <summary>
        /// 更新（Ｕ）
        /// POST: /OrdDetailedView/Update
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(OrdViewModel model)
        {
            return await this.ExecuteCud("OrdUpdate", "更新", model);
        }

        /// <summary>
        /// 削除（Ｄ）
        /// POST: /OrdDetailedView/Delete
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(OrdViewModel model)
        {
            return await this.ExecuteCud("OrdDelete", "削除", model);
        }

        /// <summary>ＣＵＤを実行する</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="caption">結果メッセージ用の見出し</param>
        /// <param name="model">OrdViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        private async Task<ActionResult> ExecuteCud(string methodName, string caption, OrdViewModel model)
        {
            await this.LoadMasters(model);

            DataTable dt = this.LoadEditingRow();
            if (dt == null || dt.Rows.Count == 0)
            {
                model.Message = "処理対象のデータがありません（画面を開き直して下さい）。";
                return View("Index", model);
            }

            DataTable details = this.LoadEditingDetails();

            // 画面の入力値を DataRow へ読み戻す（削除は取得時の値で排他するので読み戻さない）
            if (methodName != "OrdDelete")
            {
                OrdDetailedViewController.ModelToRow(model, dt.Rows[0]);
                this.ReadDetailRows(details, model, -1);
            }

            OrdReturnValue returnValue = await this.CallLayerB(methodName, model, dt, details);
            if (returnValue == null)
            {
                // 業務例外＝ロールバック済み。入力を残してやり直せるようにする。
                this.SaveEditingRow(dt);
                this.SaveEditingDetails(details);
                model.OrderDetails = details;
                return View("Index", model);
            }

            dt.AcceptChanges();
            this.SaveEditingRow(dt);
            if (details != null) { details.AcceptChanges(); }
            this.SaveEditingDetails(details);
            model.OrderDetails = details;

            model.Message = caption + "しました（"
                + (returnValue.InsertCount + returnValue.UpdateCount + returnValue.DeleteCount) + " 件"
                + "／明細 追加 " + returnValue.DetailInsertCount + " 件・更新 " + returnValue.DetailUpdateCount
                + " 件・削除 " + returnValue.DetailDeleteCount + " 件）。";

            // ★ 追加・削除の後は、この画面のＣＵＤを止める（同じ行を二重に追加／削除できないように）。
            //   更新は続けて行える（AcceptChanges で Original が現在値に揃うので楽観排他も成立する）。
            if (methodName != "OrdUpdate")
            {
                model.CudDone = true;
                model.Message += "［戻る］で一覧に戻って下さい。";
            }

            return View("Index", model);
        }

        #region 補助

        /// <summary>ＤＤＬ用のマスタを取得する</summary>
        /// <param name="model">OrdViewModel</param>
        private async Task LoadMasters(OrdViewModel model)
        {
            OrdReturnValue rv = await this.CallLayerB("OrdMasters", model, null, null);
            if (rv == null) { return; }

            model.Customers = rv.Customers;
            model.Employees = rv.Employees;
            model.Shippers = rv.Shippers;
            model.Products = rv.Products;
        }

        /// <summary>Ｂ層を呼び出す</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="model">OrdViewModel</param>
        /// <param name="order">ＣＵＤの対象（参照系は null）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private async Task<OrdReturnValue> CallLayerB(
            string methodName, OrdViewModel model, DataTable order, DataTable details)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdParameterValue parameterValue = new OrdParameterValue(
                this.ControllerName, "-", methodName, "SQL", this.UserInfo);

            parameterValue.OrderID = model.OrderID;
            parameterValue.Order = order;
            parameterValue.OrderDetails = details;

            OrdReturnValue returnValue = (OrdReturnValue)await (new LayerB())
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

        #region 画面 ⇔ DataRow

        /// <summary>画面の明細セル値を DataTable へ読み戻す</summary>
        /// <param name="details">編集中の明細 DataTable</param>
        /// <param name="model">OrdViewModel</param>
        /// <param name="targetRowIndex">確定する既存行の行インデックス（-1＝追加行のみ）</param>
        /// <remarks>
        /// ★ 読み戻す行は「追加行は常に／既存行はその行の［更新］のとき／削除行は対象外」
        ///   （opentouryo-batch-update の Web 共通ルール）。
        /// </remarks>
        private void ReadDetailRows(DataTable details, OrdViewModel model, int targetRowIndex)
        {
            if (details == null || model.DetailRows == null) { return; }

            foreach (OrdDetailRowViewModel row in model.DetailRows)
            {
                if (row.RowIndex < 0 || details.Rows.Count <= row.RowIndex) { continue; }

                DataRow dr = details.Rows[row.RowIndex];
                if (dr.RowState == DataRowState.Deleted) { continue; }
                if (dr.RowState != DataRowState.Added && row.RowIndex != targetRowIndex) { continue; }

                OrdDetailedViewController.SetIfChanged(dr, "ProductID", row.ProductID);
                OrdDetailedViewController.SetIfChanged(dr, "UnitPrice", row.UnitPrice);
                OrdDetailedViewController.SetIfChanged(dr, "Quantity", row.Quantity);
                OrdDetailedViewController.SetIfChanged(dr, "Discount", row.Discount);
            }
        }

        /// <summary>DataRow の値を ViewModel に移す</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="model">OrdViewModel</param>
        private static void RowToModel(DataRow dr, OrdViewModel model)
        {
            model.OrderID = OrdDetailedViewController.CellText(dr, "OrderID");
            model.DetailCustomerID = OrdDetailedViewController.CellText(dr, "CustomerID");
            model.DetailEmployeeID = OrdDetailedViewController.CellText(dr, "EmployeeID");
            model.DetailShipVia = OrdDetailedViewController.CellText(dr, "ShipVia");
            model.OrderDate = OrdDetailedViewController.CellText(dr, "OrderDate");
            model.RequiredDate = OrdDetailedViewController.CellText(dr, "RequiredDate");
            model.ShippedDate = OrdDetailedViewController.CellText(dr, "ShippedDate");
            model.Freight = OrdDetailedViewController.CellText(dr, "Freight");
            model.ShipName = OrdDetailedViewController.CellText(dr, "ShipName");
            model.ShipAddress = OrdDetailedViewController.CellText(dr, "ShipAddress");
            model.ShipCity = OrdDetailedViewController.CellText(dr, "ShipCity");
            model.ShipRegion = OrdDetailedViewController.CellText(dr, "ShipRegion");
            model.ShipPostalCode = OrdDetailedViewController.CellText(dr, "ShipPostalCode");
            model.DetailShipCountry = OrdDetailedViewController.CellText(dr, "ShipCountry");
        }

        /// <summary>ViewModel の入力値を DataRow に読み戻す</summary>
        /// <param name="model">OrdViewModel</param>
        /// <param name="dr">対象の DataRow</param>
        /// <remarks>
        /// ★ 値が変わったときだけ代入する＝Original（取得時の値）を壊さず、
        ///   無駄な Modified も作らない。
        /// </remarks>
        private static void ModelToRow(OrdViewModel model, DataRow dr)
        {
            OrdDetailedViewController.SetIfChanged(dr, "CustomerID", model.DetailCustomerID);
            OrdDetailedViewController.SetIfChanged(dr, "EmployeeID", model.DetailEmployeeID);
            OrdDetailedViewController.SetIfChanged(dr, "ShipVia", model.DetailShipVia);
            OrdDetailedViewController.SetIfChanged(dr, "OrderDate", model.OrderDate);
            OrdDetailedViewController.SetIfChanged(dr, "RequiredDate", model.RequiredDate);
            OrdDetailedViewController.SetIfChanged(dr, "ShippedDate", model.ShippedDate);
            OrdDetailedViewController.SetIfChanged(dr, "Freight", model.Freight);
            OrdDetailedViewController.SetIfChanged(dr, "ShipName", model.ShipName);
            OrdDetailedViewController.SetIfChanged(dr, "ShipAddress", model.ShipAddress);
            OrdDetailedViewController.SetIfChanged(dr, "ShipCity", model.ShipCity);
            OrdDetailedViewController.SetIfChanged(dr, "ShipRegion", model.ShipRegion);
            OrdDetailedViewController.SetIfChanged(dr, "ShipPostalCode", model.ShipPostalCode);
            OrdDetailedViewController.SetIfChanged(dr, "ShipCountry", model.DetailShipCountry);
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

            string current = OrdDetailedViewController.CellText(dr, columnName);
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
    }
}
