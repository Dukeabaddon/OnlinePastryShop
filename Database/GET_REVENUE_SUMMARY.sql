--------------------------------------------------------
--  DDL for Function GET_REVENUE_SUMMARY
--------------------------------------------------------

  CREATE OR REPLACE FUNCTION "AARON_IPT"."GET_REVENUE_SUMMARY" (
    p_days IN NUMBER DEFAULT 30
) RETURN SYS_REFCURSOR AS
    v_result SYS_REFCURSOR;
    v_start_date DATE := TRUNC(SYSDATE) - p_days;
    v_prev_start_date DATE := v_start_date - p_days;
    v_prev_end_date DATE := TRUNC(SYSDATE) - p_days - 1;
BEGIN
    OPEN v_result FOR
        SELECT
            'Current' AS Period,
            COUNT(DISTINCT O.ORDERID) AS OrderCount,
            NVL(SUM(O.TOTALAMOUNT), 0) AS TotalRevenue,
            (SELECT COUNT(DISTINCT USERID) FROM ORDERS 
             WHERE ORDERDATE >= v_start_date) AS CustomerCount,
            (SELECT COUNT(DISTINCT PRODUCTID) FROM ORDERDETAILS OD
             JOIN ORDERS O ON OD.ORDERID = O.ORDERID
             WHERE O.ORDERDATE >= v_start_date) AS ProductCount
        FROM
            ORDERS O
        WHERE
            O.ORDERDATE >= v_start_date
            AND O.STATUS NOT IN ('Cancelled', 'Rejected')

        UNION ALL

        SELECT
            'Previous' AS Period,
            COUNT(DISTINCT O.ORDERID) AS OrderCount,
            NVL(SUM(O.TOTALAMOUNT), 0) AS TotalRevenue,
            (SELECT COUNT(DISTINCT USERID) FROM ORDERS 
             WHERE ORDERDATE BETWEEN v_prev_start_date AND v_prev_end_date) AS CustomerCount,
            (SELECT COUNT(DISTINCT PRODUCTID) FROM ORDERDETAILS OD
             JOIN ORDERS O ON OD.ORDERID = O.ORDERID
             WHERE O.ORDERDATE BETWEEN v_prev_start_date AND v_prev_end_date) AS ProductCount
        FROM
            ORDERS O
        WHERE
            O.ORDERDATE BETWEEN v_prev_start_date AND v_prev_end_date
            AND O.STATUS NOT IN ('Cancelled', 'Rejected');

    RETURN v_result;
END GET_REVENUE_SUMMARY;

/
