--------------------------------------------------------
--  DDL for Procedure PRC_CREATE_ORDER
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_CREATE_ORDER" (
    p_user_id IN NUMBER,
    p_shipping_address IN VARCHAR2,
    p_billing_address IN VARCHAR2,
    p_payment_method IN VARCHAR2,
    p_shipping_method IN VARCHAR2,
    p_order_notes IN CLOB DEFAULT NULL, -- Changed from p_notes
    p_voucher_code IN VARCHAR2 DEFAULT NULL,
    p_order_id OUT NUMBER
) AS
    v_total_amount NUMBER := 0;
    v_discount_amount NUMBER := 0;
    v_voucher_id NUMBER;
    v_cart_count NUMBER;
BEGIN
    -- Create CART table if it doesn't exist
    BEGIN
        EXECUTE IMMEDIATE '
        CREATE TABLE CART (
            CARTID NUMBER PRIMARY KEY,
            USERID NUMBER NOT NULL,
            PRODUCTID NUMBER NOT NULL,
            QUANTITY NUMBER DEFAULT 1,
            DATEADDED DATE DEFAULT SYSDATE,
            CONSTRAINT FK_CART_USER FOREIGN KEY (USERID) REFERENCES USERS(USERID),
            CONSTRAINT FK_CART_PRODUCT FOREIGN KEY (PRODUCTID) REFERENCES PRODUCTS(PRODUCTID)
        )';
    EXCEPTION
        WHEN OTHERS THEN NULL; -- Table already exists
    END;

    -- Check if user has items in cart
    SELECT COUNT(*) INTO v_cart_count FROM CART WHERE USERID = p_user_id;

    IF v_cart_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20005, 'Cart is empty');
    END IF;

    -- Create order
    INSERT INTO ORDERS (
        USERID, ORDERDATE, STATUS, TOTALAMOUNT, SHIPPINGADDRESS, 
        BILLINGADDRESS, PAYMENTMETHOD, SHIPPINGMETHOD, NOTES
    ) VALUES (
        p_user_id, SYSDATE, 'Pending', 0, p_shipping_address, 
        p_billing_address, p_payment_method, p_shipping_method, p_order_notes
    ) RETURNING ORDERID INTO p_order_id;

    -- Apply voucher if provided
    IF p_voucher_code IS NOT NULL THEN
        BEGIN
            SELECT VOUCHERID, DISCOUNTAMOUNT 
            INTO v_voucher_id, v_discount_amount
            FROM VOUCHERS
            WHERE CODE = p_voucher_code
              AND ISACTIVE = 1
              AND (EXPIRYDATE IS NULL OR EXPIRYDATE > SYSDATE)
              AND (USAGELIMIT IS NULL OR USAGELIMIT > (
                SELECT COUNT(*) FROM VOUCHERUSAGE WHERE VOUCHERID = VOUCHERS.VOUCHERID
              ));

            -- Record voucher usage
            INSERT INTO VOUCHERUSAGE (VOUCHERID, ORDERID, USERID, DATE)
            VALUES (v_voucher_id, p_order_id, p_user_id, SYSDATE);

        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                v_discount_amount := 0;
        END;
    END IF;

    -- Transfer cart items to order details
    INSERT INTO ORDERDETAILS (
        ORDERID, PRODUCTID, QUANTITY, PRICE, PRODUCTNAME
    )
    SELECT 
        p_order_id, C.PRODUCTID, C.QUANTITY, P.PRICE, P.NAME
    FROM 
        CART C
        JOIN PRODUCTS P ON C.PRODUCTID = P.PRODUCTID
    WHERE 
        C.USERID = p_user_id;

    -- Calculate total amount
    SELECT NVL(SUM(PRICE * QUANTITY), 0) 
    INTO v_total_amount
    FROM ORDERDETAILS
    WHERE ORDERID = p_order_id;

    -- Apply discount
    v_total_amount := v_total_amount - v_discount_amount;
    IF v_total_amount < 0 THEN v_total_amount := 0; END IF;

    -- Update order total
    UPDATE ORDERS
    SET TOTALAMOUNT = v_total_amount
    WHERE ORDERID = p_order_id;

    -- Update product stock
    FOR item IN (SELECT PRODUCTID, QUANTITY FROM ORDERDETAILS WHERE ORDERID = p_order_id)
    LOOP
        UPDATE PRODUCTS
        SET STOCKQUANTITY = STOCKQUANTITY - item.QUANTITY
        WHERE PRODUCTID = item.PRODUCTID;
    END LOOP;

    -- Clear user's cart
    DELETE FROM CART WHERE USERID = p_user_id;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_CREATE_ORDER;

/
