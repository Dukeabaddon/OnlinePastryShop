--------------------------------------------------------
--  DDL for Procedure PRC_CALC_REVENUE_COMP
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_CALC_REVENUE_COMP" ( -- Shortened Name
    p_start_date IN DATE,
    p_end_date IN DATE,
    p_prev_start_date IN DATE,
    p_prev_end_date IN DATE,
    p_current_revenue OUT NUMBER,
    p_previous_revenue OUT NUMBER,
    p_revenue_change OUT NUMBER
) AS
BEGIN
    -- Calculate current revenue
    SELECT NVL(SUM(OD.QUANTITY * OD.PRICE), 0)
    INTO p_current_revenue
    FROM ORDERDETAILS OD
    JOIN ORDERS O ON OD.ORDERID = O.ORDERID
    WHERE O.ORDERDATE BETWEEN p_start_date AND p_end_date
    AND O.STATUS NOT IN ('Cancelled', 'Rejected', 'Pending');

    -- Calculate previous revenue
    SELECT NVL(SUM(OD.QUANTITY * OD.PRICE), 0)
    INTO p_previous_revenue
    FROM ORDERDETAILS OD
    JOIN ORDERS O ON OD.ORDERID = O.ORDERID
    WHERE O.ORDERDATE BETWEEN p_prev_start_date AND p_prev_end_date
    AND O.STATUS NOT IN ('Cancelled', 'Rejected', 'Pending');

    -- Calculate percentage change
    IF p_previous_revenue IS NULL OR p_previous_revenue = 0 THEN
        IF p_current_revenue > 0 THEN
            p_revenue_change := 100.0; -- Indicate increase if previous was zero
        ELSE
             p_revenue_change := 0.0; -- No change if both are zero
        END IF;
    ELSE
        p_revenue_change := ROUND(((p_current_revenue - p_previous_revenue) / p_previous_revenue) * 100, 1);
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        p_current_revenue := 0;
        p_previous_revenue := 0;
        p_revenue_change := 0;
        -- Consider adding logging here using PRC_LOG_ERROR if it exists
        RAISE;
END PRC_CALC_REVENUE_COMP; -- Shortened Name

/
