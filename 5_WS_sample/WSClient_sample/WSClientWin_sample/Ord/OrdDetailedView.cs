//**********************************************************************************
//* 受注管理（Ord）：画面Ｂ＝詳細・更新（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdDetailedView
//* クラス日本語名  ：受注 詳細・更新（画面Ｂ）
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
    /// <summary>受注 詳細・更新（画面Ｂ）</summary>
    /// <remarks>
    /// 仕様：
    /// ・初期処理でマスタ・テーブルを取得し「マスタ・テーブル値入力用ＤＤＬ」を生成する。
    /// ・画面Ａの詳細ボタンから遷移した場合は自動生成Dao の参照（Ｒ）で詳細表示し、
    ///   ＵＤ（更新・削除）ボタンを活性にする。
    /// ・画面Ａの追加ボタンから遷移した場合は Ｃ（追加）ボタンを活性にする。
    /// ・ＣＵＤボタンを押すと YES/NO ダイアログを表示し、YES 押下後に処理を実行する。
    /// ★ 編集中のデータは DataTable のままフォームのフィールドに持つ（WinForms は Session 不要）。
    ///   取得時の値が DataRowVersion.Original に残るので、Ｂ層で楽観排他が成立する。
    /// </remarks>
    public class OrdDetailedView : OrdBaseForm
    {
        /// <summary>対象の OrderID（null／空＝追加モード）</summary>
        private readonly string targetOrderId;

        /// <summary>追加（Ｃ）モードか</summary>
        private bool isNew;

        /// <summary>ＣＵＤが成立したか（呼び出し元が一覧を取り直すために見る）</summary>
        public bool Updated { get; private set; }

        /// <summary>編集中の1行（RowState と Original を保持する）</summary>
        private DataTable dtOrder;

        /// <summary>ＤＤＬ用のマスタ</summary>
        private DataTable dtCustomers;
        private DataTable dtEmployees;
        private DataTable dtShippers;

        private TextBox txtOrderID;
        private ComboBox ddlCustomerID;
        private ComboBox ddlEmployeeID;
        private ComboBox ddlShipVia;
        private TextBox txtOrderDate;
        private TextBox txtRequiredDate;
        private TextBox txtShippedDate;
        private TextBox txtFreight;
        private TextBox txtShipName;
        private TextBox txtShipAddress;
        private TextBox txtShipCity;
        private TextBox txtShipRegion;
        private TextBox txtShipPostalCode;
        private TextBox txtShipCountry;
        private Label labelMessage;

        /// <summary>コンストラクタ</summary>
        /// <param name="orderId">対象の OrderID（null／空＝追加モード）</param>
        public OrdDetailedView(string orderId)
        {
            this.targetOrderId = orderId;
            this.isNew = string.IsNullOrEmpty(orderId);
            this.Updated = false;

            this.Text = "受注管理（Ord）：詳細・更新";
            this.Width = 700;
            this.Height = 560;
            this.StartPosition = FormStartPosition.CenterParent;

            int y = 16;

            this.txtOrderID = this.AddTextRow("OrderID", ref y);
            this.txtOrderID.ReadOnly = true;              // IDENTITY（自動採番）＝入力不可

            this.ddlCustomerID = this.AddComboRow("得意先（Customers）", ref y);
            this.ddlEmployeeID = this.AddComboRow("担当者（Employees）", ref y);

            this.txtOrderDate = this.AddTextRow("受注日（OrderDate）", ref y);
            this.txtRequiredDate = this.AddTextRow("要求日（RequiredDate）", ref y);
            this.txtShippedDate = this.AddTextRow("出荷日（ShippedDate）", ref y);

            this.ddlShipVia = this.AddComboRow("配送業者（Shippers）", ref y);

            this.txtFreight = this.AddTextRow("運賃（Freight）", ref y);
            this.txtShipName = this.AddTextRow("出荷先名（ShipName）", ref y);
            this.txtShipAddress = this.AddTextRow("出荷先住所（ShipAddress）", ref y);
            this.txtShipCity = this.AddTextRow("出荷先市（ShipCity）", ref y);
            this.txtShipRegion = this.AddTextRow("出荷先地域（ShipRegion）", ref y);
            this.txtShipPostalCode = this.AddTextRow("出荷先郵便番号（ShipPostalCode）", ref y);
            this.txtShipCountry = this.AddTextRow("出荷先国（ShipCountry）", ref y);

            this.labelMessage = new Label();
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Location = new Point(12, y + 8);
            this.labelMessage.Size = new Size(650, 20);
            this.Controls.Add(this.labelMessage);
        }

        #region 初期化・ボタン

        /// <summary>初期化処理</summary>
        /// <remarks>仕様：初期処理でマスタ・テーブルを取得し、入力用ＤＤＬを生成する。</remarks>
        protected override void UOC_FormInit()
        {
            // --- ① マスタ・テーブルの取得 → ＤＤＬ 生成 ---
            OrdReturnValue masters = this.CallLayerB("OrdMasters", null, null);
            if (masters != null)
            {
                this.dtCustomers = masters.Customers;
                this.dtEmployees = masters.Employees;
                this.dtShippers = masters.Shippers;
            }

            OrdDetailedView.FillCombo(this.ddlCustomerID, this.dtCustomers, "CustomerID", "CompanyName");
            OrdDetailedView.FillCombo(this.ddlEmployeeID, this.dtEmployees, "EmployeeID", "EmployeeName");
            OrdDetailedView.FillCombo(this.ddlShipVia, this.dtShippers, "ShipperID", "CompanyName");

            // --- ② 詳細（自動生成Dao の参照＝Ｒ）。追加モードは 0 件＝スキーマだけ戻る ---
            OrdReturnValue detail = this.CallLayerB("OrdDetailedView", this.targetOrderId, null);
            if (detail == null) { this.SetButtons(); return; }

            this.dtOrder = detail.Order;

            if (this.dtOrder.Rows.Count == 0)
            {
                // 追加モード：空行を1行足す（RowState＝Added）
                this.isNew = true;
                this.dtOrder.Rows.Add(this.dtOrder.NewRow());
            }
            else
            {
                this.isNew = false;
            }

            this.RowToScreen();
            this.SetButtons();
        }

        /// <summary>終了処理</summary>
        protected override void UOC_FormEnd()
        {
        }

        /// <summary>共通仕様：メイン ボタン5つを設定する</summary>
        /// <remarks>
        /// 仕様：詳細ボタンから遷移＝ＵＤ（更新・削除）を活性、
        ///       追加ボタンから遷移＝Ｃ（追加）を活性にする。
        /// </remarks>
        private void SetButtons()
        {
            this.SetMainButtons("追加", "更新", "削除", null, "戻る");

            this.btnMain1.Enabled = this.isNew;
            this.btnMain2.Enabled = !this.isNew;
            this.btnMain3.Enabled = !this.isNew;

            this.Text = "受注管理（Ord）：" + (this.isNew ? "追加" : "詳細・更新");
        }

        /// <summary>btnMain1（追加＝Ｃ）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain1_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.ExecuteCud("OrdInsert", "追加します。よろしいですか？", "追加");
        }

        /// <summary>btnMain2（更新＝Ｕ）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain2_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.ExecuteCud("OrdUpdate", "更新します。よろしいですか？", "更新");
        }

        /// <summary>btnMain3（削除＝Ｄ）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain3_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.ExecuteCud("OrdDelete", "削除します。よろしいですか？", "削除");
        }

        /// <summary>btnMain5（戻る）</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain5_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.Close();
        }

        /// <summary>ＣＵＤを実行する（YES/NO 確認ダイアログつき）</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="question">確認ダイアログの本文</param>
        /// <param name="caption">結果メッセージ用の見出し</param>
        private void ExecuteCud(string methodName, string question, string caption)
        {
            if (this.dtOrder == null || this.dtOrder.Rows.Count == 0)
            {
                MessageBox.Show("処理対象のデータがありません。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ★ 削除は画面の入力値を使わない（取得時の値で楽観排他する）ので読み戻さない。
            if (methodName != "OrdDelete") { this.ScreenToRow(); }

            // 共通仕様：YES/NO 確認ダイアログは MessageBoxButtons.YesNo
            if (MessageBox.Show(question, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                this.labelMessage.Text = caption + "を中止しました。";
                return;
            }

            OrdReturnValue rv = this.CallLayerB(methodName, this.targetOrderId, this.dtOrder);
            if (rv == null) { return; }

            this.dtOrder.AcceptChanges();
            this.Updated = true;

            string message = caption + "しました（" + (rv.InsertCount + rv.UpdateCount + rv.DeleteCount) + " 件）。";
            this.labelMessage.Text = message;
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ★ 追加・削除の後は、この画面のＣＵＤを止める（同じ行を二重に追加／削除できないように）。
            //   更新は続けて行える（AcceptChanges で Original が現在値に揃うので楽観排他も成立する）。
            if (methodName != "OrdUpdate")
            {
                this.btnMain1.Enabled = false;
                this.btnMain2.Enabled = false;
                this.btnMain3.Enabled = false;
                this.labelMessage.Text = message + "［戻る］で一覧に戻って下さい。";
            }
        }

        #endregion

        #region 画面 ⇔ DataRow

        /// <summary>DataRow の値を画面に反映する</summary>
        private void RowToScreen()
        {
            DataRow dr = this.dtOrder.Rows[0];

            this.txtOrderID.Text = OrdDetailedView.CellText(dr, "OrderID");

            OrdDetailedView.SelectByValue(this.ddlCustomerID, OrdDetailedView.CellText(dr, "CustomerID"));
            OrdDetailedView.SelectByValue(this.ddlEmployeeID, OrdDetailedView.CellText(dr, "EmployeeID"));
            OrdDetailedView.SelectByValue(this.ddlShipVia, OrdDetailedView.CellText(dr, "ShipVia"));

            this.txtOrderDate.Text = OrdDetailedView.CellText(dr, "OrderDate");
            this.txtRequiredDate.Text = OrdDetailedView.CellText(dr, "RequiredDate");
            this.txtShippedDate.Text = OrdDetailedView.CellText(dr, "ShippedDate");
            this.txtFreight.Text = OrdDetailedView.CellText(dr, "Freight");
            this.txtShipName.Text = OrdDetailedView.CellText(dr, "ShipName");
            this.txtShipAddress.Text = OrdDetailedView.CellText(dr, "ShipAddress");
            this.txtShipCity.Text = OrdDetailedView.CellText(dr, "ShipCity");
            this.txtShipRegion.Text = OrdDetailedView.CellText(dr, "ShipRegion");
            this.txtShipPostalCode.Text = OrdDetailedView.CellText(dr, "ShipPostalCode");
            this.txtShipCountry.Text = OrdDetailedView.CellText(dr, "ShipCountry");
        }

        /// <summary>画面の入力値を DataRow に読み戻す</summary>
        /// <remarks>
        /// ★ 値が変わったときだけ代入する＝Original（取得時の値）を壊さず、
        ///   無駄な Modified も作らない。
        /// </remarks>
        private void ScreenToRow()
        {
            DataRow dr = this.dtOrder.Rows[0];

            OrdDetailedView.SetIfChanged(dr, "CustomerID", OrdDetailedView.SelectedValue(this.ddlCustomerID));
            OrdDetailedView.SetIfChanged(dr, "EmployeeID", OrdDetailedView.SelectedValue(this.ddlEmployeeID));
            OrdDetailedView.SetIfChanged(dr, "ShipVia", OrdDetailedView.SelectedValue(this.ddlShipVia));

            OrdDetailedView.SetIfChanged(dr, "OrderDate", this.txtOrderDate.Text);
            OrdDetailedView.SetIfChanged(dr, "RequiredDate", this.txtRequiredDate.Text);
            OrdDetailedView.SetIfChanged(dr, "ShippedDate", this.txtShippedDate.Text);
            OrdDetailedView.SetIfChanged(dr, "Freight", this.txtFreight.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipName", this.txtShipName.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipAddress", this.txtShipAddress.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipCity", this.txtShipCity.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipRegion", this.txtShipRegion.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipPostalCode", this.txtShipPostalCode.Text);
            OrdDetailedView.SetIfChanged(dr, "ShipCountry", this.txtShipCountry.Text);
        }

        /// <summary>DataRow の値を画面表示用の文字列にする</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <returns>表示用の文字列</returns>
        private static string CellText(DataRow dr, string columnName)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return ""; }

            object v = dr[columnName];
            if (v == null || v == DBNull.Value) { return ""; }
            if (v is DateTime) { return ((DateTime)v).ToString("yyyy/MM/dd"); }
            return Convert.ToString(v).Trim();
        }

        /// <summary>値が変わっているときだけ、列の型に変換して代入する</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <param name="newValue">画面の値（文字列）</param>
        /// <remarks>
        /// ★ Orders の DataTable は型付き（int / DateTime / decimal）。
        ///   文字列をそのまま代入すると例外になるので列の型へ変換する。
        /// ★ Orders は OrderID 以外すべて NULL 許容なので、空欄は DBNull にする。
        /// </remarks>
        private static void SetIfChanged(DataRow dr, string columnName, string newValue)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return; }

            string current = OrdDetailedView.CellText(dr, columnName);
            string edited = (newValue ?? "").Trim();

            if (current == edited) { return; }

            if (edited.Length == 0) { dr[columnName] = DBNull.Value; return; }

            Type t = dr.Table.Columns[columnName].DataType;

            try
            {
                if (t == typeof(int)) { dr[columnName] = int.Parse(edited); }
                else if (t == typeof(decimal)) { dr[columnName] = decimal.Parse(edited); }
                else if (t == typeof(DateTime)) { dr[columnName] = DateTime.Parse(edited); }
                else { dr[columnName] = edited; }
            }
            catch (FormatException)
            {
                // 変換できない入力は無視する（元の値のまま）
            }
        }

        #endregion

        #region 部品

        /// <summary>「ラベル＋テキストボックス」の1行を足す</summary>
        /// <param name="caption">ラベル</param>
        /// <param name="y">配置位置（呼び出しごとに進む）</param>
        /// <returns>テキストボックス</returns>
        private TextBox AddTextRow(string caption, ref int y)
        {
            Label l = new Label();
            l.Text = caption;
            l.Location = new Point(12, y + 4);
            l.Size = new Size(230, 20);
            this.Controls.Add(l);

            TextBox tb = new TextBox();
            tb.Location = new Point(250, y);
            tb.Size = new Size(400, 24);
            this.Controls.Add(tb);

            y += 30;
            return tb;
        }

        /// <summary>「ラベル＋ＤＤＬ」の1行を足す</summary>
        /// <param name="caption">ラベル</param>
        /// <param name="y">配置位置（呼び出しごとに進む）</param>
        /// <returns>コンボ ボックス</returns>
        private ComboBox AddComboRow(string caption, ref int y)
        {
            Label l = new Label();
            l.Text = caption;
            l.Location = new Point(12, y + 4);
            l.Size = new Size(230, 20);
            this.Controls.Add(l);

            ComboBox cb = new ComboBox();
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.Location = new Point(250, y);
            cb.Size = new Size(400, 24);
            this.Controls.Add(cb);

            y += 30;
            return cb;
        }

        /// <summary>ＤＤＬ にマスタを流し込む（先頭は「（未設定）」＝NULL）</summary>
        private static void FillCombo(ComboBox cb, DataTable master, string valueField, string textField)
        {
            cb.Items.Clear();
            cb.Items.Add(new MasterItem("（未設定）", ""));

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

        /// <summary>値で選択する（index ではない＝並び順はマスタの名称順のため）</summary>
        private static void SelectByValue(ComboBox cb, string value)
        {
            string v = (value ?? "").Trim();

            for (int i = 0; i < cb.Items.Count; i++)
            {
                MasterItem item = cb.Items[i] as MasterItem;
                if (item != null && item.Value == v) { cb.SelectedIndex = i; return; }
            }
            cb.SelectedIndex = 0;
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
        /// <param name="orderId">対象の OrderID</param>
        /// <param name="order">ＣＵＤの対象（参照系は null）</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private OrdReturnValue CallLayerB(string methodName, string orderId, DataTable order)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            OrdParameterValue pv = new OrdParameterValue(
                this.Name, "-", methodName, "SQL", MyBaseControllerWin.UserInfo);

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
