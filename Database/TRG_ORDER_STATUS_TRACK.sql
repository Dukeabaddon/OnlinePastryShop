--------------------------------------------------------
--  DDL for Trigger TRG_ORDER_STATUS_TRACK
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."TRG_ORDER_STATUS_TRACK" 
AFTER UPDATE OF STATUS ON ORDERS
FOR EACH ROW
BEGIN
    IF :NEW.STATUS <> :OLD.STATUS THEN
        -- Store the status change history (implement as needed)
        NULL;
    END IF;
END;

/
ALTER TRIGGER "AARON_IPT"."TRG_ORDER_STATUS_TRACK" ENABLE;
