//**********************************************************************************
//* トランザクション・テーブル（Orders）保守（Ｐ層：ViewModel）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersViewModel
//* クラス日本語名  ：Orders 保守画面の ViewModel
//*
//* 作成日時        ：2026/08/28
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/28  生技              新規作成
//**********************************************************************************

using System.Collections.Generic;
using System.Data;

namespace MVC_Sample.Models.ViewModels
{
    /// <summary>Orders 保守画面の ViewModel</summary>
    public class OrdersViewModel : BaseViewModel
    {
        /// <summary>画面に表示するメッセージ（JavaScript のダイアログで表示する）</summary>
        public string Message { get; set; }

        #region 検索条件

        /// <summary>検索条件：CustomerID</summary>
        public string CustomerID { get; set; }

        /// <summary>検索条件：EmployeeID</summary>
        public string EmployeeID { get; set; }

        /// <summary>検索条件：ShipVia</summary>
        public string ShipVia { get; set; }

        /// <summary>検索条件：ShipCountry（前方一致）</summary>
        public string ShipCountry { get; set; }

        #endregion

        #region ページング

        /// <summary>現在のページ番号（1 起算）</summary>
        public int PageIndex { get; set; }

        /// <summary>1ページの表示件数</summary>
        public int PageSize { get; set; }

        /// <summary>条件に一致する総件数</summary>
        public int TotalCount { get; set; }

        /// <summary>総ページ数</summary>
        public int TotalPages
        {
            get
            {
                if (this.PageSize <= 0) { return 0; }
                return (this.TotalCount + this.PageSize - 1) / this.PageSize;
            }
        }

        /// <summary>
        /// 編集中か（＝ページングを止める）
        /// </summary>
        /// <remarks>
        /// 仕様：バッチ更新処理が開始されたらページングを止め、処理対象を当該結果セットに限定する。
        /// ページを切り替えると再検索になり RowState が失われるため。
        /// </remarks>
        public bool IsEditing { get; set; }

        #endregion

        /// <summary>一覧の表示元（RowState を保持した DataTable）</summary>
        public DataTable Orders { get; set; }

        #region ドロップダウン用のマスタ

        /// <summary>Customers（CustomerID / CompanyName）</summary>
        public DataTable Customers { get; set; }

        /// <summary>Employees（EmployeeID / EmployeeName）</summary>
        public DataTable Employees { get; set; }

        /// <summary>Shippers（ShipperID / CompanyName）</summary>
        public DataTable Shippers { get; set; }

        #endregion

        /// <summary>ポストバックで戻ってくる明細（モデルバインド先）</summary>
        public List<OrderRowViewModel> Rows { get; set; }

        /// <summary>コンストラクタ</summary>
        public OrdersViewModel()
        {
            this.Message = "";
            this.PageIndex = 1;
            this.PageSize = 20;
            this.Rows = new List<OrderRowViewModel>();
        }
    }

    /// <summary>Orders 一覧の1行分の ViewModel</summary>
    /// <remarks>
    /// ★ DataTable 側は型付き（int / DateTime / decimal）だが、画面から戻る値は文字列。
    ///   DataRow へ書き戻すときに列の型へ変換する（変換はコントローラ側の SetIfChanged）。
    /// </remarks>
    public class OrderRowViewModel
    {
        /// <summary>DataTable の行インデックス（Deleted 行は描画しないので表示連番とはズレる）</summary>
        public int RowIndex { get; set; }

        /// <summary>CustomerID</summary>
        public string CustomerID { get; set; }

        /// <summary>EmployeeID</summary>
        public string EmployeeID { get; set; }

        /// <summary>OrderDate</summary>
        public string OrderDate { get; set; }

        /// <summary>RequiredDate</summary>
        public string RequiredDate { get; set; }

        /// <summary>ShippedDate</summary>
        public string ShippedDate { get; set; }

        /// <summary>ShipVia</summary>
        public string ShipVia { get; set; }

        /// <summary>Freight</summary>
        public string Freight { get; set; }

        /// <summary>ShipName</summary>
        public string ShipName { get; set; }

        /// <summary>ShipAddress</summary>
        public string ShipAddress { get; set; }

        /// <summary>ShipCity</summary>
        public string ShipCity { get; set; }

        /// <summary>ShipRegion</summary>
        public string ShipRegion { get; set; }

        /// <summary>ShipPostalCode</summary>
        public string ShipPostalCode { get; set; }

        /// <summary>ShipCountry</summary>
        public string ShipCountry { get; set; }
    }
}
