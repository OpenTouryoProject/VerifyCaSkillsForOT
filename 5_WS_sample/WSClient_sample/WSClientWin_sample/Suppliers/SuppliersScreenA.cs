//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：画面Ａ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersScreenA
//* クラス日本語名  ：Suppliers 画面Ａ（件数確認・画面遷移）／3層WSクライアント
//*
//* 作成日時        ：2026/08/27
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/27  生技              新規作成
//**********************************************************************************

using System.Drawing;
using System.Windows.Forms;

using WSIFType_sample;

using Touryo.Infrastructure.Business.RichClient.Presentation;
using Touryo.Infrastructure.Framework.RichClient.Presentation;
using Touryo.Infrastructure.Framework.Transmission;

namespace WSClientWin_sample.Suppliers
{
    /// <summary>Suppliers 画面Ａ（件数確認・画面遷移）</summary>
    public class SuppliersScreenA : SuppliersBaseForm
    {
        /// <summary>結果表示ラベル</summary>
        private Label labelMessage;

        /// <summary>コンストラクタ</summary>
        public SuppliersScreenA()
        {
            this.Text = "Suppliers 画面Ａ（件数確認）／3層";
            this.Width = 1020;
            this.Height = 260;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label guide = new Label();
            guide.Text = "［件数確認］で Suppliers のデータ件数を共通Dao 経由で取得し、メッセージ ダイアログで表示します。"
                + "\r\n［一覧へ］で画面Ｂ（一覧＆バッチ更新）を開きます。"
                + "\r\n呼び出し経路は、画面端の「サービス論理名」で切り替えます。";
            guide.Location = new Point(12, 16);
            guide.Size = new Size(980, 56);
            this.Controls.Add(guide);

            this.labelMessage = new Label();
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Location = new Point(12, 82);
            this.labelMessage.Size = new Size(980, 60);
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
            // ★ 3層は通信制御機能経由＝CallController.Invoke（URL やクラス名でなくサービス論理名）。
            //   トランザクションはサーバ側が確定するので、2CS のような手動 Commit/Rollback は不要。
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.Name, rcFxEventArgs.ControlName, "SuppliersSelectCount", "SQL",
                MyBaseControllerWin.UserInfo);

            CallController callController = new CallController(MyBaseControllerWin.UserInfo);
            SuppliersReturnValue returnValue =
                (SuppliersReturnValue)callController.Invoke(this.GetServiceName(), parameterValue);
            // ↑Ｂ層実行：Suppliers の件数確認----------------------------------

            if (returnValue.ErrorFlag)
            {
                this.labelMessage.Text = returnValue.ErrorMessage;
                MessageBox.Show(returnValue.ErrorMessage, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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
