--------------------------------------------------------
--  DDL for Procedure CALCULATE_DASHBOARD_REVENUE
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."CALCULATE_DASHBOARD_REVENUE" (
    p_start_date IN DATE,
    p_end_date IN DATE,
    p_current_revenue OUT NUMBER,
    p_previous_revenue OUT NUMBER,
    p_percentage_change OUT NUMBER
) AS
    v_previous_start_date DATE;
    v_previous_end_date DATE;
BEGIN
    -- Calculate date range for previous period (same duration)
    v_previous_start_date := p_start_date - (p_end_date - p_start_date + 1);
    v_previous_end_date := p_start_date - 1;

    -- Calculate current period revenue using order details sum
    SELECT NVL(SUM(od.PRICE), 0)
    INTO p_current_revenue
    FROM ORDERDETAILS od
    JOIN ORDERS o ON od.ORDERID = o.ORDERID
    WHERE o.STATUS = 'Approved'
    AND o.ISACTIVE = 1
    AND od.ISACTIVE = 1
    AND o.ORDERDATE BETWEEN p_start_date AND p_end_date;

    -- Calculate previous period revenue
    SELECT NVL(SUM(od.PRICE), 0)
    INTO p_previous_revenue
    FROM ORDERDETAILS od
    JOIN ORDERS o ON od.ORDERID = o.ORDERID
    WHERE o.STATUS = 'Approved'
    AND o.ISACTIVE = 1
    AND od.ISACTIVE = 1
    AND o.ORDERDATE BETWEEN v_previous_start_date AND v_previous_end_date;

    -- Calculate percentage change
    IF p_previous_revenue > 0 THEN
        p_percentage_change := ((p_current_revenue - p_previous_revenue) / p_previous_revenue) * 100;
    ELSE
        p_percentage_change := 0;
    END IF;
END;

/
