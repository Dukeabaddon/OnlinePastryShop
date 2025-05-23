--------------------------------------------------------
--  DDL for Trigger PRODUCTRATINGS_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."PRODUCTRATINGS_TRG" 
BEFORE INSERT ON PRODUCTRATINGS
FOR EACH ROW
BEGIN
    SELECT PRODUCTRATINGS_SEQ.NEXTVAL INTO :NEW.RATINGID FROM DUAL;
END;

/
ALTER TRIGGER "AARON_IPT"."PRODUCTRATINGS_TRG" ENABLE;
