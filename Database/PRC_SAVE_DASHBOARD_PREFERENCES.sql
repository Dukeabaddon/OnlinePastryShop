--------------------------------------------------------
--  DDL for Procedure PRC_SAVE_DASHBOARD_PREFERENCES
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_SAVE_DASHBOARD_PREFERENCES" (
    p_user_id IN NUMBER,
    p_revenue_visible IN NUMBER,
    p_orders_visible IN NUMBER,
    p_pending_orders_visible IN NUMBER,
    p_low_stock_visible IN NUMBER,
    p_success OUT NUMBER
)
AS
    v_exists NUMBER;
    v_visible_count NUMBER;
BEGIN
    -- Validate input
    v_visible_count := p_revenue_visible + p_orders_visible + p_pending_orders_visible + p_low_stock_visible;

    -- Ensure at least 3 components are visible
    IF v_visible_count < 3 THEN
        p_success := 0;
        RETURN;
    END IF;

    -- Check if preferences exist for this user
    SELECT COUNT(*) INTO v_exists
    FROM DASHBOARD_PREFERENCES
    WHERE USER_ID = p_user_id;

    IF v_exists > 0 THEN
        -- Update existing preferences
        UPDATE DASHBOARD_PREFERENCES
        SET 
            REVENUE_VISIBLE = p_revenue_visible,
            ORDERS_VISIBLE = p_orders_visible,
            PENDING_ORDERS_VISIBLE = p_pending_orders_visible,
            LOW_STOCK_VISIBLE = p_low_stock_visible,
            UPDATED_AT = CURRENT_TIMESTAMP
        WHERE USER_ID = p_user_id;
    ELSE
        -- Insert new preferences
        INSERT INTO DASHBOARD_PREFERENCES (
            USER_ID, 
            REVENUE_VISIBLE, 
            ORDERS_VISIBLE, 
            PENDING_ORDERS_VISIBLE, 
            LOW_STOCK_VISIBLE
        ) VALUES (
            p_user_id, 
            p_revenue_visible,
            p_orders_visible,
            p_pending_orders_visible,
            p_low_stock_visible
        );
    END IF;

    COMMIT;
    p_success := 1;

    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            p_success := 0;
            DBMS_OUTPUT.PUT_LINE('Error in PRC_SAVE_DASHBOARD_PREFERENCES: ' || SQLERRM);
END PRC_SAVE_DASHBOARD_PREFERENCES;

/
