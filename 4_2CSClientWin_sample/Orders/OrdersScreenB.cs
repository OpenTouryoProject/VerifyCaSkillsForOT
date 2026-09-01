//**********************************************************************************
//* トランザクション・テーブル（Orders）保守：画面Ｂ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersScreenB
//* クラス日本語名  ：Orders 画面Ｂ（条件検索・ページング・バッチ更新）
//*
//* 作成日時        ：2026/08/28
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/28  生技              新規作成
//**********************************************************************************

using _2CSClientWin_sample.Business;
using _2CSClientWin_sample.Common;

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using Touryo.Infrastructure.Business.RichClient.Presentation;
using Touryo.Infrastructure.Framework.RichClient.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace _2CSClientWin_sample.Orders
{
    /// <summary>Orders 画面Ｂ（条件検索・ページング・バッチ更新）</summary>
    /// <remarks>
    /// ★ WinForms は DataGridView のセル編集がバインド先の DataTable に自動反映されるので、
    ///   Web のような「セルから DataRow への読み戻し」も Session 保持も要らない。
    /// </remarks>
    public class OrdersScreenB : OrdersBaseForm
    {
        /// <summary>1ページの表示件数</summary>
        private const int PageSizeValue = 20;

        /// <summary>編集中の DataTable（フォームのフィールドに保持する＝Session 不要）</summary>
        private DataTable dtOrders;

        /// <summary>DDL 用のマスタ</summary>
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
        private Button btnAddRow;
        private Label labelPager;
        private Label labelMessage;

        /// <summary>コンストラクタ</summary>
        public OrdersScreenB()
        {
            this.Text = "Orders 画面Ｂ（条件検索・ページング・バッチ更新）";
            this.Width = 1100;
            this.Height = 620;
            this.StartPosition = FormStartPosition.CenterParent;

            // --- 検索条件（マスタ・テーブル関連項目は DDL 化する） ---
            this.Controls.Add(OrdersScreenB.MakeLabel("得意先", 12, 16));
            this.ddlCustomerID = OrdersScreenB.MakeCombo("ddlCustomerID", 90, 12, 200);
            this.Controls.Add(this.ddlCustomerID);

            this.Controls.Add(OrdersScreenB.MakeLabel("担当者", 300, 16));
            this.ddlEmployeeID = OrdersScreenB.MakeCombo("ddlEmployeeID", 370, 12, 160);
            this.Controls.Add(this.ddlEmployeeID);

            this.Controls.Add(OrdersScreenB.MakeLabel("配送業者", 545, 16));
            this.ddlShipVia = OrdersScreenB.MakeCombo("ddlShipVia", 620, 12, 160);
            this.Controls.Add(this.ddlShipVia);

            this.Controls.Add(OrdersScreenB.MakeLabel("出荷先国", 795, 16));
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

            // --- ［行追加］はグリッド外の通常ボタン ---
            this.btnAddRow = new Button();
            this.btnAddRow.Name = "btnAddRow";
            this.btnAddRow.Text = "行追加";
            this.btnAddRow.Location = new Point(12, 96);
            this.btnAddRow.Size = new Size(100, 28);
            this.Controls.Add(this.btnAddRow);

            // --- 一覧（共通仕様：DataGridView に DataSource をバインド） ---
            this.dgvOrders = new DataGridView();
            this.dgvOrders.Name = "dgvOrders";
            this.dgvOrders.Location = new Point(12, 132);
            this.dgvOrders.Size = new Size(1060, 380);
            this.dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.dgvOrders.AllowUserToDeleteRows = true;   // Delete キーで Deleted になる
            this.dgvOrders.AllowUserToAddRows = false;     // 追加はグリッド外ボタン
            this.dgvOrders.AutoGenerateColumns = false;    // Web 側と列構成を揃えるため明示する
            // ★ DDL 列にマスタに無い値（NULL 等）が来ても例外ダイアログを出さない
            this.dgvOrders.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };
            this.dgvOrders.DataSource = this.bindingSource;
            this.Controls.Add(this.dgvOrders);
        }

        #region 初期化・ボタン

        /// <summary>初期化処理</summary>
        protected override void UOC_FormInit()
        {
            // DDL 用のマスタを取得して検索条件とグリッド列を作る
            OrdersReturnValue rv = this.CallLayerB("OrdersMasters", null);
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

        /// <summary>共通仕様：メイン ボタン5つを設定する（編集中はページングを止める）</summary>
        private void SetButtons()
        {
            bool editing = OrdersScreenB.HasPendingChanges(this.dtOrders);

            this.SetMainButtons("検索", "バッチ更新", "前ページ", "次ページ", "閉じる");

            // ★ 仕様：バッチ更新が開始されたらページングを止め、処理対象を当該結果セットに限定する。
            this.btnMain1.Enabled = !editing;
            this.btnMain3.Enabled = !editing && this.pageIndex > 1;
            this.btnMain4.Enabled = !editing && this.pageIndex < this.TotalPages;

            this.labelPager.Text = "全 " + this.totalCount + " 件／" + this.pageIndex + " / "
                + Math.Max(this.TotalPages, 1) + " ページ（" + OrdersScreenB.PageSizeValue + " 件ずつ）"
                + (editing ? "　※ 編集中のためページングは停止しています。" : "");
        }

        /// <summary>総ページ数</summary>
        private int TotalPages
        {
            get { return (this.totalCount + OrdersScreenB.PageSizeValue - 1) / OrdersScreenB.PageSizeValue; }
        }

        /// <summary>btnMain1（検索）</summary>
        protected void UOC_btnMain1_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.pageIndex = 1;
            this.SearchPage("検索しました");
        }

        /// <summary>btnMain2（バッチ更新）</summary>
        protected void UOC_btnMain2_Click(RcFxEventArgs rcFxEventArgs)
        {
            if (this.dtOrders == null)
            {
                MessageBox.Show("先に［検索］を実行して下さい。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ★ 確認ダイアログの前に保留編集を確定する
            this.CommitGridEdits();

            // 共通仕様：YES/NO 確認ダイアログは MessageBoxButtons.YesNo
            if (MessageBox.Show("バッチ更新します。よろしいですか？", "確認",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            OrdersReturnValue rv = this.CallLayerB("OrdersBatchUpdate", this.dtOrders);
            if (rv == null) { this.SetButtons(); return; }

            this.dtOrders.AcceptChanges();

            string message = "更新しました（挿入 " + rv.InsertCount
                + " 件／更新 " + rv.UpdateCount + " 件／削除 " + rv.DeleteCount + " 件）。";

            // ★ IDENTITY の採番値は DataTable に戻らないので、同じ条件・同じページで取り直す。
            this.SearchPage(message);
            MessageBox.Show(message, "バッチ更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>btnMain3（前ページ）</summary>
        protected void UOC_btnMain3_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.MovePage(this.pageIndex - 1);
        }

        /// <summary>btnMain4（次ページ）</summary>
        protected void UOC_btnMain4_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.MovePage(this.pageIndex + 1);
        }

        /// <summary>btnMain5（閉じる）</summary>
        protected void UOC_btnMain5_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.Close();
        }

        /// <summary>btnAddRow（行追加＝空行を足す）</summary>
        protected void UOC_btnAddRow_Click(RcFxEventArgs rcFxEventArgs)
        {
            if (this.dtOrders == null)
            {
                MessageBox.Show("先に［検索］を実行して下さい。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ★ 先に保留編集を確定する
            this.CommitGridEdits();

            // Orders は OrderID（IDENTITY）以外すべて NULL 許容なので、空行のままでも INSERT できる。
            this.dtOrders.Rows.Add(this.dtOrders.NewRow());

            this.labelMessage.Text = "行を追加しました（［バッチ更新］でDBに反映されます）。";
            this.SetButtons();
        }

        /// <summary>ページを移動する</summary>
        /// <param name="targetPage">移動先のページ番号（1 起算）</param>
        private void MovePage(int targetPage)
        {
            // ★ 仕様：編集中はページングを止める
            if (OrdersScreenB.HasPendingChanges(this.dtOrders))
            {
                MessageBox.Show("編集中はページングできません（［バッチ更新］で反映して下さい）。",
                    "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (targetPage < 1) { targetPage = 1; }
            this.pageIndex = targetPage;
            this.SearchPage("ページを移動しました");
        }

        #endregion

        #region 検索・バインド

        /// <summary>条件検索を実行してページを表示する</summary>
        /// <param name="message">表示するメッセージ</param>
        private void SearchPage(string message)
        {
            OrdersReturnValue rv = this.CallLayerB("OrdersSearch", null);
            if (rv == null) { return; }

            this.dtOrders = rv.Orders;
            this.totalCount = rv.TotalCount;
            this.bindingSource.DataSource = this.dtOrders;

            this.labelMessage.Text = message + "（" + this.totalCount + " 件中 "
                + this.pageIndex + " / " + Math.Max(this.TotalPages, 1) + " ページ）。";
            this.SetButtons();
        }

        /// <summary>
        /// グリッドの保留中の編集を確定する
        /// </summary>
        /// <remarks>
        /// ★ EndEdit() はセルの編集しか確定しない。行（DataRowView）の保留編集は
        ///   CurrencyManager.EndCurrentEdit() まで確定しない。
        /// </remarks>
        private void CommitGridEdits()
        {
            this.dgvOrders.EndEdit();
            this.bindingSource.CurrencyManager.EndCurrentEdit();
        }

        /// <summary>検索条件の DDL を作る</summary>
        private void BindSearchConditionDdl()
        {
            OrdersScreenB.FillCombo(this.ddlCustomerID, this.dtCustomers, "CustomerID", "CompanyName");
            OrdersScreenB.FillCombo(this.ddlEmployeeID, this.dtEmployees, "EmployeeID", "EmployeeName");
            OrdersScreenB.FillCombo(this.ddlShipVia, this.dtShippers, "ShipperID", "CompanyName");
        }

        /// <summary>グリッドの列を作る（Web 側と同じ14列。FK 列は DDL）</summary>
        private void BuildColumns()
        {
            this.dgvOrders.Columns.Clear();

            // OrderID は IDENTITY＝読み取り専用
            OrdersScreenB.AddTextColumn(this.dgvOrders, "OrderID", "OrderID", 70, true);

            // マスタ・テーブル関連項目は DDL 化する
            OrdersScreenB.AddComboColumn(this.dgvOrders, "CustomerID", "得意先", 160, this.dtCustomers, "CustomerID", "CompanyName");
            OrdersScreenB.AddComboColumn(this.dgvOrders, "EmployeeID", "担当者", 130, this.dtEmployees, "EmployeeID", "EmployeeName");

            OrdersScreenB.AddTextColumn(this.dgvOrders, "OrderDate", "OrderDate", 100, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "RequiredDate", "RequiredDate", 100, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "ShippedDate", "ShippedDate", 100, false);

            OrdersScreenB.AddComboColumn(this.dgvOrders, "ShipVia", "配送業者", 130, this.dtShippers, "ShipperID", "CompanyName");

            OrdersScreenB.AddTextColumn(this.dgvOrders, "Freight", "Freight", 70, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "ShipName", "ShipName", 150, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "ShipAddress", "ShipAddress", 150, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "ShipCity", "ShipCity", 100, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "ShipRegion", "ShipRegion", 80, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "ShipPostalCode", "ShipPostalCode", 90, false);
            OrdersScreenB.AddTextColumn(this.dgvOrders, "ShipCountry", "ShipCountry", 100, false);
        }

        #endregion

        #region 部品

        private static Label MakeLabel(string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.Size = new Size(75, 20);
            return l;
        }

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

        private static void AddTextColumn(DataGridView grid, string col, string header, int width, bool readOnly)
        {
            DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
            c.Name = col; c.DataPropertyName = col; c.HeaderText = header; c.Width = width; c.ReadOnly = readOnly;
            grid.Columns.Add(c);
        }

        /// <summary>マスタをバインドした DDL 列を足す（表示は名称・値は ID）</summary>
        private static void AddComboColumn(DataGridView grid, string col, string header, int width,
            DataTable master, string valueField, string textField)
        {
            DataGridViewComboBoxColumn c = new DataGridViewComboBoxColumn();
            c.Name = col; c.DataPropertyName = col; c.HeaderText = header; c.Width = width;
            c.DisplayMember = textField;
            c.ValueMember = valueField;
            c.DataSource = (master == null) ? null : master.Copy();

            grid.Columns.Add(c);
        }

        /// <summary>ComboBox の表示名と値の組</summary>
        private class MasterItem
        {
            private readonly string name;

            public string Value { get; private set; }

            public MasterItem(string name, string value)
            {
                this.name = name;
                this.Value = value;
            }

            public override string ToString()
            {
                return this.name;
            }
        }

        /// <summary>編集中（未反映の変更がある）か</summary>
        private static bool HasPendingChanges(DataTable dt)
        {
            if (dt == null) { return false; }

            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState != DataRowState.Unchanged) { return true; }
            }
            return false;
        }

        #endregion

        #region Ｂ層呼び出し

        /// <summary>Ｂ層を呼び出す（2CS の手動トランザクション制御つき）</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="orders">バッチ更新対象（参照系は null）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private OrdersReturnValue CallLayerB(string methodName, DataTable orders)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdersParameterValue pv = new OrdersParameterValue(
                this.Name, "-", methodName, "SQL", MyBaseControllerWin.UserInfo);

            pv.CustomerID = OrdersScreenB.SelectedValue(this.ddlCustomerID);
            pv.EmployeeID = OrdersScreenB.SelectedValue(this.ddlEmployeeID);
            pv.ShipVia = OrdersScreenB.SelectedValue(this.ddlShipVia);
            pv.ShipCountry = this.txtShipCountry.Text;
            pv.PageIndex = this.pageIndex;
            pv.PageSize = OrdersScreenB.PageSizeValue;
            pv.Orders = orders;

            LayerB layerB = new LayerB();

            try
            {
                OrdersReturnValue rv = (OrdersReturnValue)layerB.DoBusinessLogic(
                    pv, DbEnum.IsolationLevelEnum.ReadCommitted);

                if (rv.ErrorFlag)
                {
                    // ★ 2CS は業務例外でも自動ロールバックしない
                    LayerB.RollbackAndClose();
                    this.labelMessage.Text = rv.ErrorMessage;
                    MessageBox.Show(rv.ErrorMessage, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // ★ 2CS は明示コミット
                LayerB.CommitAndClose();
                return rv;
            }
            catch
            {
                LayerB.RollbackAndClose();
                throw;
            }
            // ↑Ｂ層実行---------------------------------------------------------
        }

        #endregion
    }
}
