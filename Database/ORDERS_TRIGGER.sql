--------------------------------------------------------
--  DDL for Trigger ORDERS_TRIGGER
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."ORDERS_TRIGGER" 
BEFORE INSERT ON ORDERS
FOR EACH ROW
BEGIN
    SELECT ORDERS_SEQ.NEXTVAL INTO :NEW.ORDERID FROM DUAL;
END;

/
ALTER TRIGGER "AARON_IPT"."ORDERS_TRIGGER" ENABLE;
