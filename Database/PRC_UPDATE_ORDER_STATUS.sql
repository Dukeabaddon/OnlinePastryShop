--------------------------------------------------------
--  DDL for Procedure PRC_UPDATE_ORDER_STATUS
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_UPDATE_ORDER_STATUS" (
    p_order_id IN NUMBER,
    p_new_status IN VARCHAR2,
    p_user_id IN NUMBER,
    p_notes IN VARCHAR2 DEFAULT NULL
) AS
    v_current_status VARCHAR2(50);
    v_customer_id NUMBER;
    v_customer_email VARCHAR2(100);
BEGIN
    -- Create ORDER_STATUS_HISTORY table if it doesn't exist
    BEGIN
        EXECUTE IMMEDIATE '
        CREATE TABLE ORDER_STATUS_HISTORY (
            HISTORYID NUMBER PRIMARY KEY,
            ORDERID NUMBER NOT NULL,
            OLDSTATUS VARCHAR2(50),
            NEWSTATUS VARCHAR2(50),
            CHANGEDATE DATE DEFAULT SYSDATE,
            CHANGEDBY NUMBER,
            NOTES VARCHAR2(500),
            CONSTRAINT FK_ORDERHIST_ORDER FOREIGN KEY (ORDERID) REFERENCES ORDERS(ORDERID),
            CONSTRAINT FK_ORDERHIST_USER FOREIGN KEY (CHANGEDBY) REFERENCES USERS(USERID)
        )';
    EXCEPTION
        WHEN OTHERS THEN NULL; -- Table already exists
    END;

    -- Get current status and customer info
    SELECT O.STATUS, O.USERID, U.EMAIL
    INTO v_current_status, v_customer_id, v_customer_email
    FROM ORDERS O
    JOIN USERS U ON O.USERID = U.USERID
    WHERE O.ORDERID = p_order_id;

    -- Record status change in history
    INSERT INTO ORDER_STATUS_HISTORY (
        ORDERID, OLDSTATUS, NEWSTATUS, CHANGEDATE, CHANGEDBY, NOTES
    ) VALUES (
        p_order_id, v_current_status, p_new_status, SYSDATE, p_user_id, p_notes
    );

    -- Update order status
    UPDATE ORDERS
    SET STATUS = p_new_status,
        DATE = SYSDATE  -- Using DATE instead of STATUSCHANGEDATE
    WHERE ORDERID = p_order_id;

    -- Add DATEDELIVERED column if it doesn't exist
    BEGIN
        EXECUTE IMMEDIATE 'ALTER TABLE ORDERS ADD (DATEDELIVERED DATE)';
    EXCEPTION
        WHEN OTHERS THEN NULL; -- Column already exists
    END;

    -- Special handling for specific statuses
    IF p_new_status = 'Delivered' THEN
        UPDATE ORDERS
        SET DATEDELIVERED = SYSDATE
        WHERE ORDERID = p_order_id;
    ELSIF p_new_status = 'Cancelled' THEN
        -- Return items to inventory
        FOR item IN (SELECT PRODUCTID, QUANTITY FROM ORDERDETAILS WHERE ORDERID = p_order_id)
        LOOP
            UPDATE PRODUCTS
            SET STOCKQUANTITY = STOCKQUANTITY + item.QUANTITY
            WHERE PRODUCTID = item.PRODUCTID;
        END LOOP;
    END IF;

    COMMIT;

    -- Send notification logic would go here in a real implementation

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_UPDATE_ORDER_STATUS;

/
