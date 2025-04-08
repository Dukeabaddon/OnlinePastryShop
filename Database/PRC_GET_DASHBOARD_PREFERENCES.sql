--------------------------------------------------------
--  DDL for Procedure PRC_GET_DASHBOARD_PREFERENCES
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_GET_DASHBOARD_PREFERENCES" (
    p_user_id IN NUMBER,
    p_revenue_visible OUT NUMBER,
    p_orders_visible OUT NUMBER,
    p_pending_orders_visible OUT NUMBER,
    p_low_stock_visible OUT NUMBER
)
AS
    v_exists NUMBER;
BEGIN
    -- Check if preferences exist for this user
    SELECT COUNT(*) INTO v_exists
    FROM DASHBOARD_PREFERENCES
    WHERE USER_ID = p_user_id;

    IF v_exists > 0 THEN
        -- Return existing preferences
        SELECT 
            REVENUE_VISIBLE, 
            ORDERS_VISIBLE, 
            PENDING_ORDERS_VISIBLE, 
            LOW_STOCK_VISIBLE
        INTO 
            p_revenue_visible, 
            p_orders_visible, 
            p_pending_orders_visible, 
            p_low_stock_visible
        FROM DASHBOARD_PREFERENCES
        WHERE USER_ID = p_user_id;
    ELSE
        -- Insert default preferences and return them
        INSERT INTO DASHBOARD_PREFERENCES (
            USER_ID, 
            REVENUE_VISIBLE, 
            ORDERS_VISIBLE, 
            PENDING_ORDERS_VISIBLE, 
            LOW_STOCK_VISIBLE
        ) VALUES (
            p_user_id, 
            1, -- Revenue visible by default
            1, -- Orders visible by default
            1, -- Pending orders visible by default
            1  -- Low stock visible by default
        );

        p_revenue_visible := 1;
        p_orders_visible := 1;
        p_pending_orders_visible := 1;
        p_low_stock_visible := 1;
    END IF;

    EXCEPTION
        WHEN OTHERS THEN
            -- In case of error, return default values
            p_revenue_visible := 1;
            p_orders_visible := 1;
            p_pending_orders_visible := 1;
            p_low_stock_visible := 1;
            DBMS_OUTPUT.PUT_LINE('Error in PRC_GET_DASHBOARD_PREFERENCES: ' || SQLERRM);
END PRC_GET_DASHBOARD_PREFERENCES;

/
