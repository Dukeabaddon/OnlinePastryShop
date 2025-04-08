--------------------------------------------------------
--  DDL for Procedure CALC_REVENUE
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."CALC_REVENUE" (
  p_start_date IN DATE,
  p_end_date IN DATE,
  p_revenue OUT NUMBER)
AS
BEGIN
  SELECT NVL(SUM(OD.PRICE * OD.QUANTITY), 0) INTO p_revenue
  FROM ORDERS O
  JOIN ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
  WHERE O.STATUS = 'Approved'
  AND O.ORDERDATE BETWEEN p_start_date AND p_end_date;
END;

/
