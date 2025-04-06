--------------------------------------------------------
--  DDL for Function CALCULATE_REVENUE_STATS2
--------------------------------------------------------

  CREATE OR REPLACE FUNCTION "AARON_IPT"."CALCULATE_REVENUE_STATS2" (
    p_start_date IN DATE,
    p_end_date IN DATE
) RETURN SYS_REFCURSOR AS
    v_result SYS_REFCURSOR;
BEGIN
    OPEN v_result FOR
        SELECT
            COUNT(DISTINCT O.ORDERID) AS OrderCount,
            NVL(SUM(O.TOTALAMOUNT), 0) AS TotalRevenue,
            NVL(AVG(O.TOTALAMOUNT), 0) AS AvgOrderValue,
            (SELECT COUNT(DISTINCT USERID) FROM ORDERS 
             WHERE ORDERDATE BETWEEN p_start_date AND p_end_date) AS CustomerCount,
            (SELECT COUNT(*) FROM ORDERDETAILS OD
             JOIN ORDERS O ON OD.ORDERID = O.ORDERID
             WHERE O.ORDERDATE BETWEEN p_start_date AND p_end_date) AS TotalItemsSold,
            (SELECT AVG(DETAIL_COUNT) FROM (
                SELECT O.ORDERID, COUNT(*) AS DETAIL_COUNT 
                FROM ORDERS O
                JOIN ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                WHERE O.ORDERDATE BETWEEN p_start_date AND p_end_date
                GROUP BY O.ORDERID
             )) AS AvgItemsPerOrder
        FROM
            ORDERS O
        WHERE
            O.ORDERDATE BETWEEN p_start_date AND p_end_date
            AND O.STATUS NOT IN ('Cancelled', 'Rejected');

    RETURN v_result;
END CALCULATE_REVENUE_STATS2;

/
