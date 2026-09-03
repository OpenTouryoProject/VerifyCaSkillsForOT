//**********************************************************************************
//* 受注管理（Ord）：条件検索一覧・詳細・更新（Ｐ層：ViewModel）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdViewModel
//* クラス日本語名  ：受注管理（Ord）画面の ViewModel
//*
//* 作成日時        ：2026/09/02
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/09/02  生技              新規作成
//**********************************************************************************

using System.Collections.Generic;
using System.Data;

namespace MVC_Sample.Models.ViewModels
{
    /// <summary>受注管理（Ord）画面の ViewModel</summary>
    public class OrdViewModel : BaseViewModel
    {
        /// <summary>画面に表示するメッセージ（JavaScript のダイアログで表示する）</summary>
        public string Message { get; set; }

        #region 検索条件（画面Ａ＝OrdListSearch）

        /// <summary>検索条件：CustomerID</summary>
        public string CustomerID { get; set; }

        /// <summary>検索条件：EmployeeID</summary>
        public string EmployeeID { get; set; }

        /// <summary>検索条件：ShipVia</summary>
        public string ShipVia { get; set; }

        /// <summary>検索条件：ShipCountry（前方一致）</summary>
        public string ShipCountry { get; set; }

        #endregion

        #region ページング（画面Ａ）

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

        #endregion

        /// <summary>一覧（マスタと JOIN 済みの表示値を含む）</summary>
        public DataTable Orders { get; set; }

        #region ドロップダウン用のマスタ

        /// <summary>Customers（CustomerID / CompanyName）</summary>
        public DataTable Customers { get; set; }

        /// <summary>Employees（EmployeeID / EmployeeName）</summary>
        public DataTable Employees { get; set; }

        /// <summary>Shippers（ShipperID / CompanyName）</summary>
        public DataTable Shippers { get; set; }

        #endregion

        #region 詳細・更新（画面Ｂ＝OrdDetailedView）

        /// <summary>対象の OrderID（空＝追加モード）</summary>
        public string OrderID { get; set; }

        /// <summary>追加（Ｃ）モードか</summary>
        public bool IsNew { get; set; }

        /// <summary>ＣＵＤが済んで、この画面での操作を止めたか</summary>
        public bool CudDone { get; set; }

        /// <summary>CustomerID（入力値）</summary>
        public string DetailCustomerID { get; set; }

        /// <summary>EmployeeID（入力値）</summary>
        public string DetailEmployeeID { get; set; }

        /// <summary>ShipVia（入力値）</summary>
        public string DetailShipVia { get; set; }

        /// <summary>OrderDate（入力値）</summary>
        public string OrderDate { get; set; }

        /// <summary>RequiredDate（入力値）</summary>
        public string RequiredDate { get; set; }

        /// <summary>ShippedDate（入力値）</summary>
        public string ShippedDate { get; set; }

        /// <summary>Freight（入力値）</summary>
        public string Freight { get; set; }

        /// <summary>ShipName（入力値）</summary>
        public string ShipName { get; set; }

        /// <summary>ShipAddress（入力値）</summary>
        public string ShipAddress { get; set; }

        /// <summary>ShipCity（入力値）</summary>
        public string ShipCity { get; set; }

        /// <summary>ShipRegion（入力値）</summary>
        public string ShipRegion { get; set; }

        /// <summary>ShipPostalCode（入力値）</summary>
        public string ShipPostalCode { get; set; }

        /// <summary>ShipCountry（入力値。画面Ａの検索条件とは別項目）</summary>
        public string DetailShipCountry { get; set; }

        #endregion

        #region 明細（Order Details）

        /// <summary>明細の表示元（RowState を保持した DataTable）</summary>
        public DataTable OrderDetails { get; set; }

        /// <summary>Products（ProductID / ProductName）＝明細の ＤＤＬ 用</summary>
        public DataTable Products { get; set; }

        /// <summary>ポストバックで戻ってくる明細（モデルバインド先）</summary>
        public List<OrdDetailRowViewModel> DetailRows { get; set; }

        #endregion

        /// <summary>コンストラクタ</summary>
        public OrdViewModel()
        {
            this.Message = "";
            this.PageIndex = 1;
            this.PageSize = 20;
            this.DetailRows = new List<OrdDetailRowViewModel>();
        }
    }

    /// <summary>明細（Order Details）1行分の ViewModel</summary>
    /// <remarks>
    /// ★ DataTable 側は型付き（int / decimal / short / float）だが、画面から戻る値は文字列。
    ///   DataRow へ書き戻すときに列の型へ変換する（変換はコントローラ側の SetIfChanged）。
    /// </remarks>
    public class OrdDetailRowViewModel
    {
        /// <summary>DataTable の行インデックス（Deleted 行は描画しないので表示連番とはズレる）</summary>
        public int RowIndex { get; set; }

        /// <summary>ProductID</summary>
        public string ProductID { get; set; }

        /// <summary>UnitPrice</summary>
        public string UnitPrice { get; set; }

        /// <summary>Quantity</summary>
        public string Quantity { get; set; }

        /// <summary>Discount</summary>
        public string Discount { get; set; }
    }
}
