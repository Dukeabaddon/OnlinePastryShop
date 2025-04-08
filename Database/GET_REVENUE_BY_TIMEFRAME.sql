--------------------------------------------------------
--  DDL for Function GET_REVENUE_BY_TIMEFRAME
--------------------------------------------------------

  CREATE OR REPLACE FUNCTION "AARON_IPT"."GET_REVENUE_BY_TIMEFRAME" (
    p_timeframe IN VARCHAR2, -- 'daily', 'weekly', 'monthly', 'yearly'
    p_start_date IN DATE,
    p_end_date IN DATE
) RETURN SYS_REFCURSOR AS
    v_result SYS_REFCURSOR;
BEGIN
    IF p_timeframe = 'daily' THEN
        OPEN v_result FOR
            SELECT 
                TO_CHAR(ORDERDATE, 'YYYY-MM-DD') AS TimePeriod,
                COUNT(ORDERID) AS OrderCount,
                NVL(SUM(TOTALAMOUNT), 0) AS Revenue
            FROM ORDERS
            WHERE ORDERDATE BETWEEN p_start_date AND p_end_date
            AND STATUS NOT IN ('Cancelled', 'Rejected')
            GROUP BY TO_CHAR(ORDERDATE, 'YYYY-MM-DD')
            ORDER BY TimePeriod;
    ELSIF p_timeframe = 'weekly' THEN
        OPEN v_result FOR
            SELECT 
                TO_CHAR(TRUNC(ORDERDATE, 'IW'), 'YYYY-MM-DD') AS WeekStart,
                COUNT(ORDERID) AS OrderCount,
                NVL(SUM(TOTALAMOUNT), 0) AS Revenue
            FROM ORDERS
            WHERE ORDERDATE BETWEEN p_start_date AND p_end_date
            AND STATUS NOT IN ('Cancelled', 'Rejected')
            GROUP BY TO_CHAR(TRUNC(ORDERDATE, 'IW'), 'YYYY-MM-DD')
            ORDER BY WeekStart;
    ELSIF p_timeframe = 'monthly' THEN
        OPEN v_result FOR
            SELECT 
                TO_CHAR(ORDERDATE, 'YYYY-MM') AS Month,
                COUNT(ORDERID) AS OrderCount,
                NVL(SUM(TOTALAMOUNT), 0) AS Revenue
            FROM ORDERS
            WHERE ORDERDATE BETWEEN p_start_date AND p_end_date
            AND STATUS NOT IN ('Cancelled', 'Rejected')
            GROUP BY TO_CHAR(ORDERDATE, 'YYYY-MM')
            ORDER BY Month;
    ELSE -- yearly or default
        OPEN v_result FOR
            SELECT 
                TO_CHAR(ORDERDATE, 'YYYY') AS Year,
                COUNT(ORDERID) AS OrderCount,
                NVL(SUM(TOTALAMOUNT), 0) AS Revenue
            FROM ORDERS
            WHERE ORDERDATE BETWEEN p_start_date AND p_end_date
            AND STATUS NOT IN ('Cancelled', 'Rejected')
            GROUP BY TO_CHAR(ORDERDATE, 'YYYY')
            ORDER BY Year;
    END IF;

    RETURN v_result;
END GET_REVENUE_BY_TIMEFRAME;

/
