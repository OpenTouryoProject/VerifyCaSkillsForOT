//**********************************************************************************
//* 受注管理（Ord）：条件検索一覧・詳細・更新（引数）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：OrdParameterValue
//* クラス日本語名  ：受注管理（Ord）用の引数クラス
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
using Touryo.Infrastructure.Business.Util;

namespace WSIFType_sample
{
    /// <summary>受注管理（Ord）用の引数クラス</summary>
    /// <remarks>シリアライズ可能にする（WS対応）</remarks>
    [System.Serializable()]
    public class OrdParameterValue : MyParameterValue
    {
        #region 検索条件（画面Ａ＝OrdListSearch）

        /// <summary>検索条件：CustomerID（未指定なら条件から外す）</summary>
        public string CustomerID;

        /// <summary>検索条件：EmployeeID（未指定なら条件から外す）</summary>
        public string EmployeeID;

        /// <summary>検索条件：ShipVia（未指定なら条件から外す）</summary>
        public string ShipVia;

        /// <summary>検索条件：ShipCountry（前方一致。未指定なら条件から外す）</summary>
        public string ShipCountry;

        #endregion

        #region ページング（画面Ａ）

        /// <summary>ページ番号（1 起算）</summary>
        public int PageIndex = 1;

        /// <summary>1ページの表示件数</summary>
        public int PageSize = 20;

        #endregion

        #region 詳細・更新（画面Ｂ＝OrdDetailedView）

        /// <summary>詳細表示（参照＝R）の対象となる OrderID（空＝新規モード＝スキーマだけ返す）</summary>
        public string OrderID;

        /// <summary>
        /// ＣＵＤの対象（1行だけの DataTable）
        /// </summary>
        /// <remarks>
        /// ★ 楽観排他のため「取得時の値」が要る。DataTable で運べば
        ///   DataRowVersion.Original に取得時の値が残る（＝画面で文字列に潰さない）。
        ///   追加＝Added（Original 無し）／更新＝Modified／削除＝Unchanged or Modified。
        /// </remarks>
        public DataTable Order;

        /// <summary>
        /// ＣＵＤの対象となる明細（Order Details）
        /// </summary>
        /// <remarks>
        /// ★ 親（Orders）と違い RowState でバッチ更新する（追加＝Added／更新＝Modified／削除＝Deleted）。
        ///   楽観排他のため取得時の値（DataRowVersion.Original）を残したまま運ぶ。
        /// </remarks>
        public DataTable OrderDetails;

        #endregion

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        /// <param name="screenId">画面ID</param>
        /// <param name="controlId">コントロールID</param>
        /// <param name="methodName">メソッド名</param>
        /// <param name="actionType">アクション タイプ</param>
        /// <param name="user">ユーザ情報</param>
        public OrdParameterValue(string screenId, string controlId, string methodName, string actionType, MyUserInfo user)
            : base(screenId, controlId, methodName, actionType, user)
        {
            // Baseのコンストラクタに引数を渡すために必要。
        }

        #endregion
    }
}
