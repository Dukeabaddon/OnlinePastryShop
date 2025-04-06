--------------------------------------------------------
--  DDL for Procedure GET_INVENTORY_ALERTS
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."GET_INVENTORY_ALERTS" (
    p_low_stock_cursor OUT SYS_REFCURSOR,
    p_low_stock_count OUT NUMBER
) AS
BEGIN
    -- Get count of low stock items
    SELECT COUNT(*) 
    INTO p_low_stock_count
    FROM PRODUCTS
    WHERE STOCKQUANTITY > 0 
    AND STOCKQUANTITY <= 10
    AND ISACTIVE = 1;

    -- Open cursor for low stock items details
    OPEN p_low_stock_cursor FOR
    SELECT 
        PRODUCTID,
        NAME AS PRODUCT,
        STOCKQUANTITY AS STOCK
    FROM PRODUCTS
    WHERE STOCKQUANTITY > 0 
    AND STOCKQUANTITY <= 10
    AND ISACTIVE = 1
    ORDER BY STOCKQUANTITY ASC;
END;

/
