--------------------------------------------------------
--  DDL for Trigger TRG_ORDER_STATUS_TRACK
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."TRG_ORDER_STATUS_TRACK" 
   AFTER UPDATE OF STATUS ON ORDERS
   FOR EACH ROW
   BEGIN
     INSERT INTO ORDER_STATUS_HISTORY (ORDERID, OLDSTATUS, NEWSTATUS, CHANGEDATE, CHANGEDBY)
     VALUES (:NEW.ORDERID, :OLD.STATUS, :NEW.STATUS, SYSDATE, USER);

     -- Update the status change date
     UPDATE ORDERS SET STATUSCHANGEDATE = SYSDATE WHERE ORDERID = :NEW.ORDERID;
   END;

/
ALTER TRIGGER "AARON_IPT"."TRG_ORDER_STATUS_TRACK" DISABLE;
