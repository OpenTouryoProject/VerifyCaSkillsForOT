//**********************************************************************************
//* 受注管理（Ord）：条件検索一覧・詳細・更新（戻り値）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdReturnValue
//* クラス日本語名  ：受注管理（Ord）用の戻り値クラス
//*
//* 作成日時        ：2026/09/02
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/09/02  生技              新規作成
//**********************************************************************************

using System.Data;

using Touryo.Infrastructure.Business.Common;

namespace MVC_Sample.Logic.Common
{
    /// <summary>受注管理（Ord）用の戻り値クラス</summary>
    public class OrdReturnValue : MyReturnValue
    {
        /// <summary>一覧（条件検索の結果＝現在ページ分。マスタと JOIN 済みの表示値を含む）</summary>
        public DataTable Orders;

        /// <summary>条件に一致する総件数（ページャの総ページ数計算に使う）</summary>
        public int TotalCount;

        /// <summary>詳細（自動生成Dao の参照＝R の結果。新規モードは 0 行＝スキーマだけ）</summary>
        public DataTable Order;

        #region ドロップダウン用のマスタ

        /// <summary>Customers（CustomerID / CompanyName）</summary>
        public DataTable Customers;

        /// <summary>Employees（EmployeeID / EmployeeName）</summary>
        public DataTable Employees;

        /// <summary>Shippers（ShipperID / CompanyName）</summary>
        public DataTable Shippers;

        #endregion

        #region ＣＵＤの結果件数

        /// <summary>INSERT した件数</summary>
        public int InsertCount;

        /// <summary>UPDATE した件数</summary>
        public int UpdateCount;

        /// <summary>DELETE した件数</summary>
        public int DeleteCount;

        #endregion
    }
}
