//**********************************************************************************
//* フレームワーク・テストクラス（Ｂ層）
//**********************************************************************************

// テスト用サンプルなので、必要に応じて流用 or 削除して下さい。

//**********************************************************************************
//* クラス名        ：LayerB
//* クラス日本語名  ：Ｂ層のテスト
//*
//* 作成日時        ：－
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  20xx/xx/xx  ＸＸ ＸＸ         ＸＸＸＸ
//**********************************************************************************

using _2CSClientWin_sample.Common;
using _2CSClientWin_sample.Dao;

using System;
using System.Data;

using Touryo.Infrastructure.Business.RichClient.Business;
using Touryo.Infrastructure.Business.Dao;
using Touryo.Infrastructure.Framework.Exceptions;

namespace _2CSClientWin_sample.Business
{
    /// <summary>
    /// LayerB の概要の説明です
    /// </summary>
    public class LayerB : MyFcBaseLogic2CS
    {
        #region テンプレ

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_メソッド名(TestParameterValue testParameter)
        { //メソッド引数にBaseParameterValueの派生の型を定義可能。

            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------

            // 個別Dao
            LayerD myDao = new LayerD(this.GetDam());
            //myDao.xxxx(testParameter, ref testReturn);

            // 共通Dao
            CmnDao cmnDao = new CmnDao(this.GetDam());
            cmnDao.ExecSelectScalar();

            // ↑業務処理-----------------------------------------------------
        }

        #endregion

        #region UOCメソッド

        #region SelectCount

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_SelectCount(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLFileName = "ShipperCount.sql";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLFileName = "ShipperCount.xml";
                            break;
                    }

                    // 共通Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = cmnDao.ExecSelectScalar();

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // 共通Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = genDao.D5_SelCnt();

                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.SelectCount(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region SelectAll_DT

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_SelectAll_DT(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------
            DataTable dt = null;

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLText = "SELECT * FROM Shippers";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><ROOT>SELECT * FROM Shippers</ROOT>";
                            break;
                    }

                    // 戻り値 dt
                    dt = new DataTable();

                    // 共通Daoを実行
                    cmnDao.ExecSelectFill_DT(dt);

                    // 戻り値を設定
                    testReturn.Obj = dt;

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // 戻り値 dt
                    dt = new DataTable();

                    // 自動生成Daoを実行
                    genDao.D2_Select(dt);

                    // 戻り値を設定
                    testReturn.Obj = (DataTable)dt;
                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.SelectAll_DT(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region SelectAll_DS

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_SelectAll_DS(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------
            DataSet ds = null;

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLText = "SELECT * FROM Shippers";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><ROOT>SELECT * FROM Shippers</ROOT>";
                            break;
                    }

                    // 戻り値 ds
                    ds = new DataSet();

                    // 共通Daoを実行
                    cmnDao.ExecSelectFill_DS(ds);

                    // 戻り値を設定
                    testReturn.Obj = ds;

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // 戻り値 ds
                    ds = new DataSet();
                    ds.Tables.Add(new DataTable());

                    // 自動生成Daoを実行
                    genDao.D2_Select(ds.Tables[0]);

                    // 戻り値を設定
                    testReturn.Obj = ds;
                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.SelectAll_DS(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region SelectAll_DR

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_SelectAll_DR(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------
            DataTable dt = null;

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLText = "SELECT * FROM Shippers";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><ROOT>SELECT * FROM Shippers</ROOT>";
                            break;
                    }

                    // 戻り値 dt
                    dt = new DataTable();

                    // ３列生成
                    dt.Columns.Add("c1", typeof(string));
                    dt.Columns.Add("c2", typeof(string));
                    dt.Columns.Add("c3", typeof(string));

                    // 共通Daoを実行
                    IDataReader idr = cmnDao.ExecSelect_DR();

                    while (idr.Read())
                    {
                        // DRから読む
                        object[] objArray = new object[3];
                        idr.GetValues(objArray);

                        // DTに設定する。
                        DataRow dr = dt.NewRow();
                        dr.ItemArray = objArray;
                        dt.Rows.Add(dr);
                    }

                    // 終了したらクローズ
                    idr.Close();

                    // 戻り値を設定
                    testReturn.Obj = dt;

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // DRのI/Fなし

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // 戻り値 dt
                    dt = new DataTable();

                    // 自動生成Daoを実行
                    genDao.D2_Select(dt);

                    // 戻り値を設定
                    testReturn.Obj = (DataTable)dt;

                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.SelectAll_DR(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region SelectAll_DSQL

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_SelectAll_DSQL(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLFileName = "ShipperSelectOrder.sql";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLFileName = "ShipperSelectOrder.xml";
                            break;
                    }

                    // ユーザ定義パラメタに対して、動的に値を設定する。
                    string orderColumn = "";
                    string orderSequence = "";

                    if (testParameter.OrderColumn == "c1")
                    {
                        orderColumn = "ShipperID";
                    }
                    else if (testParameter.OrderColumn == "c2")
                    {
                        orderColumn = "CompanyName";
                    }
                    else if (testParameter.OrderColumn == "c3")
                    {
                        orderColumn = "Phone";
                    }
                    else { }

                    if (testParameter.OrderSequence == "A")
                    {
                        orderSequence = "ASC";
                    }
                    else if (testParameter.OrderSequence == "D")
                    {
                        orderSequence = "DESC";
                    }
                    else { }

                    // パラメタ ライズド クエリのパラメタに対して、動的に値を設定する。
                    cmnDao.SetParameter("P1", "test");

                    // ユーザ入力は指定しない。
                    // ※ 動的SQLのVALタグは、前後の空白をつめることが有るので、
                    //    必要であれば、前後の空白を明示的に指定する必要がある。
                    cmnDao.SetUserParameter("COLUMN", " " + orderColumn + " ");
                    cmnDao.SetUserParameter("SEQUENCE", " " + orderSequence + " ");

                    // 戻り値 dt
                    DataTable dt = new DataTable();

                    // 共通Daoを実行
                    cmnDao.ExecSelectFill_DT(dt);

                    // 自動生成Daoを実行
                    testReturn.Obj = dt;

                    break;

                //case "generate": // 自動生成Daoを使用する。
                //    // 当該SQLなし
                //    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.SelectAll_DSQL(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region Select

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_Select(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------
            DataTable dt = null;

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLFileName = "ShipperSelect.sql";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLFileName = "ShipperSelect.xml";
                            break;
                    }

                    // パラメタ ライズド クエリのパラメタに対して、動的に値を設定する。
                    cmnDao.SetParameter("P1", testParameter.ShipperID);

                    // 戻り値 dt
                    dt = new DataTable();

                    // 共通Daoを実行
                    cmnDao.ExecSelectFill_DT(dt);

                    // キャストの対策コードを挿入

                    // ・SQLの場合、ShipperIDのintがInt32型にマップされる。
                    // ・ODPの場合、ShipperIDのNUMBERがInt64型にマップされる。
                    // ・DB2の場合、ShipperIDのDECIMALがｘｘｘ型にマップされる。
                    if (dt.Rows[0].ItemArray.GetValue(0).GetType().ToString() == "System.Int32")
                    {
                        // Int32なのでキャスト
                        testReturn.ShipperID = (int)dt.Rows[0].ItemArray.GetValue(0);
                    }
                    else
                    {
                        // それ以外の場合、一度、文字列に変換してInt32.Parseする。
                        testReturn.ShipperID = int.Parse(dt.Rows[0].ItemArray.GetValue(0).ToString());
                    }

                    testReturn.CompanyName = (string)dt.Rows[0].ItemArray.GetValue(1);
                    testReturn.Phone = (string)dt.Rows[0].ItemArray.GetValue(2);

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // パラメタに対して、動的に値を設定する。
                    genDao.PK_ShipperID = testParameter.ShipperID;

                    // 戻り値 dt
                    dt = new DataTable();

                    // 自動生成Daoを実行
                    genDao.S2_Select(dt);

                    // キャストの対策コードを挿入

                    // ・SQLの場合、ShipperIDのintがInt32型にマップされる。
                    // ・ODPの場合、ShipperIDのNUMBERがInt64型にマップされる。
                    // ・DB2の場合、ShipperIDのDECIMALがｘｘｘ型にマップされる。
                    if (dt.Rows[0].ItemArray.GetValue(0).GetType().ToString() == "System.Int32")
                    {
                        // Int32なのでキャスト
                        testReturn.ShipperID = (int)dt.Rows[0].ItemArray.GetValue(0);
                    }
                    else
                    {
                        // それ以外の場合、一度、文字列に変換してInt32.Parseする。
                        testReturn.ShipperID = int.Parse(dt.Rows[0].ItemArray.GetValue(0).ToString());
                    }

                    testReturn.CompanyName = (string)dt.Rows[0].ItemArray.GetValue(1);
                    testReturn.Phone = (string)dt.Rows[0].ItemArray.GetValue(2);

                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.Select(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region Insert

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_Insert(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    cmnDao.SQLFileName = "ShipperInsert.sql";

                    // パラメタ ライズド クエリのパラメタに対して、動的に値を設定する。
                    cmnDao.SetParameter("P2", testParameter.CompanyName);
                    cmnDao.SetParameter("P3", testParameter.Phone);

                    // 共通Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = cmnDao.ExecInsUpDel_NonQuery();

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // パラメタに対して、動的に値を設定する。
                    genDao.CompanyName = testParameter.CompanyName;
                    genDao.Phone = testParameter.Phone;

                    // 自動生成Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = genDao.D1_Insert();

                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.Insert(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region Update

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_Update(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLFileName = "ShipperUpdate.sql";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLFileName = "ShipperUpdate.xml";
                            break;
                    }

                    // パラメタ ライズド クエリのパラメタに対して、動的に値を設定する。
                    cmnDao.SetParameter("P1", testParameter.ShipperID);
                    cmnDao.SetParameter("P2", testParameter.CompanyName);
                    cmnDao.SetParameter("P3", testParameter.Phone);

                    // 共通Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = cmnDao.ExecInsUpDel_NonQuery();

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // パラメタに対して、動的に値を設定する。
                    genDao.PK_ShipperID = testParameter.ShipperID;
                    genDao.Set_CompanyName_forUPD = testParameter.CompanyName;
                    genDao.Set_Phone_forUPD = testParameter.Phone;

                    // 自動生成Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = genDao.S3_Update();

                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.Update(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #region Delete

        /// <summary>業務処理を実装</summary>
        /// <param name="testParameter">引数クラス</param>
        private void UOC_Delete(TestParameterValue testParameter)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            TestReturnValue testReturn = new TestReturnValue();
            this.ReturnValue = testReturn;

            // ↓業務処理-----------------------------------------------------

            switch ((testParameter.ActionType.Split('%'))[1])
            {
                case "common": // 共通Daoを使用する。

                    // 共通Daoを生成
                    CmnDao cmnDao = new CmnDao(this.GetDam());

                    switch ((testParameter.ActionType.Split('%'))[2])
                    {
                        case "static":
                            // 静的SQLを指定
                            cmnDao.SQLFileName = "ShipperDelete.sql";
                            break;

                        case "dynamic":
                            // 動的SQLを指定
                            cmnDao.SQLFileName = "ShipperDelete.xml";
                            break;
                    }

                    // パラメタ ライズド クエリのパラメタに対して、動的に値を設定する。
                    cmnDao.SetParameter("P1", testParameter.ShipperID);

                    // 共通Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = cmnDao.ExecInsUpDel_NonQuery();

                    break;

                case "generate": // 自動生成Daoを使用する。

                    // 自動生成Daoを生成
                    DaoShippers genDao = new DaoShippers(this.GetDam());

                    // パラメタに対して、動的に値を設定する。
                    genDao.PK_ShipperID = testParameter.ShipperID;

                    // 自動生成Daoを実行
                    // 戻り値を設定
                    testReturn.Obj = genDao.S4_Delete();

                    break;

                default: // 個別Daoを使用する。
                    LayerD myDao = new LayerD(this.GetDam());
                    myDao.Delete(testParameter, testReturn);
                    break;
            }

            // ↑業務処理-----------------------------------------------------

            // ロールバックのテスト
            this.TestRollback(testParameter);
        }

        #endregion

        #endregion

        #region ロールバックのテスト

        /// <summary>ロールバックのテスト</summary>
        /// <param name="testParameter">引数クラス</param>
        private void TestRollback(TestParameterValue testParameter)
        {
            switch ((testParameter.ActionType.Split('%'))[3])
            {

                case "Business":

                    // 戻り値が見えるか確認する。
                    ((TestReturnValue)this.ReturnValue).Obj = "戻り値が戻るか？";

                    // 業務例外のスロー
                    throw new BusinessApplicationException(
                        "ロールバックのテスト",
                        "ロールバックのテスト",
                        "エラー情報");
                //break; // 到達できないためコメントアウト

                case "System":

                    // 戻り値が見えるか確認する。
                    ((TestReturnValue)this.ReturnValue).Obj = "戻り値が戻るか？";

                    // システム例外のスロー
                    throw new BusinessSystemException(
                        "ロールバックのテスト",
                        "ロールバックのテスト");
                //break; // 到達できないためコメントアウト

                case "Other":

                    // 戻り値が見えるか確認する。
                    ((TestReturnValue)this.ReturnValue).Obj = "戻り値が戻るか？";

                    // その他、一般的な例外のスロー
                    throw new Exception("ロールバックのテスト");
                //break; // 到達できないためコメントアウト

                case "Other-Business":
                    // 戻り値が見えるか確認する。
                    ((TestReturnValue)this.ReturnValue).Obj = "戻り値が戻るか？";

                    // その他、一般的な例外（業務例外へ振り替え）のスロー
                    throw new Exception("Other-Business");
                //break; // 到達できないためコメントアウト

                case "Other-System":

                    // 戻り値が見えるか確認する。
                    ((TestReturnValue)this.ReturnValue).Obj = "戻り値が戻るか？";

                    // その他、一般的な例外（システム例外へ振り替え）のスロー
                    throw new Exception("Other-System");
                //break; // 到達できないためコメントアウト
            }
        }

        #endregion

        #region マスタ・テーブル（Suppliers）保守

        #region 件数確認（共通Dao）

        /// <summary>Suppliers のデータ件数を確認する（共通Dao 経由）</summary>
        /// <param name="parameterValue">引数クラス</param>
        private void UOC_SuppliersSelectCount(SuppliersParameterValue parameterValue)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            SuppliersReturnValue returnValue = new SuppliersReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------

            // 共通Daoを生成（接続・トランザクションはＢ層が持つ＝GetDam() を渡す）
            CmnDao cmnDao = new CmnDao(this.GetDam());

            // 件数確認の静的SQLを指定（%OT_RESOURCE_ROOT%\Sql\SupplierCount.sql）
            cmnDao.SQLFileName = "SupplierCount.sql";

            // 共通Daoを実行して、件数を戻り値に設定
            returnValue.Obj = cmnDao.ExecSelectScalar();

            // ↑業務処理-----------------------------------------------------
        }

        #endregion

        #region 一覧取得（自動生成Dao の参照）

        /// <summary>Suppliers の一覧を取得する（自動生成Dao の D2_Select）</summary>
        /// <param name="parameterValue">引数クラス</param>
        private void UOC_SuppliersSelectAll(SuppliersParameterValue parameterValue)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            SuppliersReturnValue returnValue = new SuppliersReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------

            DataTable dt = new DataTable("Suppliers");

            // 自動生成Daoを生成
            DaoSuppliers dao = new DaoSuppliers(this.GetDam());

            // 検索条件を設定しない＝動的SQLの WHERE 句が消えて全件になる。
            dao.D2_Select(dt);

            returnValue.Suppliers = dt;

            // ↑業務処理-----------------------------------------------------
        }

        #endregion

        #region バッチ更新（RowState で CUD を振り分け）

        /// <summary>Suppliers をバッチ更新する（自動生成Dao の D1/D3/D4）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// DataTable の RowState（Added / Modified / Deleted）で INSERT / UPDATE / DELETE を振り分ける。
        /// トランザクション境界はＢ層＝この UOC メソッド全体が1トランザクション。
        /// </remarks>
        private void UOC_SuppliersBatchUpdate(SuppliersParameterValue parameterValue)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            SuppliersReturnValue returnValue = new SuppliersReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------

            DataTable dt = parameterValue.Suppliers;
            if (dt == null) { return; }

            DaoSuppliers dao = new DaoSuppliers(this.GetDam());

            // ★ 削除 → 追加 の順で流す（同じキーを使い回したときに旧行と衝突しないため）。
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState != DataRowState.Deleted) { continue; }

                dao.ClearParametersFromHt();

                // ★ 削除行は DataRowVersion.Original しか読めない。
                dao.PK_SupplierID = dr["SupplierID", DataRowVersion.Original];

                // 楽観排他：取得時の値を WHERE に入れ、他者が更新済みなら 0 件になるようにする。
                LayerB.SetOriginalToWhere(dao, dr);

                int deleted = dao.D4_Delete();
                if (deleted == 0)
                {
                    // 更新件数0＝他者が先に更新/削除した（楽観排他の失敗）。
                    throw new BusinessApplicationException(
                        "SuppliersBatchUpdate", "他のユーザによって更新されています。",
                        "SupplierID = " + dr["SupplierID", DataRowVersion.Original]);
                }
                returnValue.DeleteCount += deleted;
            }

            foreach (DataRow dr in dt.Rows)
            {
                switch (dr.RowState)
                {
                    case DataRowState.Added:

                        dao.ClearParametersFromHt();

                        // ★ SupplierID は IDENTITY（自動採番）＝INSERT に含めない。
                        //   含めると "IDENTITY_INSERT が OFF..." で必ず失敗するため、
                        //   全列必須の S1_Insert ではなく、設定した列だけを INSERT する D1_Insert を使う。
                        dao.CompanyName  = LayerB.ForInsert(dr, "CompanyName");
                        dao.ContactName  = LayerB.ForInsert(dr, "ContactName");
                        dao.ContactTitle = LayerB.ForInsert(dr, "ContactTitle");
                        dao.Address      = LayerB.ForInsert(dr, "Address");
                        dao.City         = LayerB.ForInsert(dr, "City");
                        dao.Region       = LayerB.ForInsert(dr, "Region");
                        dao.PostalCode   = LayerB.ForInsert(dr, "PostalCode");
                        dao.Country      = LayerB.ForInsert(dr, "Country");
                        dao.Phone        = LayerB.ForInsert(dr, "Phone");
                        dao.Fax          = LayerB.ForInsert(dr, "Fax");

                        // ★ HomePage（ntext）は「WHERE に入れられない」だけで、
                        //   INSERT の値／UPDATE の SET には普通に渡せる（比較しないため）。
                        dao.HomePage     = LayerB.ForInsert(dr, "HomePage");

                        returnValue.InsertCount += dao.D1_Insert();
                        break;

                    case DataRowState.Modified:

                        dao.ClearParametersFromHt();

                        // WHERE 用：主キー＋取得時の値（楽観排他）
                        dao.PK_SupplierID = dr["SupplierID", DataRowVersion.Original];
                        LayerB.SetOriginalToWhere(dao, dr);

                        // SET 用：現在値（空欄は DBNull＝NULL に落とす。WHERE 側とは役割が逆）
                        dao.Set_CompanyName_forUPD  = LayerB.ForUpdate(dr, "CompanyName");
                        dao.Set_ContactName_forUPD  = LayerB.ForUpdate(dr, "ContactName");
                        dao.Set_ContactTitle_forUPD = LayerB.ForUpdate(dr, "ContactTitle");
                        dao.Set_Address_forUPD      = LayerB.ForUpdate(dr, "Address");
                        dao.Set_City_forUPD         = LayerB.ForUpdate(dr, "City");
                        dao.Set_Region_forUPD       = LayerB.ForUpdate(dr, "Region");
                        dao.Set_PostalCode_forUPD   = LayerB.ForUpdate(dr, "PostalCode");
                        dao.Set_Country_forUPD      = LayerB.ForUpdate(dr, "Country");
                        dao.Set_Phone_forUPD        = LayerB.ForUpdate(dr, "Phone");
                        dao.Set_Fax_forUPD          = LayerB.ForUpdate(dr, "Fax");
                        dao.Set_HomePage_forUPD     = LayerB.ForUpdate(dr, "HomePage");

                        int updated = dao.D3_Update();
                        if (updated == 0)
                        {
                            throw new BusinessApplicationException(
                                "SuppliersBatchUpdate", "他のユーザによって更新されています。",
                                "SupplierID = " + dr["SupplierID", DataRowVersion.Original]);
                        }
                        returnValue.UpdateCount += updated;
                        break;

                    default:
                        // Unchanged / Deleted（Deleted は上のループで処理済み）は対象外
                        break;
                }
            }

            // ↑業務処理-----------------------------------------------------
        }

        /// <summary>楽観排他：取得時の値（Original）を WHERE 用パラメタに設定する</summary>
        /// <param name="dao">自動生成Dao</param>
        /// <param name="dr">対象の DataRow</param>
        /// <remarks>
        /// ★ HomePage（ntext）は WHERE に入れない。SQL Server は ntext を「=」で比較できず
        ///   「ntext と nvarchar は equal to 演算子では互換性がありません」で落ちるため。
        ///   動的SQL（D3/D4）はパラメタを設定しなければ、その列の &lt;IF&gt; ごと WHERE から消える。
        /// ★ Original が DBNull の列は null に読み替えて渡す（&lt;ELSE&gt; の「IS NULL」を出させる）。
        ///   DBNull のまま渡すと「= @col（NULL）」になり、決して一致しない。
        /// </remarks>
        private static void SetOriginalToWhere(DaoSuppliers dao, DataRow dr)
        {
            dao.CompanyName  = LayerB.ForWhere(dr, "CompanyName");
            dao.ContactName  = LayerB.ForWhere(dr, "ContactName");
            dao.ContactTitle = LayerB.ForWhere(dr, "ContactTitle");
            dao.Address      = LayerB.ForWhere(dr, "Address");
            dao.City         = LayerB.ForWhere(dr, "City");
            dao.Region       = LayerB.ForWhere(dr, "Region");
            dao.PostalCode   = LayerB.ForWhere(dr, "PostalCode");
            dao.Country      = LayerB.ForWhere(dr, "Country");
            dao.Phone        = LayerB.ForWhere(dr, "Phone");
            dao.Fax          = LayerB.ForWhere(dr, "Fax");
            // HomePage は設定しない（上記のとおり ntext のため）
        }

        /// <summary>WHERE 用の値（DBNull は null に読み替える）</summary>
        private static object ForWhere(DataRow dr, string columnName)
        {
            object value = dr[columnName, DataRowVersion.Original];
            return (value == DBNull.Value) ? null : value;
        }

        /// <summary>SET 用の値（空欄は DBNull＝NULL に落とす）</summary>
        private static object ForUpdate(DataRow dr, string columnName)
        {
            object value = dr[columnName];
            if (value == DBNull.Value) { return DBNull.Value; }
            return (value.ToString().Length == 0) ? (object)DBNull.Value : value;
        }

        /// <summary>INSERT 用の値（CompanyName は DB 側 NOT NULL＝空文字で埋める）</summary>
        private static object ForInsert(DataRow dr, string columnName)
        {
            object value = dr[columnName];
            bool isBlank = (value == DBNull.Value) || (value.ToString().Length == 0);

            // ★ DB 側 NOT NULL の列に DBNull を渡すと INSERT が SqlException 515 になる。
            //   ExecSelectFill_DT は制約を取り込まないので DataTable からは判定できない＝アプリが知っておく。
            if (columnName == "CompanyName") { return isBlank ? (object)"" : value; }

            return isBlank ? (object)DBNull.Value : value;
        }

        #endregion

        #endregion

        #region トランザクション・テーブル（Orders）保守

        #region 件数確認（共通Dao）

        /// <summary>Orders のデータ件数を確認する（共通Dao 経由）</summary>
        /// <param name="parameterValue">引数クラス</param>
        private void UOC_OrdersSelectCount(OrdersParameterValue parameterValue)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            OrdersReturnValue returnValue = new OrdersReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------

            // 共通Daoを生成（接続・トランザクションはＢ層が持つ＝GetDam() を渡す）
            CmnDao cmnDao = new CmnDao(this.GetDam());

            // 件数確認の静的SQLを指定（%OT_RESOURCE_ROOT%\Sql\OrderCount.sql）
            cmnDao.SQLFileName = "OrderCount.sql";

            returnValue.Obj = cmnDao.ExecSelectScalar();

            // ↑業務処理-----------------------------------------------------
        }

        #endregion

        #region ドロップダウン用のマスタ取得（共通Dao）

        /// <summary>ドロップダウン用のマスタを取得する（共通Dao 経由）</summary>
        /// <param name="parameterValue">引数クラス</param>
        private void UOC_OrdersMasters(OrdersParameterValue parameterValue)
        {
            OrdersReturnValue returnValue = new OrdersReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------
            LayerB.LoadMasters(this.GetDam(), returnValue);
            // ↑業務処理-----------------------------------------------------
        }

        #endregion

        #region 条件検索（共通Dao ＋ 動的パラメタライズドクエリ）

        /// <summary>Orders を条件検索する（共通Dao ＋ 動的クエリ／ページング付き）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// ★ Ｄ層は共通Dao（CmnDao）を使用する（仕様）。
        /// ★ ページングは SQL 制御方式（ROW_NUMBER + CTE）。全件取得してメモリで切らない。
        /// ★ 検索条件は「パラメタを設定しない＝その &lt;IF&gt; ブロックごと WHERE から消える」で外す。
        ///   null を設定すると「IS NULL」になってしまい、条件を外したことにならない。
        /// </remarks>
        private void UOC_OrdersSearch(OrdersParameterValue parameterValue)
        {
            OrdersReturnValue returnValue = new OrdersReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------

            // --- 総件数（同じ条件で COUNT。ページャの総ページ数に使う） ---
            CmnDao countDao = new CmnDao(this.GetDam());
            countDao.SQLFileName = "OrderSearchCount.xml";
            LayerB.SetSearchCondition(countDao, parameterValue);
            returnValue.TotalCount = int.Parse(countDao.ExecSelectScalar().ToString());

            // --- 現在ページ分の明細 ---
            int pageSize = (parameterValue.PageSize <= 0) ? 20 : parameterValue.PageSize;
            int pageIndex = (parameterValue.PageIndex <= 0) ? 1 : parameterValue.PageIndex;

            CmnDao cmnDao = new CmnDao(this.GetDam());
            cmnDao.SQLFileName = "OrderSearch.xml";
            LayerB.SetSearchCondition(cmnDao, parameterValue);

            // RNUM の範囲（1 起算）
            cmnDao.SetParameter("FromRow", ((pageIndex - 1) * pageSize) + 1);
            cmnDao.SetParameter("ToRow", pageIndex * pageSize);

            DataTable dt = new DataTable("Orders");
            cmnDao.ExecSelectFill_DT(dt);
            returnValue.Orders = dt;

            // --- 仕様：一覧と同時にマスタ・テーブルのデータも取得する（DDL 化・表示変換用） ---
            LayerB.LoadMasters(this.GetDam(), returnValue);

            // ↑業務処理-----------------------------------------------------
        }

        /// <summary>検索条件を動的クエリのパラメタに設定する</summary>
        /// <param name="cmnDao">共通Dao</param>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// ★ 未指定の条件は「設定しない」＝&lt;IF&gt; ごと WHERE から消える。
        ///   ここで null や空文字を渡すと条件が残ってしまう。
        /// </remarks>
        private static void SetSearchCondition(CmnDao cmnDao, OrdersParameterValue parameterValue)
        {
            if (!string.IsNullOrEmpty(parameterValue.CustomerID))
            {
                cmnDao.SetParameter("CustomerID", parameterValue.CustomerID);
            }

            if (!string.IsNullOrEmpty(parameterValue.EmployeeID))
            {
                cmnDao.SetParameter("EmployeeID", int.Parse(parameterValue.EmployeeID));
            }

            if (!string.IsNullOrEmpty(parameterValue.ShipVia))
            {
                cmnDao.SetParameter("ShipVia", int.Parse(parameterValue.ShipVia));
            }

            if (!string.IsNullOrEmpty(parameterValue.ShipCountry))
            {
                // 前方一致（ユーザ入力は必ずパラメタで渡す＝ユーザパラメタにしない）
                cmnDao.SetParameter("ShipCountry", parameterValue.ShipCountry + "%");
            }
        }

        /// <summary>ドロップダウン用のマスタを取得する</summary>
        /// <param name="dam">Ｂ層が持つ Dam（BaseDam）</param>
        /// <param name="returnValue">戻り値クラス</param>
        private static void LoadMasters(Touryo.Infrastructure.Public.Db.BaseDam dam, OrdersReturnValue returnValue)
        {
            CmnDao dao = new CmnDao(dam);

            DataTable customers = new DataTable("Customers");
            dao.SQLFileName = "CustomerListForDdl.sql";
            dao.ExecSelectFill_DT(customers);
            returnValue.Customers = customers;

            DataTable employees = new DataTable("Employees");
            dao.SQLFileName = "EmployeeListForDdl.sql";
            dao.ExecSelectFill_DT(employees);
            returnValue.Employees = employees;

            DataTable shippers = new DataTable("Shippers");
            dao.SQLFileName = "ShipperListForDdl.sql";
            dao.ExecSelectFill_DT(shippers);
            returnValue.Shippers = shippers;
        }

        #endregion

        #region バッチ更新（RowState で CUD を振り分け）

        /// <summary>Orders をバッチ更新する（自動生成Dao の D1/D3/D4）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// DataTable の RowState（Added / Modified / Deleted）で INSERT / UPDATE / DELETE を振り分ける。
        /// トランザクション境界はＢ層＝この UOC メソッド全体が1トランザクション。
        /// </remarks>
        private void UOC_OrdersBatchUpdate(OrdersParameterValue parameterValue)
        {
            OrdersReturnValue returnValue = new OrdersReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------

            DataTable dt = parameterValue.Orders;
            if (dt == null) { return; }

            DaoOrders dao = new DaoOrders(this.GetDam());

            // ★ 削除 → 追加 の順で流す（同じキーを使い回したときに旧行と衝突しないため）。
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.RowState != DataRowState.Deleted) { continue; }

                dao.ClearParametersFromHt();

                // ★ 削除行は DataRowVersion.Original しか読めない。
                dao.PK_OrderID = dr["OrderID", DataRowVersion.Original];
                LayerB.SetOriginalToWhereForOrders(dao, dr);

                int deleted = dao.D4_Delete();
                if (deleted == 0)
                {
                    throw new BusinessApplicationException(
                        "OrdersBatchUpdate", "他のユーザによって更新されています。",
                        "OrderID = " + dr["OrderID", DataRowVersion.Original]);
                }
                returnValue.DeleteCount += deleted;
            }

            foreach (DataRow dr in dt.Rows)
            {
                switch (dr.RowState)
                {
                    case DataRowState.Added:

                        dao.ClearParametersFromHt();

                        // ★ OrderID は IDENTITY（自動採番）＝INSERT に含めない。
                        //   含めると "IDENTITY_INSERT が OFF..." で必ず失敗するため、
                        //   全列必須の S1_Insert ではなく、設定した列だけを INSERT する D1_Insert を使う。
                        dao.CustomerID     = LayerB.ForInsertOrders(dr, "CustomerID");
                        dao.EmployeeID     = LayerB.ForInsertOrders(dr, "EmployeeID");
                        dao.OrderDate      = LayerB.ForInsertOrders(dr, "OrderDate");
                        dao.RequiredDate   = LayerB.ForInsertOrders(dr, "RequiredDate");
                        dao.ShippedDate    = LayerB.ForInsertOrders(dr, "ShippedDate");
                        dao.ShipVia        = LayerB.ForInsertOrders(dr, "ShipVia");
                        dao.Freight        = LayerB.ForInsertOrders(dr, "Freight");
                        dao.ShipName       = LayerB.ForInsertOrders(dr, "ShipName");
                        dao.ShipAddress    = LayerB.ForInsertOrders(dr, "ShipAddress");
                        dao.ShipCity       = LayerB.ForInsertOrders(dr, "ShipCity");
                        dao.ShipRegion     = LayerB.ForInsertOrders(dr, "ShipRegion");
                        dao.ShipPostalCode = LayerB.ForInsertOrders(dr, "ShipPostalCode");
                        dao.ShipCountry    = LayerB.ForInsertOrders(dr, "ShipCountry");

                        returnValue.InsertCount += dao.D1_Insert();
                        break;

                    case DataRowState.Modified:

                        dao.ClearParametersFromHt();

                        // WHERE 用：主キー＋取得時の値（楽観排他）
                        dao.PK_OrderID = dr["OrderID", DataRowVersion.Original];
                        LayerB.SetOriginalToWhereForOrders(dao, dr);

                        // SET 用：現在値（空欄は DBNull＝NULL に落とす。WHERE 側とは役割が逆）
                        dao.Set_CustomerID_forUPD     = LayerB.ForUpdateOrders(dr, "CustomerID");
                        dao.Set_EmployeeID_forUPD     = LayerB.ForUpdateOrders(dr, "EmployeeID");
                        dao.Set_OrderDate_forUPD      = LayerB.ForUpdateOrders(dr, "OrderDate");
                        dao.Set_RequiredDate_forUPD   = LayerB.ForUpdateOrders(dr, "RequiredDate");
                        dao.Set_ShippedDate_forUPD    = LayerB.ForUpdateOrders(dr, "ShippedDate");
                        dao.Set_ShipVia_forUPD        = LayerB.ForUpdateOrders(dr, "ShipVia");
                        dao.Set_Freight_forUPD        = LayerB.ForUpdateOrders(dr, "Freight");
                        dao.Set_ShipName_forUPD       = LayerB.ForUpdateOrders(dr, "ShipName");
                        dao.Set_ShipAddress_forUPD    = LayerB.ForUpdateOrders(dr, "ShipAddress");
                        dao.Set_ShipCity_forUPD       = LayerB.ForUpdateOrders(dr, "ShipCity");
                        dao.Set_ShipRegion_forUPD     = LayerB.ForUpdateOrders(dr, "ShipRegion");
                        dao.Set_ShipPostalCode_forUPD = LayerB.ForUpdateOrders(dr, "ShipPostalCode");
                        dao.Set_ShipCountry_forUPD    = LayerB.ForUpdateOrders(dr, "ShipCountry");

                        int updated = dao.D3_Update();
                        if (updated == 0)
                        {
                            throw new BusinessApplicationException(
                                "OrdersBatchUpdate", "他のユーザによって更新されています。",
                                "OrderID = " + dr["OrderID", DataRowVersion.Original]);
                        }
                        returnValue.UpdateCount += updated;
                        break;

                    default:
                        // Unchanged / Deleted（Deleted は上のループで処理済み）は対象外
                        break;
                }
            }

            // ↑業務処理-----------------------------------------------------
        }

        /// <summary>楽観排他：取得時の値（Original）を WHERE 用パラメタに設定する</summary>
        /// <param name="dao">自動生成Dao</param>
        /// <param name="dr">対象の DataRow</param>
        /// <remarks>
        /// Orders は ntext 等の「= で比較できない型」を持たないため、全列を WHERE に入れられる。
        /// ★ Original が DBNull の列は null に読み替えて渡す（&lt;ELSE&gt; の「IS NULL」を出させる）。
        ///   DBNull のまま渡すと「= @col（NULL）」になり、決して一致しない。
        /// </remarks>
        private static void SetOriginalToWhereForOrders(DaoOrders dao, DataRow dr)
        {
            dao.CustomerID     = LayerB.ForWhereOrders(dr, "CustomerID");
            dao.EmployeeID     = LayerB.ForWhereOrders(dr, "EmployeeID");
            dao.OrderDate      = LayerB.ForWhereOrders(dr, "OrderDate");
            dao.RequiredDate   = LayerB.ForWhereOrders(dr, "RequiredDate");
            dao.ShippedDate    = LayerB.ForWhereOrders(dr, "ShippedDate");
            dao.ShipVia        = LayerB.ForWhereOrders(dr, "ShipVia");
            dao.Freight        = LayerB.ForWhereOrders(dr, "Freight");
            dao.ShipName       = LayerB.ForWhereOrders(dr, "ShipName");
            dao.ShipAddress    = LayerB.ForWhereOrders(dr, "ShipAddress");
            dao.ShipCity       = LayerB.ForWhereOrders(dr, "ShipCity");
            dao.ShipRegion     = LayerB.ForWhereOrders(dr, "ShipRegion");
            dao.ShipPostalCode = LayerB.ForWhereOrders(dr, "ShipPostalCode");
            dao.ShipCountry    = LayerB.ForWhereOrders(dr, "ShipCountry");
        }

        /// <summary>WHERE 用の値（DBNull は null に読み替える）</summary>
        private static object ForWhereOrders(DataRow dr, string columnName)
        {
            object value = dr[columnName, DataRowVersion.Original];
            return (value == DBNull.Value) ? null : value;
        }

        /// <summary>SET 用の値（空欄は DBNull＝NULL に落とす）</summary>
        private static object ForUpdateOrders(DataRow dr, string columnName)
        {
            object value = dr[columnName];
            if (value == DBNull.Value) { return DBNull.Value; }
            return (value.ToString().Length == 0) ? (object)DBNull.Value : value;
        }

        /// <summary>INSERT 用の値（Orders は全列 NULL 許容なので、空欄は DBNull にする）</summary>
        private static object ForInsertOrders(DataRow dr, string columnName)
        {
            object value = dr[columnName];
            if (value == DBNull.Value) { return DBNull.Value; }
            return (value.ToString().Length == 0) ? (object)DBNull.Value : value;
        }

        #endregion

        #endregion



        #region 受注管理（Ord）：条件検索一覧・詳細・更新

        #region 条件検索一覧（共通Dao ＋ 動的パラメタライズドクエリ）

        /// <summary>受注（Orders）を条件検索して一覧を取得する（共通Dao ＋ 動的クエリ／ページング付き）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// ★ Ｄ層は共通Dao（CmnDao）を使用する（仕様）。
        /// ★ 仕様：SQL でマスタ・テーブルと JOIN して表示値に変換しておく（OrdListSearch.xml）。
        /// ★ ページングは SQL 制御方式（ROW_NUMBER + CTE）。全件取得してメモリで切らない。
        /// ★ 検索条件は「パラメタを設定しない＝その &lt;IF&gt; ブロックごと WHERE から消える」で外す。
        ///   null を設定すると「IS NULL」になってしまい、条件を外したことにならない。
        /// </remarks>
        private void UOC_OrdListSearch(OrdParameterValue parameterValue)
        {
            // 戻り値クラスを生成して、事前に戻り値に設定しておく。
            OrdReturnValue returnValue = new OrdReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------

            // --- 総件数（同じ条件で COUNT。ページャの総ページ数に使う） ---
            CmnDao countDao = new CmnDao(this.GetDam());
            countDao.SQLFileName = "OrdListSearchCount.xml";
            LayerB.SetOrdSearchCondition(countDao, parameterValue);
            returnValue.TotalCount = int.Parse(countDao.ExecSelectScalar().ToString());

            // --- 現在ページ分の明細 ---
            int pageSize = (parameterValue.PageSize <= 0) ? 20 : parameterValue.PageSize;
            int pageIndex = (parameterValue.PageIndex <= 0) ? 1 : parameterValue.PageIndex;

            CmnDao cmnDao = new CmnDao(this.GetDam());
            cmnDao.SQLFileName = "OrdListSearch.xml";
            LayerB.SetOrdSearchCondition(cmnDao, parameterValue);

            // RNUM の範囲（1 起算）
            cmnDao.SetParameter("FromRow", ((pageIndex - 1) * pageSize) + 1);
            cmnDao.SetParameter("ToRow", pageIndex * pageSize);

            DataTable dt = new DataTable("Orders");
            cmnDao.ExecSelectFill_DT(dt);
            returnValue.Orders = dt;

            // ↑業務処理-----------------------------------------------------
        }

        /// <summary>検索条件を動的クエリのパラメタに設定する</summary>
        /// <param name="cmnDao">共通Dao</param>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// ★ 未指定の条件は「設定しない」＝&lt;IF&gt; ごと WHERE から消える。
        ///   ここで null や空文字を渡すと条件が残ってしまう。
        /// </remarks>
        private static void SetOrdSearchCondition(CmnDao cmnDao, OrdParameterValue parameterValue)
        {
            if (!string.IsNullOrEmpty(parameterValue.CustomerID))
            {
                cmnDao.SetParameter("CustomerID", parameterValue.CustomerID);
            }

            if (!string.IsNullOrEmpty(parameterValue.EmployeeID))
            {
                cmnDao.SetParameter("EmployeeID", int.Parse(parameterValue.EmployeeID));
            }

            if (!string.IsNullOrEmpty(parameterValue.ShipVia))
            {
                cmnDao.SetParameter("ShipVia", int.Parse(parameterValue.ShipVia));
            }

            if (!string.IsNullOrEmpty(parameterValue.ShipCountry))
            {
                // 前方一致（ユーザ入力は必ずパラメタで渡す＝ユーザパラメタにしない）
                cmnDao.SetParameter("ShipCountry", parameterValue.ShipCountry + "%");
            }
        }

        #endregion

        #region ドロップダウン用のマスタ取得（共通Dao）

        /// <summary>ドロップダウン用のマスタを取得する（共通Dao 経由）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>仕様：画面Ｂの初期処理で「マスタ・テーブル値入力用DDL」を生成するために使う。</remarks>
        private void UOC_OrdMasters(OrdParameterValue parameterValue)
        {
            OrdReturnValue returnValue = new OrdReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------
            CmnDao dao = new CmnDao(this.GetDam());

            DataTable customers = new DataTable("Customers");
            dao.SQLFileName = "CustomerListForDdl.sql";
            dao.ExecSelectFill_DT(customers);
            returnValue.Customers = customers;

            DataTable employees = new DataTable("Employees");
            dao.SQLFileName = "EmployeeListForDdl.sql";
            dao.ExecSelectFill_DT(employees);
            returnValue.Employees = employees;

            DataTable shippers = new DataTable("Shippers");
            dao.SQLFileName = "ShipperListForDdl.sql";
            dao.ExecSelectFill_DT(shippers);
            returnValue.Shippers = shippers;

            // 明細（Order Details）の ProductID を ＤＤＬ 化するためのマスタ
            DataTable products = new DataTable("Products");
            dao.SQLFileName = "ProductListForDdl.sql";
            dao.ExecSelectFill_DT(products);
            returnValue.Products = products;
            // ↑業務処理-----------------------------------------------------
        }

        #endregion

        #region 詳細表示（自動生成Dao の参照＝Ｒ）

        /// <summary>受注（Orders）を1件参照する（自動生成Dao の S2_Select）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// ★ 新規（追加）モードは OrderID を渡さない＝0 件で戻る。
        ///   ExecSelectFill_DT（内部は DataAdapter.Fill）は 0 件でも列（スキーマ）は作るので、
        ///   画面Ｂはこの空の DataTable に NewRow() を足して「追加」の入力欄にできる。
        /// </remarks>
        private void UOC_OrdDetailedView(OrdParameterValue parameterValue)
        {
            OrdReturnValue returnValue = new OrdReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------
            DaoOrders dao = new DaoOrders(this.GetDam());
            dao.ClearParametersFromHt();

            int orderId = 0;
            if (!string.IsNullOrEmpty(parameterValue.OrderID))
            {
                int.TryParse(parameterValue.OrderID, out orderId);
            }
            dao.PK_OrderID = orderId;

            DataTable dt = new DataTable("Orders");
            dao.S2_Select(dt);
            returnValue.Order = dt;

            // --- 明細（Order Details）を参照（Ｒ）する ---
            // ★ 複合主キー（OrderID + ProductID）のうち OrderID だけを設定して動的クエリを使う。
            //   設定しなかった列の <IF> はブロックごと消えるので、WHERE は [OrderID] = @OrderID
            //   だけになり、その受注の明細が全件取れる（S2_Select は複合主キー全指定＝1件用）。
            DaoOrder_Details detailDao = new DaoOrder_Details(this.GetDam());
            detailDao.ClearParametersFromHt();
            detailDao.PK_OrderID = orderId;

            DataTable details = new DataTable("OrderDetails");
            detailDao.D2_Select(details);
            returnValue.OrderDetails = details;
            // ↑業務処理-----------------------------------------------------
        }

        #endregion

        #region 追加・更新・削除（自動生成Dao の Ｃ・Ｕ・Ｄ）

        /// <summary>受注（Orders）を1件追加する（自動生成Dao の D1_Insert）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// ★ OrderID は IDENTITY（自動採番）＝INSERT に含めない。
        ///   含めると "IDENTITY_INSERT が OFF..." で必ず失敗するため、
        ///   全列必須の S1_Insert ではなく、設定した列だけを INSERT する D1_Insert を使う。
        /// </remarks>
        private void UOC_OrdInsert(OrdParameterValue parameterValue)
        {
            OrdReturnValue returnValue = new OrdReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------
            DataRow dr = LayerB.GetOrdSingleRow(parameterValue);

            DaoOrders dao = new DaoOrders(this.GetDam());
            dao.ClearParametersFromHt();

            dao.CustomerID     = LayerB.OrdCurrent(dr, "CustomerID");
            dao.EmployeeID     = LayerB.OrdCurrent(dr, "EmployeeID");
            dao.OrderDate      = LayerB.OrdCurrent(dr, "OrderDate");
            dao.RequiredDate   = LayerB.OrdCurrent(dr, "RequiredDate");
            dao.ShippedDate    = LayerB.OrdCurrent(dr, "ShippedDate");
            dao.ShipVia        = LayerB.OrdCurrent(dr, "ShipVia");
            dao.Freight        = LayerB.OrdCurrent(dr, "Freight");
            dao.ShipName       = LayerB.OrdCurrent(dr, "ShipName");
            dao.ShipAddress    = LayerB.OrdCurrent(dr, "ShipAddress");
            dao.ShipCity       = LayerB.OrdCurrent(dr, "ShipCity");
            dao.ShipRegion     = LayerB.OrdCurrent(dr, "ShipRegion");
            dao.ShipPostalCode = LayerB.OrdCurrent(dr, "ShipPostalCode");
            dao.ShipCountry    = LayerB.OrdCurrent(dr, "ShipCountry");

            returnValue.InsertCount = dao.D1_Insert();

            // --- 採番された OrderID を取り、明細（子）に差し込んでバッチ追加する ---
            // ★ SCOPE_IDENTITY() は同一スコープ（＝同一バッチ）限定なので、別コマンドで実行する
            //   ここでは NULL になる。@@IDENTITY は同一コネクション内で有効＝Ｂ層が持つ接続のまま取れる。
            CmnDao idDao = new CmnDao(this.GetDam());
            idDao.SQLFileName = "OrdLastIdentity.sql";
            object newId = idDao.ExecSelectScalar();
            returnValue.NewOrderID = (newId == null || newId == DBNull.Value) ? 0 : Convert.ToInt32(newId);

            LayerB.BatchUpdateOrdDetails(
                this.GetDam(), parameterValue.OrderDetails, returnValue.NewOrderID, returnValue);
            // ↑業務処理-----------------------------------------------------
        }

        /// <summary>受注（Orders）を1件更新する（自動生成Dao の D3_Update）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>
        /// 楽観排他：WHERE に「取得時の値（DataRowVersion.Original）」を入れ、
        /// 更新件数0＝他のユーザが先に更新した、として業務例外にする。
        /// </remarks>
        private void UOC_OrdUpdate(OrdParameterValue parameterValue)
        {
            OrdReturnValue returnValue = new OrdReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------
            DataRow dr = LayerB.GetOrdSingleRow(parameterValue);

            DaoOrders dao = new DaoOrders(this.GetDam());
            dao.ClearParametersFromHt();

            // WHERE 用：主キー＋取得時の値（楽観排他）
            dao.PK_OrderID = dr["OrderID", DataRowVersion.Original];
            LayerB.SetOrdOriginalToWhere(dao, dr);

            // SET 用：現在値（空欄は DBNull＝NULL に落とす。WHERE 側とは役割が逆）
            dao.Set_CustomerID_forUPD     = LayerB.OrdCurrent(dr, "CustomerID");
            dao.Set_EmployeeID_forUPD     = LayerB.OrdCurrent(dr, "EmployeeID");
            dao.Set_OrderDate_forUPD      = LayerB.OrdCurrent(dr, "OrderDate");
            dao.Set_RequiredDate_forUPD   = LayerB.OrdCurrent(dr, "RequiredDate");
            dao.Set_ShippedDate_forUPD    = LayerB.OrdCurrent(dr, "ShippedDate");
            dao.Set_ShipVia_forUPD        = LayerB.OrdCurrent(dr, "ShipVia");
            dao.Set_Freight_forUPD        = LayerB.OrdCurrent(dr, "Freight");
            dao.Set_ShipName_forUPD       = LayerB.OrdCurrent(dr, "ShipName");
            dao.Set_ShipAddress_forUPD    = LayerB.OrdCurrent(dr, "ShipAddress");
            dao.Set_ShipCity_forUPD       = LayerB.OrdCurrent(dr, "ShipCity");
            dao.Set_ShipRegion_forUPD     = LayerB.OrdCurrent(dr, "ShipRegion");
            dao.Set_ShipPostalCode_forUPD = LayerB.OrdCurrent(dr, "ShipPostalCode");
            dao.Set_ShipCountry_forUPD    = LayerB.OrdCurrent(dr, "ShipCountry");

            int updated = dao.D3_Update();
            if (updated == 0)
            {
                throw new BusinessApplicationException(
                    "OrdUpdate", "他のユーザによって更新されています。",
                    "OrderID = " + dr["OrderID", DataRowVersion.Original]);
            }
            returnValue.UpdateCount = updated;

            // --- 子（明細）のバッチ更新。親と同じトランザクション（この UOC メソッド全体）で行う ---
            LayerB.BatchUpdateOrdDetails(
                this.GetDam(), parameterValue.OrderDetails,
                dr["OrderID", DataRowVersion.Original], returnValue);
            // ↑業務処理-----------------------------------------------------
        }

        /// <summary>受注（Orders）を1件削除する（自動生成Dao の D4_Delete）</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <remarks>楽観排他は更新と同じ（取得時の値を WHERE に入れ、件数0を業務例外にする）。</remarks>
        private void UOC_OrdDelete(OrdParameterValue parameterValue)
        {
            OrdReturnValue returnValue = new OrdReturnValue();
            this.ReturnValue = returnValue;

            // ↓業務処理-----------------------------------------------------
            DataRow dr = LayerB.GetOrdSingleRow(parameterValue);

            // ★ FK（FK_Order_Details_Orders）があるので、子（明細）を先に消す。
            //   親から消すと参照整合性違反で落ちる（順序を間違えると実行時まで分からない）。
            //   複合主キーのうち OrderID だけを設定＝その受注の明細を全件削除する。
            DaoOrder_Details detailDao = new DaoOrder_Details(this.GetDam());
            detailDao.ClearParametersFromHt();
            detailDao.PK_OrderID = dr["OrderID", DataRowVersion.Original];
            returnValue.DetailDeleteCount = detailDao.D4_Delete();

            DaoOrders dao = new DaoOrders(this.GetDam());
            dao.ClearParametersFromHt();

            dao.PK_OrderID = dr["OrderID", DataRowVersion.Original];
            LayerB.SetOrdOriginalToWhere(dao, dr);

            int deleted = dao.D4_Delete();
            if (deleted == 0)
            {
                throw new BusinessApplicationException(
                    "OrdDelete", "他のユーザによって更新されています。",
                    "OrderID = " + dr["OrderID", DataRowVersion.Original]);
            }
            returnValue.DeleteCount = deleted;
            // ↑業務処理-----------------------------------------------------
        }

        /// <summary>ＣＵＤの対象（1行だけの DataTable）から DataRow を取り出す</summary>
        /// <param name="parameterValue">引数クラス</param>
        /// <returns>対象の DataRow</returns>
        private static DataRow GetOrdSingleRow(OrdParameterValue parameterValue)
        {
            if (parameterValue.Order == null || parameterValue.Order.Rows.Count == 0)
            {
                throw new BusinessApplicationException(
                    "OrdNoTarget", "処理対象のデータがありません。", "");
            }
            return parameterValue.Order.Rows[0];
        }

        /// <summary>楽観排他：取得時の値（Original）を WHERE 用パラメタに設定する</summary>
        /// <param name="dao">自動生成Dao</param>
        /// <param name="dr">対象の DataRow</param>
        /// <remarks>
        /// Orders は ntext 等の「= で比較できない型」を持たないため、全列を WHERE に入れられる。
        /// ★ Original が DBNull の列は null に読み替えて渡す（&lt;ELSE&gt; の「IS NULL」を出させる）。
        ///   DBNull のまま渡すと「= @col（NULL）」になり、決して一致しない。
        /// </remarks>
        private static void SetOrdOriginalToWhere(DaoOrders dao, DataRow dr)
        {
            dao.CustomerID     = LayerB.OrdWhere(dr, "CustomerID");
            dao.EmployeeID     = LayerB.OrdWhere(dr, "EmployeeID");
            dao.OrderDate      = LayerB.OrdWhere(dr, "OrderDate");
            dao.RequiredDate   = LayerB.OrdWhere(dr, "RequiredDate");
            dao.ShippedDate    = LayerB.OrdWhere(dr, "ShippedDate");
            dao.ShipVia        = LayerB.OrdWhere(dr, "ShipVia");
            dao.Freight        = LayerB.OrdWhere(dr, "Freight");
            dao.ShipName       = LayerB.OrdWhere(dr, "ShipName");
            dao.ShipAddress    = LayerB.OrdWhere(dr, "ShipAddress");
            dao.ShipCity       = LayerB.OrdWhere(dr, "ShipCity");
            dao.ShipRegion     = LayerB.OrdWhere(dr, "ShipRegion");
            dao.ShipPostalCode = LayerB.OrdWhere(dr, "ShipPostalCode");
            dao.ShipCountry    = LayerB.OrdWhere(dr, "ShipCountry");
        }

        /// <summary>WHERE 用の値（取得時の値。DBNull は null に読み替える）</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <returns>パラメタに設定する値</returns>
        private static object OrdWhere(DataRow dr, string columnName)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return null; }

            object value = dr[columnName, DataRowVersion.Original];
            return (value == DBNull.Value) ? null : value;
        }

        /// <summary>INSERT / SET 用の値（現在値。空欄は DBNull＝NULL に落とす）</summary>
        /// <param name="dr">対象の DataRow</param>
        /// <param name="columnName">列名</param>
        /// <returns>パラメタに設定する値</returns>
        /// <remarks>Orders は OrderID 以外すべて NULL 許容なので、空欄は DBNull でよい。</remarks>
        private static object OrdCurrent(DataRow dr, string columnName)
        {
            if (!dr.Table.Columns.Contains(columnName)) { return DBNull.Value; }

            object value = dr[columnName];
            if (value == DBNull.Value) { return DBNull.Value; }
            return (value.ToString().Length == 0) ? (object)DBNull.Value : value;
        }

        #endregion

        #region 明細（Order Details）のバッチ更新

        /// <summary>明細（Order Details）を RowState で振り分けてバッチ更新する</summary>
        /// <param name="dam">Ｂ層が持つ Dam（親と同じ接続・同じトランザクション）</param>
        /// <param name="details">明細の DataTable（RowState を保持したもの）</param>
        /// <param name="orderId">親の OrderID（追加時は採番値を差し込む）</param>
        /// <param name="returnValue">戻り値クラス（件数を積む）</param>
        /// <remarks>
        /// ★ 削除 → 追加 の順で流す（同じ主キーを付け替えたときに旧行と衝突しないため）。
        /// ★ 楽観排他は親と同じ考え方（取得時の値を WHERE に入れ、件数0 を業務例外にする）。
        /// </remarks>
        private static void BatchUpdateOrdDetails(
            Touryo.Infrastructure.Public.Db.BaseDam dam, DataTable details, object orderId, OrdReturnValue returnValue)
        {
            if (details == null) { return; }

            DaoOrder_Details dao = new DaoOrder_Details(dam);

            foreach (DataRow dr in details.Rows)
            {
                if (dr.RowState != DataRowState.Deleted) { continue; }

                dao.ClearParametersFromHt();

                // ★ 削除行は DataRowVersion.Original しか読めない。
                dao.PK_OrderID   = dr["OrderID", DataRowVersion.Original];
                dao.PK_ProductID = dr["ProductID", DataRowVersion.Original];
                dao.UnitPrice    = LayerB.OrdWhere(dr, "UnitPrice");
                dao.Quantity     = LayerB.OrdWhere(dr, "Quantity");
                dao.Discount     = LayerB.OrdWhere(dr, "Discount");

                int deleted = dao.D4_Delete();
                if (deleted == 0)
                {
                    throw new BusinessApplicationException(
                        "OrdDetailUpdate", "明細が他のユーザによって更新されています。",
                        "ProductID = " + dr["ProductID", DataRowVersion.Original]);
                }
                returnValue.DetailDeleteCount += deleted;
            }

            foreach (DataRow dr in details.Rows)
            {
                switch (dr.RowState)
                {
                    case DataRowState.Added:

                        dao.ClearParametersFromHt();

                        // ★ Order Details は IDENTITY を持たず全列 NOT NULL なので、
                        //   全列必須の S1_Insert がそのまま使える
                        //   （親 Orders は IDENTITY があるため D1_Insert 一択だったのと対照的）。
                        dao.PK_OrderID   = orderId;
                        dao.PK_ProductID = LayerB.OrdCurrent(dr, "ProductID");
                        dao.UnitPrice    = LayerB.OrdCurrent(dr, "UnitPrice");
                        dao.Quantity     = LayerB.OrdCurrent(dr, "Quantity");
                        dao.Discount     = LayerB.OrdCurrent(dr, "Discount");

                        returnValue.DetailInsertCount += dao.S1_Insert();
                        break;

                    case DataRowState.Modified:

                        dao.ClearParametersFromHt();

                        // WHERE 用：取得時の主キー＋取得時の値（楽観排他）
                        dao.PK_OrderID   = dr["OrderID", DataRowVersion.Original];
                        dao.PK_ProductID = dr["ProductID", DataRowVersion.Original];
                        dao.UnitPrice    = LayerB.OrdWhere(dr, "UnitPrice");
                        dao.Quantity     = LayerB.OrdWhere(dr, "Quantity");
                        dao.Discount     = LayerB.OrdWhere(dr, "Discount");

                        // SET 用：現在値（ProductID の付け替えも許す）
                        dao.Set_OrderID_forUPD   = orderId;
                        dao.Set_ProductID_forUPD = LayerB.OrdCurrent(dr, "ProductID");
                        dao.Set_UnitPrice_forUPD = LayerB.OrdCurrent(dr, "UnitPrice");
                        dao.Set_Quantity_forUPD  = LayerB.OrdCurrent(dr, "Quantity");
                        dao.Set_Discount_forUPD  = LayerB.OrdCurrent(dr, "Discount");

                        int updated = dao.D3_Update();
                        if (updated == 0)
                        {
                            throw new BusinessApplicationException(
                                "OrdDetailUpdate", "明細が他のユーザによって更新されています。",
                                "ProductID = " + dr["ProductID", DataRowVersion.Original]);
                        }
                        returnValue.DetailUpdateCount += updated;
                        break;

                    default:
                        // Unchanged / Deleted（Deleted は上のループで処理済み）は対象外
                        break;
                }
            }
        }

        #endregion

        #endregion

    }
}
