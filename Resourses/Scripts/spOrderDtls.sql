--(C) SEBIN 21032024
--SELECT * FROM OrderHeaders
--SELECT * FROM OrderDetails
--spOrderDtls 6
ALTER PROCEDURE spOrderDtls(@Id int)
AS
BEGIN
	BEGIN
	SELECT Id,Name,City,State,PhoneNumber,StreetAddress,PaymentStatus,OrderTotal FROM OrderHeaders WHERE Id = @Id
	END
	BEGIN
	SELECT P.Name Product,DTL.Count,DTL.Price FROM OrderHeaders HDR 
	INNER JOIN OrderDetails DTL ON HDR.Id = DTL.OrderHeaderId
	INNER JOIN Products P ON DTL.ProductId = P.Id
	END
END