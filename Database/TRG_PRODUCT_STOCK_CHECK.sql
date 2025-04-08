--------------------------------------------------------
--  DDL for Trigger TRG_PRODUCT_STOCK_CHECK
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "AARON_IPT"."TRG_PRODUCT_STOCK_CHECK" 
AFTER UPDATE OF STOCKQUANTITY ON PRODUCTS
FOR EACH ROW
BEGIN
    IF :NEW.STOCKQUANTITY <= 10 AND :NEW.STOCKQUANTITY > 0 THEN
        -- Log low stock alert (implement as needed)
        NULL;
    ELSIF :NEW.STOCKQUANTITY = 0 THEN
        -- Log out of stock alert (implement as needed)
        NULL;
    END IF;
END;

/
ALTER TRIGGER "AARON_IPT"."TRG_PRODUCT_STOCK_CHECK" ENABLE;
