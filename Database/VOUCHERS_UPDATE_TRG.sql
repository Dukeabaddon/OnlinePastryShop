--------------------------------------------------------
--  DDL for Trigger VOUCHERS_UPDATE_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."VOUCHERS_UPDATE_TRG" 
BEFORE UPDATE ON AARON_IPT.VOUCHERS
FOR EACH ROW
BEGIN
    :NEW.MODIFIED_DATE := SYSDATE;
END;

/
ALTER TRIGGER "AARON_IPT"."VOUCHERS_UPDATE_TRG" ENABLE;
