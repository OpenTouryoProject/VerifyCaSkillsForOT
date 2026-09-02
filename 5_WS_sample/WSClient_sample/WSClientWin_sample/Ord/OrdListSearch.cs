//**********************************************************************************
//* 受注管理（Ord）：画面Ａ＝条件検索一覧（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdListSearch
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

using WSIFType_sample;

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using Touryo.Infrastructure.Business.RichClient.Presentation;
using Touryo.Infrastructure.Framework.RichClient.Presentation;
using Touryo.Infrastructure.Framework.Transmission;

namespace WSClientWin_sample.Ord
{
    /// <summary>受注 条件検索一覧（画面Ａ）</summary>
    /// <remarks>
    /// 仕様：
    /// ・検索条件を入力可能にし、Ｂ層（Ｄ層は共通Dao）の条件検索を実行して一覧に表示する。
    ///   一覧の表示値はＳＱＬでマスタ・テーブルと JOIN して変換済み（得意先名・担当者名・配送業者名）。
    /// ・［追加］（画面遷移ボタン）で画面Ｂ（OrdDetailedView）を「追加＝Ｃ」モードで開く。
    /// ・行の［詳細］ボタンで画面Ｂを「詳細＝Ｒ／編集・削除＝ＵＤ」モードで開く。
    /// </remarks>
    public class OrdListSearch : OrdBaseForm
    {
        /// <summary>1ページの表示件数</summary>
        private const int PageSizeValue = 20;

        /// <summary>［詳細］ボタン列の名前</summary>
        private const string DetailColumnName = "btnDetail";

        /// <summary>一覧（表示値変換済み）</summary>
        private DataTable dtOrders;

        /// <summary>ＤＤＬ用のマスタ</summary>
        private DataTable dtCustomers;
        private DataTable dtEmployees;
        private DataTable dtShippers;

        /// <summary>現在のページ番号（1 起算）と総件数</summary>
        private int pageIndex = 1;
        private int totalCount = 0;

        private BindingSource bindingSource = new BindingSource();
        private DataGridView dgvOrders;
        private ComboBox ddlCustomerID;
        private ComboBox ddlEmployeeID;
        private ComboBox ddlShipVia;
        private TextBox txtShipCountry;
        private Label labelPager;
        private Label labelMessage;

        /// <summary>コンストラクタ</summary>
        public OrdListSearch()
        {
            this.Text = "受注管理（Ord）：条件検索一覧";
            this.Width = 1100;
            this.Height = 620;
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- 検索条件（マスタ・テーブル関連項目は ＤＤＬ 化する） ---
            this.Controls.Add(OrdListSearch.MakeLabel("得意先", 12, 16));
            this.ddlCustomerID = OrdListSearch.MakeCombo("ddlCustomerID", 90, 12, 200);
            this.Controls.Add(this.ddlCustomerID);

            this.Controls.Add(OrdListSearch.MakeLabel("担当者", 300, 16));
            this.ddlEmployeeID = OrdListSearch.MakeCombo("ddlEmployeeID", 370, 12, 160);
            this.Controls.Add(this.ddlEmployeeID);

            this.Controls.Add(OrdListSearch.MakeLabel("配送業者", 545, 16));
            this.ddlShipVia = OrdListSearch.MakeCombo("ddlShipVia", 620, 12, 160);
            this.Controls.Add(this.ddlShipVia);

            this.Controls.Add(OrdListSearch.MakeLabel("出荷先国", 795, 16));
            this.txtShipCountry = new TextBox();
            this.txtShipCountry.Name = "txtShipCountry";
            this.txtShipCountry.Location = new Point(865, 12);
            this.txtShipCountry.Size = new Size(150, 24);
            this.Controls.Add(this.txtShipCountry);

            // --- ページャ／メッセージ ---
            this.labelPager = new Label();
            this.labelPager.Name = "labelPager";
            this.labelPager.Location = new Point(12, 48);
            this.labelPager.Size = new Size(1050, 20);
            this.Controls.Add(this.labelPager);

            this.labelMessage = new Label();
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Location = new Point(12, 72);
            this.labelMessage.Size = new Size(1050, 20);
            this.Controls.Add(this.labelMessage);

            // --- 一覧（共通仕様：DataGridView に DataSource をバインド） ---
            this.dgvOrders = new DataGridView();
            this.dgvOrders.Name = "dgvOrders";
            this.dgvOrders.Location = new Point(12, 100);
            this.dgvOrders.Size = new Size(1060, 410);
            this.dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.dgvOrders.ReadOnly = true;                // 一覧は参照のみ（更新は画面Ｂ）
            this.dgvOrders.AllowUserToAddRows = false;
            this.dgvOrders.AllowUserToDeleteRows = false;
            this.dgvOrders.AutoGenerateColumns = false;
            this.dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // ★ DataGridViewButtonColumn（グリッド内ボタン列）はフレームワークの自動結線対象外
            //   （btn 接頭辞の UOC_btn…_Click にならない）＝素の CellContentClick で拾う。
            this.dgvOrders.CellContentClick += this.dgvOrders_CellContentClick;

            this.dgvOrders.DataSource = this.bindingSource;
            this.Controls.Add(this.dgvOrders);
        }

        #region 初期化・ボタン

        /// <summary>初期化処理</summary>
        protected override void UOC_FormInit()
        {
            // 検索条件の ＤＤＬ 用にマスタを取得する
            OrdReturnValue rv = this.CallLayerB("OrdMasters", null, null);
            if (rv != null)
            {
                this.dtCustomers = rv.Customers;
                this.dtEmployees = rv.Employees;
                this.dtShippers = rv.Shippers;
            }

            this.BindSearchConditionDdl();
            this.BuildColumns();
            this.SetButtons();
        }

        /// <summary>終了処理</summary>
        protected override void UOC_FormEnd()
        {
        }

        /// <summary>共通仕様：メイン ボタン5つを設定する</summary>
        private void SetButtons()
        {
            this.SetMainButtons("検索", "追加", "前ページ", "次ページ", "閉じる");

            // 不要なボタンは disable にする（先頭／最終ページではページ移動できない）
            this.btnMain3.Enabled = this.pageIndex > 1;
            this.btnMain4.Enabled = this.pageIndex < this.TotalPages;

            this.labelPager.Text = "全 " + this.totalCount + " 件／" + this.pageIndex + " / "
                + Math.Max(this.TotalPages, 1) + " ページ（" + OrdListSearch.PageSizeValue + " 件ずつ）";
        }

        /// <summary>総ページ数</summary>
        private int TotalPages
        {
            get { return (this.totalCount + OrdListSearch.PageSizeValue - 1) / OrdListSearch.PageSizeValue; }
        }

        /// <summary>btnMain1（検索）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain1_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.pageIndex = 1;
            this.SearchPage("検索しました");
        }

        /// <summary>btnMain2（追加＝画面Ｂへ遷移／Ｃモード）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        /// <remarks>仕様：画面Ａの追加ボタンから遷移した場合、画面ＢのＣ（追加）を活性にする。</remarks>
        protected void UOC_btnMain2_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.OpenDetailedView(null);
        }

        /// <summary>btnMain3（前ページ）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain3_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.MovePage(this.pageIndex - 1);
        }

        /// <summary>btnMain4（次ページ）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain4_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.MovePage(this.pageIndex + 1);
        }

        /// <summary>btnMain5（閉じる）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain5_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.Close();
        }

        /// <summary>ページを移動する</summary>
        /// <param name="targetPage">移動先のページ番号（1 起算）</param>
        private void MovePage(int targetPage)
        {
            if (targetPage < 1) { targetPage = 1; }
            this.pageIndex = targetPage;
            this.SearchPage("ページを移動しました");
        }

        #endregion

        #region グリッドのイベント（［詳細］ボタン列）

        /// <summary>一覧の［詳細］ボタンが押されたときの処理</summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        /// <remarks>
        /// ★ DataGridViewButtonColumn は自動結線の対象外なので、素の CellContentClick で
        ///   e.ColumnIndex / e.RowIndex を見て自前で分岐する。
        /// </remarks>
        private void dgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) { return; }
            if (this.dgvOrders.Columns[e.ColumnIndex].Name != OrdListSearch.DetailColumnName) { return; }

            DataRowView drv = this.dgvOrders.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (drv == null) { return; }

            this.OpenDetailedView(Convert.ToString(drv.Row["OrderID"]));
        }

        /// <summary>画面Ｂ（OrdDetailedView）を開く</summary>
        /// <param name="orderId">対象の OrderID（null／空＝追加モード）</param>
        /// <remarks>共通仕様：子画面表示は ShowDialog(this)</remarks>
        private void OpenDetailedView(string orderId)
        {
            using (OrdDetailedView dialog = new OrdDetailedView(orderId))
            {
                dialog.ShowDialog(this);

                if (dialog.Updated)
                {
                    // 更新されたら一覧を取り直す（追加は IDENTITY 採番のため特に必要）
                    this.SearchPage("最新の一覧を取得しました");
                }
            }
        }

        #endregion

        #region 検索・バインド

        /// <summary>条件検索を実行してページを表示する</summary>
        /// <param name="message">表示するメッセージ</param>
        private void SearchPage(string message)
        {
            OrdReturnValue rv = this.CallLayerB("OrdListSearch", null, null);
            if (rv == null) { return; }

            this.dtOrders = rv.Orders;
            this.totalCount = rv.TotalCount;
            this.bindingSource.DataSource = this.dtOrders;

            this.labelMessage.Text = message + "（" + this.totalCount + " 件中 "
                + this.pageIndex + " / " + Math.Max(this.TotalPages, 1) + " ページ）。";
            this.SetButtons();
        }

        /// <summary>検索条件の ＤＤＬ を作る</summary>
        private void BindSearchConditionDdl()
        {
            OrdListSearch.FillCombo(this.ddlCustomerID, this.dtCustomers, "CustomerID", "CompanyName");
            OrdListSearch.FillCombo(this.ddlEmployeeID, this.dtEmployees, "EmployeeID", "EmployeeName");
            OrdListSearch.FillCombo(this.ddlShipVia, this.dtShippers, "ShipperID", "CompanyName");
        }

        /// <summary>一覧の列を作る（マスタ関連項目はＳＱＬの JOIN で表示値に変換済み）</summary>
        private void BuildColumns()
        {
            this.dgvOrders.Columns.Clear();

            OrdListSearch.AddTextColumn(this.dgvOrders, "OrderID", "OrderID", 70);
            OrdListSearch.AddTextColumn(this.dgvOrders, "CustomerName", "得意先", 170);
            OrdListSearch.AddTextColumn(this.dgvOrders, "EmployeeName", "担当者", 130);
            OrdListSearch.AddTextColumn(this.dgvOrders, "OrderDate", "受注日", 100);
            OrdListSearch.AddTextColumn(this.dgvOrders, "RequiredDate", "要求日", 100);
            OrdListSearch.AddTextColumn(this.dgvOrders, "ShippedDate", "出荷日", 100);
            OrdListSearch.AddTextColumn(this.dgvOrders, "ShipperName", "配送業者", 130);
            OrdListSearch.AddTextColumn(this.dgvOrders, "Freight", "運賃", 70);
            OrdListSearch.AddTextColumn(this.dgvOrders, "ShipName", "出荷先", 150);
            OrdListSearch.AddTextColumn(this.dgvOrders, "ShipCity", "出荷先市", 100);
            OrdListSearch.AddTextColumn(this.dgvOrders, "ShipCountry", "出荷先国", 100);

            DataGridViewButtonColumn detail = new DataGridViewButtonColumn();
            detail.Name = OrdListSearch.DetailColumnName;
            detail.HeaderText = "";
            detail.Text = "詳細";
            detail.UseColumnTextForButtonValue = true;
            detail.Width = 60;
            this.dgvOrders.Columns.Add(detail);
        }

        #endregion

        #region 部品

        /// <summary>ラベルを生成する</summary>
        private static Label MakeLabel(string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.Size = new Size(75, 20);
            return l;
        }

        /// <summary>コンボ ボックスを生成する</summary>
        private static ComboBox MakeCombo(string name, int x, int y, int w)
        {
            ComboBox cb = new ComboBox();
            cb.Name = name;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.Location = new Point(x, y);
            cb.Size = new Size(w, 24);
            return cb;
        }

        /// <summary>検索条件の ComboBox にマスタを流し込む（先頭は「（すべて）」）</summary>
        private static void FillCombo(ComboBox cb, DataTable master, string valueField, string textField)
        {
            cb.Items.Clear();
            cb.Items.Add(new MasterItem("（すべて）", ""));

            if (master != null)
            {
                foreach (DataRow r in master.Rows)
                {
                    cb.Items.Add(new MasterItem(Convert.ToString(r[textField]), Convert.ToString(r[valueField]).Trim()));
                }
            }
            cb.SelectedIndex = 0;
        }

        /// <summary>選択中の値を返す</summary>
        private static string SelectedValue(ComboBox cb)
        {
            MasterItem item = cb.SelectedItem as MasterItem;
            return (item == null) ? "" : item.Value;
        }

        /// <summary>読み取り専用のテキスト列を足す</summary>
        private static void AddTextColumn(DataGridView grid, string col, string header, int width)
        {
            DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
            c.Name = col; c.DataPropertyName = col; c.HeaderText = header; c.Width = width; c.ReadOnly = true;
            grid.Columns.Add(c);
        }

        /// <summary>ComboBox の表示名と値の組</summary>
        private class MasterItem
        {
            private readonly string name;

            /// <summary>値</summary>
            public string Value { get; private set; }

            /// <summary>コンストラクタ</summary>
            public MasterItem(string name, string value)
            {
                this.name = name;
                this.Value = value;
            }

            /// <summary>表示名</summary>
            public override string ToString()
            {
                return this.name;
            }
        }

        #endregion

        #region Ｂ層呼び出し

        /// <summary>Ｂ層を呼び出す（3層＝サービス論理名で伝送）</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="orderId">対象の OrderID（使わないなら null）</param>
        /// <param name="order">ＣＵＤの対象（使わないなら null）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private OrdReturnValue CallLayerB(string methodName, string orderId, DataTable order)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdParameterValue pv = new OrdParameterValue(
                this.Name, "-", methodName, "SQL", MyBaseControllerWin.UserInfo);

            pv.CustomerID = OrdListSearch.SelectedValue(this.ddlCustomerID);
            pv.EmployeeID = OrdListSearch.SelectedValue(this.ddlEmployeeID);
            pv.ShipVia = OrdListSearch.SelectedValue(this.ddlShipVia);
            pv.ShipCountry = this.txtShipCountry.Text;
            pv.PageIndex = this.pageIndex;
            pv.PageSize = OrdListSearch.PageSizeValue;
            pv.OrderID = orderId;
            pv.Order = order;

            // ★ 3層はサービス論理名で呼ぶ＝トランザクション（分離レベル・コミット）はサーバ側。
            CallController callController = new CallController(MyBaseControllerWin.UserInfo);
            OrdReturnValue rv = (OrdReturnValue)callController.Invoke(this.GetServiceName(), pv);

            if (rv.ErrorFlag)
            {
                this.labelMessage.Text = rv.ErrorMessage;
                MessageBox.Show(rv.ErrorMessage, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return rv;
            // ↑Ｂ層実行---------------------------------------------------------
        }

        #endregion
    }
}
