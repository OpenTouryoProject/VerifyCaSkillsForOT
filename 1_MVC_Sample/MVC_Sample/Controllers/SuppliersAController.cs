//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：画面Ａ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersAController
//* クラス日本語名  ：Suppliers 画面Ａ（件数確認・画面遷移）
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

using System.Threading.Tasks;
using System.Web.Mvc;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace MVC_Sample.Controllers
{
    /// <summary>
    /// Suppliers 画面Ａ（件数確認 → OK ダイアログ表示／画面Ｂへ遷移）
    /// </summary>
    [Authorize]
    public class SuppliersAController : MyBaseMVController
    {
        /// <summary>
        /// 画面の初期表示
        /// GET: /SuppliersA/
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <returns>初期表示状態の画面（ViewResult）</returns>
        [HttpGet]
        public ActionResult Index(SuppliersViewModel model)
        {
            return View(model);
        }

        /// <summary>
        /// Suppliers のデータ件数を確認する（共通Dao 経由）
        /// POST: /SuppliersA/SelectCount
        /// </summary>
        /// <param name="model">SuppliersViewModel</param>
        /// <returns>再描画（ViewResult）</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelectCount(SuppliersViewModel model)
        {
            // ↓Ｂ層実行：Suppliers の件数確認----------------------------------
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.ControllerName, "-", "SuppliersSelectCount", "SQL", this.UserInfo);

            // Ｂ層呼出し＋都度コミット
            SuppliersReturnValue returnValue = (SuppliersReturnValue)await (new LayerB())
                .DoBusinessLogicAsync(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行：Suppliers の件数確認----------------------------------

            if (returnValue.ErrorFlag)
            {
                // 業務例外（業務続行可能なエラー）
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
