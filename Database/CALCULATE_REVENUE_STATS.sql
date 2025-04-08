--------------------------------------------------------
--  DDL for Procedure CALCULATE_REVENUE_STATS
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."CALCULATE_REVENUE_STATS" (
    p_time_range IN VARCHAR2,
    p_order_count OUT NUMBER,
    p_detail_count OUT NUMBER,
    p_total_revenue OUT NUMBER,
    p_profit OUT NUMBER
) AS
    v_date_condition VARCHAR2(200);
BEGIN
    -- Initialize outputs
    p_order_count := 0;
    p_detail_count := 0;
    p_total_revenue := 0;
    p_profit := 0;
    
    -- Set the date condition based on time range
    CASE p_time_range
        WHEN 'today' THEN
            v_date_condition := 'TRUNC(ORDERDATE) = TRUNC(SYSDATE)';
        WHEN 'yesterday' THEN
            v_date_condition := 'TRUNC(ORDERDATE) = TRUNC(SYSDATE) - 1';
        WHEN 'week' THEN
            v_date_condition := 'TRUNC(ORDERDATE) >= TRUNC(SYSDATE) - 7';
        WHEN 'month' THEN
            v_date_condition := 'TRUNC(ORDERDATE) >= TRUNC(SYSDATE) - 30';
        ELSE
            v_date_condition := 'TRUNC(ORDERDATE) = TRUNC(SYSDATE)'; -- Default to today
    END CASE;
    
    -- Get approved order count
    EXECUTE IMMEDIATE 
        'SELECT COUNT(*) FROM AARON_IPT.ORDERS WHERE STATUS = ''Approved'' AND ' || v_date_condition
        INTO p_order_count;
    
    -- Get order details count
    EXECUTE IMMEDIATE 
        'SELECT COUNT(*) FROM AARON_IPT.ORDERDETAILS OD ' ||
        'JOIN AARON_IPT.ORDERS O ON OD.ORDERID = O.ORDERID ' ||
        'WHERE O.STATUS = ''Approved'' AND ' || v_date_condition
        INTO p_detail_count;
    
    -- Calculate total revenue (sum of order details prices)
    EXECUTE IMMEDIATE 
        'SELECT NVL(SUM(OD.QUANTITY * OD.PRICE), 0) ' ||
        'FROM AARON_IPT.ORDERDETAILS OD ' ||
        'JOIN AARON_IPT.ORDERS O ON OD.ORDERID = O.ORDERID ' ||
        'WHERE O.STATUS = ''Approved'' AND ' || v_date_condition
        INTO p_total_revenue;
    
    -- Calculate profit (revenue - cost) - using COSTPRICE column from PRODUCTS table
    EXECUTE IMMEDIATE 
        'SELECT NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) ' ||
        'FROM AARON_IPT.ORDERDETAILS OD ' ||
        'JOIN AARON_IPT.ORDERS O ON OD.ORDERID = O.ORDERID ' ||
        'JOIN AARON_IPT.PRODUCTS P ON OD.PRODUCTID = P.PRODUCTID ' ||
        'WHERE O.STATUS = ''Approved'' AND ' || v_date_condition
        INTO p_profit;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Log error
        DBMS_OUTPUT.PUT_LINE('Error in CALCULATE_REVENUE_STATS: ' || SQLERRM);
        RAISE;
END CALCULATE_REVENUE_STATS;

/
