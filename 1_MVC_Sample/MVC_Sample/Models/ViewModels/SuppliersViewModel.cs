//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守（Ｐ層：ViewModel）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersViewModel
//* クラス日本語名  ：Suppliers 保守画面の ViewModel
//*
//* 作成日時        ：2026/08/27
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/27  生技              新規作成
//**********************************************************************************

using System.Collections.Generic;
using System.Data;

namespace MVC_Sample.Models.ViewModels
{
    /// <summary>Suppliers 保守画面の ViewModel</summary>
    public class SuppliersViewModel : BaseViewModel
    {
        /// <summary>画面に表示するメッセージ（JavaScript のダイアログで表示する）</summary>
        public string Message { get; set; }

        /// <summary>一覧の表示元（RowState を保持した DataTable）</summary>
        public DataTable Suppliers { get; set; }

        /// <summary>ポストバックで戻ってくる明細（モデルバインド先）</summary>
        public List<SupplierRowViewModel> Rows { get; set; }

        /// <summary>コンストラクタ</summary>
        public SuppliersViewModel()
        {
            this.Message = "";
            this.Rows = new List<SupplierRowViewModel>();
        }
    }

    /// <summary>Suppliers 一覧の1行分の ViewModel</summary>
    public class SupplierRowViewModel
    {
        /// <summary>
        /// DataTable の行インデックス
        /// </summary>
        /// <remarks>
        /// ★ Deleted 行は描画しないので、表示上の連番とはズレる。
        ///   画面から DataRow を引くときは、この値を使う（表示連番を使わない）。
        /// </remarks>
        public int RowIndex { get; set; }

        /// <summary>CompanyName（DB 側 NOT NULL）</summary>
        public string CompanyName { get; set; }

        /// <summary>ContactName</summary>
        public string ContactName { get; set; }

        /// <summary>ContactTitle</summary>
        public string ContactTitle { get; set; }

        /// <summary>Address</summary>
        public string Address { get; set; }

        /// <summary>City</summary>
        public string City { get; set; }

        /// <summary>Region</summary>
        public string Region { get; set; }

        /// <summary>PostalCode</summary>
        public string PostalCode { get; set; }

        /// <summary>Country</summary>
        public string Country { get; set; }

        /// <summary>Phone</summary>
        public string Phone { get; set; }

        /// <summary>Fax</summary>
        public string Fax { get; set; }

        /// <summary>HomePage</summary>
        public string HomePage { get; set; }
    }
}
