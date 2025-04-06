--------------------------------------------------------
--  DDL for Procedure APPROVE_ORDER
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."APPROVE_ORDER" (
    p_order_id IN NUMBER,
    p_success OUT NUMBER,
    p_message OUT VARCHAR2
) AS
    v_order_exists NUMBER;
    v_correct_total NUMBER;
BEGIN
    -- Check if order exists
    SELECT COUNT(*) INTO v_order_exists
    FROM ORDERS
    WHERE ORDERID = p_order_id AND ISACTIVE = 1;

    IF v_order_exists = 0 THEN
        p_success := 0;
        p_message := 'Order not found or inactive';
        RETURN;
    END IF;

    -- Calculate correct total from order details
    SELECT NVL(SUM(PRICE), 0) INTO v_correct_total
    FROM ORDERDETAILS
    WHERE ORDERID = p_order_id AND ISACTIVE = 1;

    -- Update order status and ensure total amount is correct
    UPDATE ORDERS
    SET STATUS = 'Approved',
        TOTALAMOUNT = v_correct_total
    WHERE ORDERID = p_order_id AND ISACTIVE = 1;

    -- Return success
    p_success := 1;
    p_message := 'Order approved successfully. Total amount: ' || v_correct_total;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_success := 0;
        p_message := 'Error: ' || SQLERRM;
END;

/
