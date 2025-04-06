--------------------------------------------------------
--  DDL for Trigger NEWSLETTER_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."NEWSLETTER_TRG" 
    BEFORE INSERT ON NEWSLETTER
    FOR EACH ROW
    BEGIN
        IF :NEW.ID IS NULL THEN
            SELECT NEWSLETTER_SEQ.NEXTVAL INTO :NEW.ID FROM DUAL;
        END IF;
    END;
/
ALTER TRIGGER "AARON_IPT"."NEWSLETTER_TRG" ENABLE;
