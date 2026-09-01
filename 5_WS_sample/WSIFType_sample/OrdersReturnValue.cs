//**********************************************************************************
//* トランザクション・テーブル（Orders）保守（戻り値）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersReturnValue
//* クラス日本語名  ：Orders 保守用の戻り値クラス
//*
//* 作成日時        ：2026/08/28
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/28  生技              新規作成
//**********************************************************************************

using System.Data;

using Touryo.Infrastructure.Business.Common;

namespace WSIFType_sample
{
    /// <summary>Orders 保守用の戻り値クラス</summary>
    /// <remarks>シリアライズ可能にする（WS対応）</remarks>
    [System.Serializable()]
    public class OrdersReturnValue : MyReturnValue
    {
        /// <summary>汎用エリア（件数確認の結果など）</summary>
        public object Obj;

        /// <summary>一覧（条件検索の結果＝現在ページ分）</summary>
        public DataTable Orders;

        /// <summary>条件に一致する総件数（ページャの総ページ数計算に使う）</summary>
        public int TotalCount;

        #region ドロップダウン用のマスタ（表示変換にも使う）

        /// <summary>Customers（CustomerID / CompanyName）</summary>
        public DataTable Customers;

        /// <summary>Employees（EmployeeID / EmployeeName）</summary>
        public DataTable Employees;

        /// <summary>Shippers（ShipperID / CompanyName）</summary>
        public DataTable Shippers;

        #endregion

        /// <summary>バッチ更新：INSERT した件数</summary>
        public int InsertCount;

        /// <summary>バッチ更新：UPDATE した件数</summary>
        public int UpdateCount;

        /// <summary>バッチ更新：DELETE した件数</summary>
        public int DeleteCount;
    }
}
