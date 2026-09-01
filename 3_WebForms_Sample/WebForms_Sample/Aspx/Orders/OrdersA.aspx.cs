//**********************************************************************************
//* トランザクション・テーブル（Orders）保守：画面Ａ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersA
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

using System;

using Touryo.Infrastructure.Business.Presentation;
using Touryo.Infrastructure.CustomControl;
using Touryo.Infrastructure.Framework.Presentation;
using Touryo.Infrastructure.Framework.Util;
using Touryo.Infrastructure.Public.Db;

namespace WebForms_Sample.Aspx.Orders
{
    /// <summary>Orders 画面Ａ（件数確認・画面遷移）</summary>
    public partial class OrdersA : MyBaseController
    {
        #region ページ ロードの共通処理（UOC メソッド）

        /// <summary>初期表示時の処理</summary>
        protected override void UOC_FormInit()
        {
            this.SetMainButtons();
        }

        /// <summary>ポストバック時の処理</summary>
        protected override void UOC_FormInit_PostBack()
        {
            this.SetMainButtons();
        }

        /// <summary>
        /// 共通仕様：フッタ部のメイン ボタン5つのキャプションと活性状態を動的に設定する
        /// </summary>
        /// <remarks>
        /// ボタン自体はマスタ ページ（testBlankScreen.master）に配置されている。
        /// 画面ごとの制御はコードビハインド（初期処理）で行う＝この画面で使うのは2つだけ。
        /// </remarks>
        private void SetMainButtons()
        {
            WebCustomButton btn1 = (WebCustomButton)this.GetMasterWebControl("btnMButton1");
            WebCustomButton btn2 = (WebCustomButton)this.GetMasterWebControl("btnMButton2");
            WebCustomButton btn3 = (WebCustomButton)this.GetMasterWebControl("btnMButton3");
            WebCustomButton btn4 = (WebCustomButton)this.GetMasterWebControl("btnMButton4");
            WebCustomButton btn5 = (WebCustomButton)this.GetMasterWebControl("btnMButton5");

            btn1.Text = "件数確認"; btn1.Enabled = true;
            btn2.Text = "一覧へ";   btn2.Enabled = true;

            // 使わないボタンは disable にする（共通仕様）
            btn3.Text = "－"; btn3.Enabled = false;
            btn4.Text = "－"; btn4.Enabled = false;
            btn5.Text = "－"; btn5.Enabled = false;
        }

        #endregion

        #region マスタ ページ上のボタンのイベント

        /// <summary>
        /// btnMButton1 のクリック イベント（件数確認）
        /// </summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL（遷移しないので空文字列）</returns>
        /// <remarks>
        /// ★ ハンドラ名の接頭辞は「マスタ ページのファイル名」（testBlankScreen）。
        ///   コンテンツ .aspx の名前ではない。間違えるとコンパイルは通るが呼ばれない。
        /// </remarks>
        protected string UOC_testBlankScreen_btnMButton1_Click(FxEventArgs fxEventArgs)
        {
            // ↓Ｂ層実行：Suppliers の件数確認----------------------------------
            OrdersParameterValue parameterValue = new OrdersParameterValue(
                this.ContentPageFileNoEx, fxEventArgs.ButtonID, "OrdersSelectCount", "SQL", this.UserInfo);

            // Ｂ層呼出し（Web はインプロセス直呼び）
            OrdersReturnValue returnValue = (OrdersReturnValue)new LayerB()
                .DoBusinessLogic(parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);
            // ↑Ｂ層実行：Suppliers の件数確認----------------------------------

            if (returnValue.ErrorFlag)
            {
                // 業務例外（業務続行可能なエラー）
                this.lblMessage.Text = returnValue.ErrorMessage;
            }
            else
            {
                // 共通仕様：メッセージ ダイアログはフレームワークの ShowOKMessageDialog を使う
                this.ShowOKMessageDialog(
                    "OrdersCount",
                    returnValue.Obj.ToString() + "件のデータがあります",
                    FxEnum.IconType.Information,
                    "件数確認");

                this.lblMessage.Text = returnValue.Obj.ToString() + "件のデータがあります";
            }

            return string.Empty;
        }

        /// <summary>
        /// btnMButton2 のクリック イベント（画面Ｂへ遷移）
        /// </summary>
        /// <param name="fxEventArgs">イベント ハンドラの共通引数</param>
        /// <returns>遷移先 URL</returns>
        protected string UOC_testBlankScreen_btnMButton2_Click(FxEventArgs fxEventArgs)
        {
            // 遷移先は SCDefinition（画面遷移定義）に定義しておく必要がある
            // （FxScreenTransitionCheck = on のため、未定義の遷移は拒否される）。
            return "~/Aspx/Orders/OrdersB.aspx";
        }

        #endregion
    }
}
