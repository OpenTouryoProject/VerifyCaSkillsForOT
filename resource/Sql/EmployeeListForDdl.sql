SELECT
  [EmployeeID], [LastName] + ' ' + [FirstName] AS [EmployeeName]
FROM
  [Employees]
ORDER BY
  [LastName], [FirstName]