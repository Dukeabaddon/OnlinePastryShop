--------------------------------------------------------
--  DDL for Trigger WISHLIST_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."WISHLIST_TRG" 
BEFORE INSERT ON WISHLIST
FOR EACH ROW
BEGIN
    SELECT WISHLIST_SEQ.NEXTVAL INTO :NEW.WISHLISTID FROM DUAL;
END;

/
ALTER TRIGGER "AARON_IPT"."WISHLIST_TRG" ENABLE;
