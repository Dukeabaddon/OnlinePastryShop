--------------------------------------------------------
--  DDL for Trigger TRG_UPDATE_ORDER_TOTAL
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."TRG_UPDATE_ORDER_TOTAL" 
AFTER INSERT OR UPDATE OR DELETE ON ORDERDETAILS
FOR EACH ROW
DECLARE
    v_order_id NUMBER;
BEGIN
    -- Determine which order needs updating
    IF INSERTING OR UPDATING THEN
        v_order_id := :NEW.ORDERID;
    ELSE -- DELETING
        v_order_id := :OLD.ORDERID;
    END IF;

    -- Update the order total
    UPDATE ORDERS
    SET TOTALAMOUNT = (
        SELECT NVL(SUM(PRICE), 0)
        FROM ORDERDETAILS
        WHERE ORDERID = v_order_id
        AND ISACTIVE = 1
    )
    WHERE ORDERID = v_order_id;
END;

/
ALTER TRIGGER "AARON_IPT"."TRG_UPDATE_ORDER_TOTAL" ENABLE;
