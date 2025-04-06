--------------------------------------------------------
--  DDL for Function GET_TOTAL_REVENUE
--------------------------------------------------------

  CREATE OR REPLACE FUNCTION "AARON_IPT"."GET_TOTAL_REVENUE" (
    p_timeframe IN VARCHAR2,
    p_start_date IN DATE,
    p_end_date IN DATE
) RETURN NUMBER AS
    v_total_revenue NUMBER := 0;
BEGIN
    SELECT NVL(SUM(TOTALAMOUNT), 0)
    INTO v_total_revenue
    FROM ORDERS
    WHERE ORDERDATE BETWEEN p_start_date AND p_end_date
    AND STATUS NOT IN ('Cancelled', 'Rejected');

    RETURN v_total_revenue;
END;

/
