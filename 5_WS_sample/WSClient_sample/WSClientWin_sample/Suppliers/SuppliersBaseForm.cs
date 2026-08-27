//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：共通フッタを持つ中間 BaseForm（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersBaseForm
//* クラス日本語名  ：共通フッタ（メイン ボタン5つ）＋サービス論理名 DDL を持つ BaseForm
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

namespace WSClientWin_sample.Suppliers
{
    /// <summary>共通フッタ（メイン ボタン5つ）＋サービス論理名 DDL を持つ BaseForm</summary>
    /// <remarks>
    /// 共通仕様：
    ///  ・すべての画面のメイン ボタンはフッタ部に5つ配置する（キャプションは画面ごとに動的設定）。
    ///  ・WSClientWin_sample は画面端に「通信制御機能のサービス論理名」を選択する DDL を配置する。
    /// ★ ボタンの ID 接頭辞 btn がイベントの自動結線を決めるので、
    ///   ハンドラは派生先の画面クラスに UOC_btnMain1_Click(RcFxEventArgs) で書く。
    /// ★ DDL の接頭辞 ddl は SelectedIndexChanged に結線される。ここでは値を読むだけなので
    ///   UOC_ddlServiceName_SelectedIndexChanged は実装しない（未実装のイベントは無視される）。
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

        /// <summary>通信制御機能のサービス論理名を選択する DDL</summary>
        protected ComboBox ddlServiceName;

        /// <summary>フッタ部のパネル</summary>
        private Panel pnlFooter;

        /// <summary>コンストラクタ</summary>
        public SuppliersBaseForm()
        {
            this.InitializeFooter();
        }

        /// <summary>共通フッタ（メイン ボタン5つ＋サービス論理名 DDL）を組み立てる</summary>
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

            // 画面端（フッタ右端）にサービス論理名の DDL を置く。
            // ★ 絶対座標＋Anchor(Right) にしてはいけない。
            //   このコンストラクタは「基底」なので、派生画面が this.Width を設定する前に走る。
            //   その時点のフォーム幅（既定 300）を基準に右端からの距離が確定してしまい、
            //   後でフォームを広げると DDL が画面外（実測 X=1610／パネル幅 1004）へ飛ぶ。
            //   幅に依存しないよう、右ドッキングのパネルに載せる。
            Panel pnlService = new Panel();
            pnlService.Name = "pnlService";
            pnlService.Dock = DockStyle.Right;
            pnlService.Width = 320;

            Label lbl = new Label();
            lbl.Text = "サービス論理名";
            lbl.Location = new Point(4, 14);
            lbl.Size = new Size(96, 20);
            pnlService.Controls.Add(lbl);

            this.ddlServiceName = new ComboBox();
            this.ddlServiceName.Name = "ddlServiceName";
            this.ddlServiceName.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ddlServiceName.Location = new Point(104, 10);
            this.ddlServiceName.Size = new Size(210, 24);

            // TMProtocolDefinition（経路）／TMInProcessDefinition（実体）に定義した
            // サービス論理名。3経路すべてを選べるようにする。
            //   protocol=1 … インプロセス（WSServer_sample を直接ロード）
            //   protocol=4 … WCF netTcpBinding 経由（ServiceInterface\WCFService）
            //   protocol=5 … ASP.NET WebAPI 経由（ServiceInterface\ASPNETWebService）
            this.ddlServiceName.Items.Add(new ComboBoxItem("インプロセス呼出", "SuppliersInProcess"));
            this.ddlServiceName.Items.Add(new ComboBoxItem("WCF TCPサービス呼出", "SuppliersWCFTcp"));
            this.ddlServiceName.Items.Add(new ComboBoxItem("ASP.NET WebAPI呼出", "SuppliersWebAPI"));
            this.ddlServiceName.SelectedIndex = 0;

            pnlService.Controls.Add(this.ddlServiceName);

            // 右ドッキングを先に足してから、残りをボタン領域にする
            this.pnlFooter.Controls.Add(pnlService);

            this.Controls.Add(this.pnlFooter);
        }

        /// <summary>ComboBox の表示名と値の組</summary>
        /// <remarks>
        /// ★ サンプルの ComboBoxItem は Form1 の private な入れ子クラスでフレームワークの型ではない。
        ///   ここでは同じ形のものをこのクラスに用意する。
        /// </remarks>
        protected class ComboBoxItem
        {
            /// <summary>表示名</summary>
            private readonly string name;

            /// <summary>値（サービス論理名）</summary>
            public string Value { get; private set; }

            /// <summary>コンストラクタ</summary>
            /// <param name="name">表示名</param>
            /// <param name="value">値</param>
            public ComboBoxItem(string name, string value)
            {
                this.name = name;
                this.Value = value;
            }

            /// <summary>ComboBox には表示名を出す</summary>
            /// <returns>表示名</returns>
            public override string ToString()
            {
                return this.name;
            }
        }

        /// <summary>選択中のサービス論理名を返す</summary>
        /// <returns>サービス論理名</returns>
        protected string GetServiceName()
        {
            return ((ComboBoxItem)this.ddlServiceName.SelectedItem).Value;
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
