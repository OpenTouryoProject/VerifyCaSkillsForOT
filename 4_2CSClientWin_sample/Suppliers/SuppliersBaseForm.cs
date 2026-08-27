//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：共通フッタを持つ中間 BaseForm（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersBaseForm
//* クラス日本語名  ：共通フッタ（メイン ボタン5つ）を持つ BaseForm
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

using Touryo.Infrastructure.Business.RichClient.Presentation;

namespace _2CSClientWin_sample.Suppliers
{
    /// <summary>共通フッタ（メイン ボタン5つ）を持つ BaseForm</summary>
    /// <remarks>
    /// 共通仕様：すべての画面のメイン ボタンはフッタ部に5つ配置する。
    /// 各画面（このクラスの派生）は初期処理でキャプションと活性状態を動的に設定する。
    /// ★ ボタンの ID 接頭辞 btn がイベントの自動結線を決めるので、
    ///   ハンドラは派生先の画面クラスに UOC_btnMain1_Click(RcFxEventArgs) で書く
    ///   （ベース Form 上のコントロールも再帰検索で結線される）。
    /// </remarks>
    public class SuppliersBaseForm : MyBaseControllerWin
    {
        /// <summary>メイン ボタン1</summary>
        protected Button btnMain1;
        /// <summary>メイン ボタン2</summary>
        protected Button btnMain2;
        /// <summary>メイン ボタン3</summary>
        protected Button btnMain3;
        /// <summary>メイン ボタン4</summary>
        protected Button btnMain4;
        /// <summary>メイン ボタン5</summary>
        protected Button btnMain5;

        /// <summary>フッタ部のパネル</summary>
        private Panel pnlFooter;

        /// <summary>コンストラクタ</summary>
        public SuppliersBaseForm()
        {
            this.InitializeFooter();
        }

        /// <summary>共通フッタ（メイン ボタン5つ）を組み立てる</summary>
        private void InitializeFooter()
        {
            this.pnlFooter = new Panel();
            this.pnlFooter.Dock = DockStyle.Bottom;
            this.pnlFooter.Height = 48;
            this.pnlFooter.Name = "pnlFooter";

            this.btnMain1 = SuppliersBaseForm.CreateMainButton("btnMain1", 8);
            this.btnMain2 = SuppliersBaseForm.CreateMainButton("btnMain2", 148);
            this.btnMain3 = SuppliersBaseForm.CreateMainButton("btnMain3", 288);
            this.btnMain4 = SuppliersBaseForm.CreateMainButton("btnMain4", 428);
            this.btnMain5 = SuppliersBaseForm.CreateMainButton("btnMain5", 568);

            this.pnlFooter.Controls.Add(this.btnMain1);
            this.pnlFooter.Controls.Add(this.btnMain2);
            this.pnlFooter.Controls.Add(this.btnMain3);
            this.pnlFooter.Controls.Add(this.btnMain4);
            this.pnlFooter.Controls.Add(this.btnMain5);

            this.Controls.Add(this.pnlFooter);
        }

        /// <summary>メイン ボタンを1つ生成する</summary>
        /// <param name="name">コントロール名（接頭辞 btn ＝自動結線の対象）</param>
        /// <param name="left">配置位置</param>
        /// <returns>ボタン</returns>
        private static Button CreateMainButton(string name, int left)
        {
            Button btn = new Button();
            btn.Name = name;
            btn.Text = "－";
            btn.Enabled = false;          // 既定は「未使用」。使う画面が初期処理で上書きする。
            btn.Location = new Point(left, 10);
            btn.Size = new Size(132, 28);
            return btn;
        }

        /// <summary>メイン ボタンのキャプションと活性状態をまとめて設定する</summary>
        /// <param name="captions">5つ分のキャプション（null または空文字＝未使用＝非活性）</param>
        protected void SetMainButtons(params string[] captions)
        {
            Button[] buttons = new Button[] { this.btnMain1, this.btnMain2, this.btnMain3, this.btnMain4, this.btnMain5 };

            for (int i = 0; i < buttons.Length; i++)
            {
                string caption = (captions != null && i < captions.Length) ? captions[i] : null;

                if (string.IsNullOrEmpty(caption))
                {
                    // 不要なボタンは disable にする（共通仕様）
                    buttons[i].Text = "－";
                    buttons[i].Enabled = false;
                }
                else
                {
                    buttons[i].Text = caption;
                    buttons[i].Enabled = true;
                }
            }
        }
    }
}
