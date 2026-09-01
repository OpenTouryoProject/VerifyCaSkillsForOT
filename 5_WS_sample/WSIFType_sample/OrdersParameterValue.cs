//**********************************************************************************
//* トランザクション・テーブル（Orders）保守（引数）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdersParameterValue
//* クラス日本語名  ：Orders 保守用の引数クラス
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
using Touryo.Infrastructure.Business.Util;

namespace WSIFType_sample
{
    /// <summary>Orders 保守用の引数クラス</summary>
    /// <remarks>シリアライズ可能にする（WS対応）</remarks>
    [System.Serializable()]
    public class OrdersParameterValue : MyParameterValue
    {
        #region 検索条件

        /// <summary>検索条件：CustomerID（未指定なら条件から外す）</summary>
        public string CustomerID;

        /// <summary>検索条件：EmployeeID（未指定なら条件から外す）</summary>
        public string EmployeeID;

        /// <summary>検索条件：ShipVia（未指定なら条件から外す）</summary>
        public string ShipVia;

        /// <summary>検索条件：ShipCountry（前方一致。未指定なら条件から外す）</summary>
        public string ShipCountry;

        #endregion

        #region ページング

        /// <summary>ページ番号（1 起算）</summary>
        public int PageIndex = 1;

        /// <summary>1ページの表示件数</summary>
        public int PageSize = 20;

        #endregion

        /// <summary>
        /// バッチ更新の対象（RowState を保持した DataTable）
        /// </summary>
        /// <remarks>
        /// 追加＝Added／更新＝Modified／削除＝Deleted を、この DataTable が覚えている。
        /// Ｂ層はこの RowState を見て INSERT / UPDATE / DELETE を振り分ける。
        /// </remarks>
        public DataTable Orders;

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public OrdersParameterValue(string screenId, string controlId, string methodName, string actionType, MyUserInfo user)
            : base(screenId, controlId, methodName, actionType, user)
        {
            // Baseのコンストラクタに引数を渡すために必要。
        }

        #endregion
    }
}
