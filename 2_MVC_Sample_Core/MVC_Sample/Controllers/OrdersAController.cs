//**********************************************************************************
//* トランザクション・テーブル（Orders）保守：画面Ａ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersAController
//* クラス日本語名  ：Orders 画面Ａ（件数確認・画面遷移）
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

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace MVC_Sample.Controllers
{
    /// <summary>
    /// Orders 画面Ａ（件数確認 → OK ダイアログ表示／画面Ｂへ遷移）
    /// </summary>
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class OrdersAController : MyBaseMVControllerCore
    {
        /// <summary>
        /// 画面の初期表示
        /// GET: /OrdersA/
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <returns>初期表示状態の画面（ViewResult）</returns>
        [HttpGet]
        public IActionResult Index(OrdersViewModel model)
        {
            return View(model);
        }

        /// <summary>
        /// Orders のデータ件数を確認する（共通Dao 経由）
        /// POST: /OrdersA/SelectCount
        /// </summary>
        /// <param name="model">OrdersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectCount(OrdersViewModel model)
        {
            // ↓Ｂ層実行：Orders の件数確認------------------------------------
            // ★ MethodName は UOC メソッド名に対応する。既存のＢ層に相乗りするので
            //   this.ActionName ではなく UOC 名を明示する（別の UOC と衝突するため）。
            OrdersParameterValue parameterValue = new OrdersParameterValue(
                this.ControllerName, "-", "OrdersSelectCount", "SQL", this.UserInfo);

            OrdersReturnValue returnValue = (OrdersReturnValue)await (new LayerB())
                .DoBusinessLogicAsync(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行：Orders の件数確認------------------------------------

            if (returnValue.ErrorFlag)
            {
                model.Message = returnValue.ErrorMessage;
            }
            else
            {
                model.Message = returnValue.Obj.ToString() + "件のデータがあります";
            }

            return View("Index", model);
        }
    }
}
