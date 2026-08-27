//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：画面Ａ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersScreenA
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

using _2CSClientWin_sample.Business;
using _2CSClientWin_sample.Common;

using System.Drawing;
using System.Windows.Forms;

using Touryo.Infrastructure.Business.RichClient.Presentation;
using Touryo.Infrastructure.Framework.RichClient.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace _2CSClientWin_sample.Suppliers
{
    /// <summary>Suppliers 画面Ａ（件数確認・画面遷移）</summary>
    public class SuppliersScreenA : SuppliersBaseForm
    {
        /// <summary>結果表示ラベル</summary>
        private Label labelMessage;

        /// <summary>コンストラクタ</summary>
        public SuppliersScreenA()
        {
            this.Text = "Suppliers 画面Ａ（件数確認）";
            this.Width = 720;
            this.Height = 260;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label guide = new Label();
            guide.Text = "［件数確認］で Suppliers のデータ件数を共通Dao 経由で取得し、メッセージ ダイアログで表示します。"
                + "\r\n［一覧へ］で画面Ｂ（一覧＆バッチ更新）を開きます。";
            guide.Location = new Point(12, 16);
            guide.Size = new Size(680, 40);
            this.Controls.Add(guide);

            this.labelMessage = new Label();
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Location = new Point(12, 70);
            this.labelMessage.Size = new Size(680, 60);
            this.Controls.Add(this.labelMessage);
        }

        /// <summary>初期化処理</summary>
        protected override void UOC_FormInit()
        {
            // 共通仕様：メイン ボタン5つのキャプションを動的に設定し、不要なものは disable にする
            this.SetMainButtons("件数確認", "一覧へ", null, null, "閉じる");
        }

        /// <summary>終了処理</summary>
        protected override void UOC_FormEnd()
        {
        }

        /// <summary>btnMain1（件数確認）のクリック イベント</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain1_Click(RcFxEventArgs rcFxEventArgs)
        {
            // ↓Ｂ層実行：Suppliers の件数確認----------------------------------
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.Name, rcFxEventArgs.ControlName, "SuppliersSelectCount", "SQL",
                MyBaseControllerWin.UserInfo);

            LayerB layerB = new LayerB();
            SuppliersReturnValue returnValue;

            try
            {
                returnValue = (SuppliersReturnValue)layerB.DoBusinessLogic(
                    parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);

                if (returnValue.ErrorFlag)
                {
                    // ★ 2CS は業務例外でも自動ロールバックしない＝明示的にロールバックする
                    LayerB.RollbackAndClose();
                    this.labelMessage.Text = returnValue.ErrorMessage;
                    MessageBox.Show(returnValue.ErrorMessage, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ★ 2CS は明示的にコミットする（呼ばないと確定しない）
                LayerB.CommitAndClose();
            }
            catch
            {
                LayerB.RollbackAndClose();
                throw;
            }

            string message = returnValue.Obj.ToString() + "件のデータがあります";
            this.labelMessage.Text = message;

            // 共通仕様：ダイアログは標準機能（MessageBox.Show）を使用する
            MessageBox.Show(message, "件数確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>btnMain2（一覧へ）のクリック イベント</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain2_Click(RcFxEventArgs rcFxEventArgs)
        {
            // 共通仕様：子画面表示は ShowDialog(this)
            using (SuppliersScreenB dialog = new SuppliersScreenB())
            {
                dialog.ShowDialog(this);
            }
        }

        /// <summary>btnMain5（閉じる）のクリック イベント</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain5_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.Close();
        }
    }
}
