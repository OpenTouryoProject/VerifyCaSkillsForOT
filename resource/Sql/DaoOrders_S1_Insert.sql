-- DaoOrders_S1_Insert
-- 2026/8/28 日立 太郎
INSERT INTO 
  [Orders]
    (
      [OrderID],
      [CustomerID],
      [EmployeeID],
      [OrderDate],
      [RequiredDate],
      [ShippedDate],
      [ShipVia],
      [Freight],
      [ShipName],
      [ShipAddress],
      [ShipCity],
      [ShipRegion],
      [ShipPostalCode],
      [ShipCountry]
    )
VALUES
    (
      @OrderID,
      @CustomerID,
      @EmployeeID,
      @OrderDate,
      @RequiredDate,
      @ShippedDate,
      @ShipVia,
      @Freight,
      @ShipName,
      @ShipAddress,
      @ShipCity,
      @ShipRegion,
      @ShipPostalCode,
      @ShipCountry
    )
