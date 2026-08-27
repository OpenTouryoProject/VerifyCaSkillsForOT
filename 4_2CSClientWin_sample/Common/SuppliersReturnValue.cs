//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守（戻り値）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：SuppliersReturnValue
//* クラス日本語名  ：Suppliers 保守用の戻り値クラス
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

namespace _2CSClientWin_sample.Common
{
    /// <summary>Suppliers 保守用の戻り値クラス</summary>
    public class SuppliersReturnValue : MyReturnValue
    {
        /// <summary>汎用エリア（件数確認の結果など）</summary>
        public object Obj;

        /// <summary>一覧（参照結果）</summary>
        public DataTable Suppliers;

        /// <summary>バッチ更新：INSERT した件数</summary>
        public int InsertCount;

        /// <summary>バッチ更新：UPDATE した件数</summary>
        public int UpdateCount;

        /// <summary>バッチ更新：DELETE した件数</summary>
        public int DeleteCount;
    }
}
