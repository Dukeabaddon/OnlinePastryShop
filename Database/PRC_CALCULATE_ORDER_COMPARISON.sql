--------------------------------------------------------
--  DDL for Procedure PRC_CALCULATE_ORDER_COMPARISON
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_CALCULATE_ORDER_COMPARISON" (
    p_start_date IN DATE,
    p_end_date IN DATE,
    p_prev_start_date IN DATE,
    p_prev_end_date IN DATE,
    p_current_orders OUT NUMBER,
    p_previous_orders OUT NUMBER,
    p_order_change OUT NUMBER
) AS
BEGIN
    -- Calculate current orders (all non-cancelled/rejected)
    SELECT COUNT(*)
    INTO p_current_orders
    FROM ORDERS O
    WHERE O.ORDERDATE BETWEEN p_start_date AND p_end_date
    AND O.STATUS NOT IN ('Cancelled', 'Rejected');

    -- Calculate previous orders
    SELECT COUNT(*)
    INTO p_previous_orders
    FROM ORDERS O
    WHERE O.ORDERDATE BETWEEN p_prev_start_date AND p_prev_end_date
    AND O.STATUS NOT IN ('Cancelled', 'Rejected');

    -- Calculate percentage change
    IF p_previous_orders IS NULL OR p_previous_orders = 0 THEN
         IF p_current_orders > 0 THEN
            p_order_change := 100.0;
        ELSE
            p_order_change := 0.0;
        END IF;
    ELSE
        p_order_change := ROUND(((p_current_orders - p_previous_orders) / p_previous_orders) * 100, 1);
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        -- Log error or handle appropriately
        p_current_orders := 0;
        p_previous_orders := 0;
        p_order_change := 0;
        -- Consider logging SQLERRM
        RAISE;
END PRC_CALCULATE_ORDER_COMPARISON;

/
