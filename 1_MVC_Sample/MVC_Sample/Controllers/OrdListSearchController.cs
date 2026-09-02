//**********************************************************************************
//* 受注管理（Ord）：画面Ａ＝条件検索一覧（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdListSearchController
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

using MVC_Sample.Logic.Business;
using MVC_Sample.Logic.Common;
using MVC_Sample.Models.ViewModels;

using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace MVC_Sample.Controllers
{
    /// <summary>受注 条件検索一覧（画面Ａ）</summary>
    /// <remarks>
    /// 仕様：
    /// ・検索条件を入力可能にし、Ｂ層（Ｄ層は共通Dao）の条件検索を実行して一覧に表示する。
    ///   一覧の表示値はＳＱＬでマスタ・テーブルと JOIN して変換済み。
    /// ・［追加］（画面遷移ボタン）で画面Ｂを「追加＝Ｃ」モードで開く。
    /// ・行の［詳細］で画面Ｂを「詳細＝Ｒ／更新・削除＝ＵＤ」モードで開く。
    /// </remarks>
    [Authorize]
    public class OrdListSearchController : MyBaseMVController
    {
        /// <summary>
        /// 画面の初期表示（ＤＤＬ用のマスタだけ読む）
        /// GET: /OrdListSearch/
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <param name="autoSearch">true＝そのまま条件検索も実行する（画面Ｂから戻ってきたとき）</param>
        /// <returns>初期表示状態の画面（ViewResult）</returns>
        /// <remarks>
        /// ★ Web は画面遷移で入力値が消えるので、画面Ｂへは検索条件をクエリ文字列で渡し、
        ///   戻るときも同じ条件を返してもらって再検索する（Session に持たない）。
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult> Index(OrdViewModel model, bool autoSearch = false)
        {
            if (autoSearch) { return await this.SearchPage(model, "最新の一覧を取得しました"); }

            await this.EnsureMasters(model);
            return View(model);
        }

        /// <summary>
        /// 条件検索（1ページ目から）
        /// POST: /OrdListSearch/Search
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Search(OrdViewModel model)
        {
            model.PageIndex = 1;
            return await this.SearchPage(model, "検索しました");
        }

        /// <summary>
        /// ページ移動
        /// POST: /OrdListSearch/Page
        /// </summary>
        /// <param name="model">OrdViewModel</param>
        /// <param name="targetPage">移動先のページ番号（1 起算）</param>
        /// <returns>再描画（ViewResult）</returns>
        /// <remarks>
        /// ★ 引数名を pageIndex にしてはいけない。フォームの hidden も PageIndex を持つため、
        ///   モデルバインドが大文字小文字を区別せず衝突し、フォーム側（現在ページ）が優先されて
        ///   クエリ文字列で渡した移動先が無視される（＝ページが動かない）。名前を分ける。
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Page(OrdViewModel model, int targetPage)
        {
            model.PageIndex = (targetPage < 1) ? 1 : targetPage;
            return await this.SearchPage(model, "ページを移動しました");
        }

        /// <summary>条件検索を実行してページを表示する</summary>
        /// <param name="model">OrdViewModel</param>
        /// <param name="message">表示するメッセージ</param>
        /// <returns>再描画（ViewResult）</returns>
        private async Task<ActionResult> SearchPage(OrdViewModel model, string message)
        {
            OrdReturnValue returnValue = await this.CallLayerB("OrdListSearch", model);
            if (returnValue == null)
            {
                await this.EnsureMasters(model);
                return View("Index", model);
            }

            model.Orders = returnValue.Orders;
            model.TotalCount = returnValue.TotalCount;
            await this.EnsureMasters(model);

            model.Message = message + "（" + model.TotalCount + " 件中 "
                + model.PageIndex + " / " + System.Math.Max(model.TotalPages, 1) + " ページ）。";

            return View("Index", model);
        }

        #region 補助

        /// <summary>ＤＤＬ用のマスタが未設定なら取得する</summary>
        /// <param name="model">OrdViewModel</param>
        private async Task EnsureMasters(OrdViewModel model)
        {
            if (model.Customers != null) { return; }

            OrdReturnValue rv = await this.CallLayerB("OrdMasters", model);
            if (rv == null) { return; }

            model.Customers = rv.Customers;
            model.Employees = rv.Employees;
            model.Shippers = rv.Shippers;
        }

        /// <summary>Ｂ層を呼び出す</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="model">OrdViewModel（検索条件・ページを渡す）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private async Task<OrdReturnValue> CallLayerB(string methodName, OrdViewModel model)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdParameterValue parameterValue = new OrdParameterValue(
                this.ControllerName, "-", methodName, "SQL", this.UserInfo);

            parameterValue.CustomerID = model.CustomerID;
            parameterValue.EmployeeID = model.EmployeeID;
            parameterValue.ShipVia = model.ShipVia;
            parameterValue.ShipCountry = model.ShipCountry;
            parameterValue.PageIndex = model.PageIndex;
            parameterValue.PageSize = (model.PageSize <= 0) ? 20 : model.PageSize;

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
    }
}
