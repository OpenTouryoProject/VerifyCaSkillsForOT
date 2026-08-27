//**********************************************************************************
//* 初期画面の選択ダイアログ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：ScreenSelector
//* クラス日本語名  ：初期画面の選択ダイアログ
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

namespace _2CSClientWin_sample.Suppliers
{
    /// <summary>初期画面の選択ダイアログ</summary>
    /// <remarks>
    /// 共通仕様：初期画面は Program.cs から起動した選択ダイアログの結果で振り分ける。
    /// ★ フレームワークの画面（MyBaseControllerWin 派生）ではなく素の Form。
    ///   ログイン後・業務画面の起動前に出す選択専用のダイアログのため。
    /// </remarks>
    public class ScreenSelector : Form
    {
        /// <summary>選択された画面</summary>
        public enum SelectedScreen
        {
            /// <summary>既存のサンプル画面（Form1）</summary>
            Sample,
            /// <summary>マスタ保守（Suppliers）画面Ａ</summary>
            SuppliersMaintenance
        }

        /// <summary>選択結果</summary>
        public SelectedScreen Selected { get; private set; }

        /// <summary>コンストラクタ</summary>
        public ScreenSelector()
        {
            this.Text = "画面の選択";
            this.Width = 420;
            this.Height = 220;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.Selected = ScreenSelector.SelectedScreen.Sample;

            Label guide = new Label();
            guide.Text = "起動する画面を選択して下さい。";
            guide.Location = new Point(16, 16);
            guide.Size = new Size(380, 20);
            this.Controls.Add(guide);

            Button btnSample = new Button();
            btnSample.Text = "サンプル画面（Form1）";
            btnSample.Location = new Point(16, 48);
            btnSample.Size = new Size(380, 32);
            btnSample.Click += delegate
            {
                this.Selected = ScreenSelector.SelectedScreen.Sample;
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnSample);

            Button btnSuppliers = new Button();
            btnSuppliers.Text = "マスタ保守（Suppliers）";
            btnSuppliers.Location = new Point(16, 88);
            btnSuppliers.Size = new Size(380, 32);
            btnSuppliers.Click += delegate
            {
                this.Selected = ScreenSelector.SelectedScreen.SuppliersMaintenance;
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnSuppliers);

            Button btnCancel = new Button();
            btnCancel.Text = "終了";
            btnCancel.Location = new Point(16, 128);
            btnCancel.Size = new Size(380, 28);
            btnCancel.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            this.CancelButton = btnCancel;
        }
    }
}
