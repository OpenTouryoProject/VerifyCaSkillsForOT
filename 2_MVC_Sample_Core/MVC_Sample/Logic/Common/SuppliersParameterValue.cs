//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守（引数）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersParameterValue
//* クラス日本語名  ：Suppliers 保守用の引数クラス
//*
//* 作成日時        ：2026/08/27
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/27  生技              新規作成
//**********************************************************************************

using System.Data;

using Touryo.Infrastructure.Business.Common;
using Touryo.Infrastructure.Business.Util;

namespace MVC_Sample.Logic.Common
{
    /// <summary>Suppliers 保守用の引数クラス</summary>
    public class SuppliersParameterValue : MyParameterValue
    {
        /// <summary>
        /// バッチ更新の対象（RowState を保持した DataTable）
        /// </summary>
        /// <remarks>
        /// 追加＝Added／更新＝Modified／削除＝Deleted を、この DataTable が覚えている。
        /// Ｂ層はこの RowState を見て INSERT / UPDATE / DELETE を振り分ける。
        /// </remarks>
        public DataTable Suppliers;

        #region コンストラクタ

        /// <summary>コンストラクタ</summary>
        public SuppliersParameterValue(string screenId, string controlId, string methodName, string actionType, MyUserInfo user)
            : base(screenId, controlId, methodName, actionType, user)
        {
            // Baseのコンストラクタに引数を渡すために必要。
        }

        #endregion
    }
}
