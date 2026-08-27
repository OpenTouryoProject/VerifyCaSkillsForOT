//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守：画面Ｂ（Ｐ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersScreenB
//* クラス日本語名  ：Suppliers 画面Ｂ（一覧＆更新：RowState バッチ）
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

using System.Data;
using System.Drawing;
using System.Windows.Forms;

using Touryo.Infrastructure.Business.RichClient.Presentation;
using Touryo.Infrastructure.Framework.RichClient.Presentation;
using Touryo.Infrastructure.Public.Db;

namespace _2CSClientWin_sample.Suppliers
{
    /// <summary>Suppliers 画面Ｂ（一覧＆更新：RowState バッチ）</summary>
    /// <remarks>
    /// グリッド中で行の追加・更新・削除を行い、DataTable の RowState に覚えさせて、
    /// ［バッチ更新］で Ｂ層 → 自動生成Dao 経由で一括反映する。
    /// ★ WinForms は DataGridView のセル編集がバインド先の DataTable に自動反映されるので、
    ///   Web のような「セルから DataRow への読み戻し」も Session 保持も要らない。
    /// </remarks>
    public class SuppliersScreenB : SuppliersBaseForm
    {
        /// <summary>編集中の DataTable（フォームのフィールドに保持する＝Session 不要）</summary>
        private DataTable dtSuppliers;

        /// <summary>グリッドのバインド ソース</summary>
        private BindingSource bindingSource = new BindingSource();

        /// <summary>一覧のグリッド</summary>
        private DataGridView dgvSuppliers;

        /// <summary>行追加ボタン（グリッド外）</summary>
        private Button btnAddRow;

        /// <summary>結果表示ラベル</summary>
        private Label labelMessage;

        /// <summary>コンストラクタ</summary>
        public SuppliersScreenB()
        {
            this.Text = "Suppliers 画面Ｂ（一覧＆バッチ更新）";
            this.Width = 900;
            this.Height = 560;
            this.StartPosition = FormStartPosition.CenterParent;

            // ［行追加］はグリッド外の通常ボタン（空行＝RowState:Added を足す）
            this.btnAddRow = new Button();
            this.btnAddRow.Name = "btnAddRow";
            this.btnAddRow.Text = "行追加";
            this.btnAddRow.Location = new Point(12, 12);
            this.btnAddRow.Size = new Size(100, 28);
            this.Controls.Add(this.btnAddRow);

            this.labelMessage = new Label();
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Location = new Point(124, 18);
            this.labelMessage.Size = new Size(740, 20);
            this.Controls.Add(this.labelMessage);

            // 共通仕様：一覧表示は DataGridView（DataSource にバインド）
            this.dgvSuppliers = new DataGridView();
            this.dgvSuppliers.Name = "dgvSuppliers";
            this.dgvSuppliers.Location = new Point(12, 48);
            this.dgvSuppliers.Size = new Size(852, 400);
            this.dgvSuppliers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            // ★ AutoGenerateColumns にすると DataTable の全12列がそのまま出て、
            //   Web 側（MVC / Web Forms）の一覧と列構成が食い違う。
            //   フレームワーク間で同じ画面仕様にするため、列を明示して揃える。
            this.dgvSuppliers.AutoGenerateColumns = false;
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "SupplierID", "SupplierID", 90, true);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "CompanyName", "CompanyName", 200, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "ContactName", "ContactName", 150, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "ContactTitle", "ContactTitle", 150, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "Address", "Address", 180, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "City", "City", 110, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "Region", "Region", 90, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "PostalCode", "PostalCode", 100, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "Country", "Country", 110, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "Phone", "Phone", 140, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "Fax", "Fax", 140, false);
            SuppliersScreenB.AddColumn(this.dgvSuppliers, "HomePage", "HomePage", 180, false);

            // 行削除は標準の Delete キーで可（バインド経由＝DataRowView.Delete()＝Deleted になる）
            this.dgvSuppliers.AllowUserToDeleteRows = true;

            // 追加はグリッド外の［行追加］で行うので、グリッド末尾の新規行は出さない
            this.dgvSuppliers.AllowUserToAddRows = false;

            this.dgvSuppliers.DataSource = this.bindingSource;
            this.Controls.Add(this.dgvSuppliers);
        }

        /// <summary>初期化処理</summary>
        protected override void UOC_FormInit()
        {
            // 共通仕様：メイン ボタン5つのキャプションを動的に設定し、不要なものは disable にする
            this.SetMainButtons("一覧取得", "バッチ更新", null, null, "閉じる");
        }

        /// <summary>終了処理</summary>
        protected override void UOC_FormEnd()
        {
        }

        /// <summary>グリッドに列を1つ足す</summary>
        /// <param name="grid">対象のグリッド</param>
        /// <param name="dataPropertyName">バインドする列名</param>
        /// <param name="headerText">見出し</param>
        /// <param name="width">幅</param>
        /// <param name="readOnly">読み取り専用にするか（IDENTITY の主キーなど）</param>
        private static void AddColumn(DataGridView grid, string dataPropertyName, string headerText, int width, bool readOnly)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.Name = dataPropertyName;
            col.DataPropertyName = dataPropertyName;
            col.HeaderText = headerText;
            col.Width = width;

            // SupplierID は IDENTITY（自動採番）＝画面から編集させない
            col.ReadOnly = readOnly;

            grid.Columns.Add(col);
        }
        /// <summary>
        /// グリッドの保留中の編集を確定する
        /// </summary>
        /// <remarks>
        /// ★ EndEdit() はセルの編集しか確定しない。行（DataRowView）の保留編集は
        ///   CurrencyManager.EndCurrentEdit() まで確定しないので、これを呼ばずに
        ///   追加・削除・バッチ更新・確認ダイアログへ進むと入力が失われる。
        /// </remarks>
        private void CommitGridEdits()
        {
            this.dgvSuppliers.EndEdit();
            this.bindingSource.CurrencyManager.EndCurrentEdit();
        }

        /// <summary>btnMain1（一覧取得）のクリック イベント</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain1_Click(RcFxEventArgs rcFxEventArgs)
        {
            SuppliersReturnValue returnValue = this.CallLayerB("SuppliersSelectAll", null, rcFxEventArgs.ControlName);
            if (returnValue == null) { return; }

            this.dtSuppliers = returnValue.Suppliers;
            this.bindingSource.DataSource = this.dtSuppliers;
            this.labelMessage.Text = "一覧を取得しました（" + this.dtSuppliers.Rows.Count + " 件）。";
        }

        /// <summary>btnMain2（バッチ更新）のクリック イベント</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain2_Click(RcFxEventArgs rcFxEventArgs)
        {
            if (this.dtSuppliers == null)
            {
                MessageBox.Show("先に［一覧取得］を実行して下さい。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ★ 確認ダイアログの前に保留編集を確定する
            this.CommitGridEdits();

            // 共通仕様：YES/NO 確認ダイアログは MessageBoxButtons.YesNo
            DialogResult result = MessageBox.Show(
                "バッチ更新します。よろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) { return; }

            SuppliersReturnValue returnValue = this.CallLayerB("SuppliersBatchUpdate", this.dtSuppliers, rcFxEventArgs.ControlName);
            if (returnValue == null) { return; }

            // 反映できたので確定（RowState を Unchanged に戻す）
            this.dtSuppliers.AcceptChanges();

            string message = "更新しました（挿入 " + returnValue.InsertCount
                + " 件／更新 " + returnValue.UpdateCount
                + " 件／削除 " + returnValue.DeleteCount + " 件）。";

            // ★ IDENTITY の採番値は DataTable に戻らないので、一覧を取り直す。
            SuppliersReturnValue reloadRv = this.CallLayerB("SuppliersSelectAll", null, rcFxEventArgs.ControlName);
            if (reloadRv != null)
            {
                this.dtSuppliers = reloadRv.Suppliers;
                this.bindingSource.DataSource = this.dtSuppliers;
            }

            this.labelMessage.Text = message;
            MessageBox.Show(message, "バッチ更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>btnMain5（閉じる）のクリック イベント</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnMain5_Click(RcFxEventArgs rcFxEventArgs)
        {
            this.Close();
        }

        /// <summary>btnAddRow（行追加）のクリック イベント</summary>
        /// <param name="rcFxEventArgs">イベント ハンドラの共通引数</param>
        protected void UOC_btnAddRow_Click(RcFxEventArgs rcFxEventArgs)
        {
            if (this.dtSuppliers == null)
            {
                MessageBox.Show("先に［一覧取得］を実行して下さい。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ★ 先に保留編集を確定する
            this.CommitGridEdits();

            DataRow nr = this.dtSuppliers.NewRow();

            // ★ DB 側 NOT NULL の列は空文字で初期化する。
            //   DBNull のまま INSERT すると SqlException 515 になる。
            nr["CompanyName"] = "";
            this.dtSuppliers.Rows.Add(nr);

            this.labelMessage.Text = "行を追加しました（［バッチ更新］でDBに反映されます）。";
        }

        /// <summary>Ｂ層を呼び出す（2CS の手動トランザクション制御つき）</summary>
        /// <param name="methodName">UOC メソッド名</param>
        /// <param name="suppliers">バッチ更新対象（参照系は null）</param>
        /// <param name="controlName">イベント発生元のコントロール名</param>
        /// <returns>戻り値クラス（業務例外時は null）</returns>
        private SuppliersReturnValue CallLayerB(string methodName, DataTable suppliers, string controlName)
        {
            // ↓Ｂ層実行---------------------------------------------------------
            SuppliersParameterValue parameterValue = new SuppliersParameterValue(
                this.Name, controlName, methodName, "SQL", MyBaseControllerWin.UserInfo);
            parameterValue.Suppliers = suppliers;

            LayerB layerB = new LayerB();

            try
            {
                SuppliersReturnValue returnValue = (SuppliersReturnValue)layerB.DoBusinessLogic(
                    parameterValue, DbEnum.IsolationLevelEnum.ReadCommitted);

                if (returnValue.ErrorFlag)
                {
                    // ★ 2CS は業務例外でも自動ロールバックしない＝明示的にロールバックする。
                    //   RowState は残るのでやり直せる。
                    LayerB.RollbackAndClose();
                    this.labelMessage.Text = returnValue.ErrorMessage;
                    MessageBox.Show(returnValue.ErrorMessage, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // ★ 2CS は明示的にコミットする（呼ばないと確定しない）
                LayerB.CommitAndClose();
                return returnValue;
            }
            catch
            {
                LayerB.RollbackAndClose();
                throw;
            }
            // ↑Ｂ層実行---------------------------------------------------------
        }
    }
}
