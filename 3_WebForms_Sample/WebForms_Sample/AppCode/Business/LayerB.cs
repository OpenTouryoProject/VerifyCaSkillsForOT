//**********************************************************************************
//* マスタ・テーブル（Suppliers）保守（Ｂ層）
//**********************************************************************************

//**********************************************************************************
//* クラス名        ：LayerB
//* クラス日本語名  ：Ｂ層（業務ロジック）
//*
//* 作成日時        ：2026/08/27
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  2026/08/27  生技              新規作成（Suppliers のマスタ保守）
//**********************************************************************************

using System;
using System.Data;

using Touryo.Infrastructure.Business.Business;
using Touryo.Infrastructure.Business.Dao;
using Touryo.Infrastructure.Framework.Exceptions;

namespace WebForms_Sample
{
    /// <summary>Ｂ層（業務ロジック）</summary>
    /// <remarks>
    /// トランザクション境界はＢ層。UOC メソッドは引数クラスの MethodName で自動振り分けされる。
    /// </remarks>
    public class LayerB : MyFcBaseLogic
    {

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


    }
}
