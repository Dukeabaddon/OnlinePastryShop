--------------------------------------------------------
--  DDL for Procedure PRC_PRODUCT_UPDATE_STOCK
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_PRODUCT_UPDATE_STOCK" (
    p_product_id IN NUMBER,
    p_quantity_change IN NUMBER,
    p_reason IN VARCHAR2,
    p_user_id IN NUMBER
) AS
    v_current_stock NUMBER;
    v_new_stock NUMBER;
    v_low_threshold NUMBER;
BEGIN
    -- Get current stock and threshold
    SELECT STOCKQUANTITY, LOWSTOCKTHRESHOLD 
    INTO v_current_stock, v_low_threshold
    FROM PRODUCTS 
    WHERE PRODUCTID = p_product_id;

    -- Calculate new stock level
    v_new_stock := v_current_stock + p_quantity_change;

    -- Prevent negative stock
    IF v_new_stock < 0 THEN
        RAISE_APPLICATION_ERROR(-20004, 'Stock cannot be negative');
    END IF;

    -- Update product stock
    UPDATE PRODUCTS
    SET STOCKQUANTITY = v_new_stock,
        DATE = SYSDATE  -- Using DATE instead of DATEUPDATED
    WHERE PRODUCTID = p_product_id;

    -- Create STOCK_MOVEMENTS table if it doesn't exist
    BEGIN
        EXECUTE IMMEDIATE '
        CREATE TABLE STOCK_MOVEMENTS (
            MOVEMENTID NUMBER PRIMARY KEY,
            PRODUCTID NUMBER NOT NULL,
            ADJUSTMENTDATE DATE DEFAULT SYSDATE,
            PREVIOUSSTOCK NUMBER,
            NEWSTOCK NUMBER,
            ADJUSTMENTQUANTITY NUMBER,
            REASON VARCHAR2(200),
            USERID NUMBER,
            CONSTRAINT FK_STOCKMOVE_PRODUCT FOREIGN KEY (PRODUCTID) REFERENCES PRODUCTS(PRODUCTID),
            CONSTRAINT FK_STOCKMOVE_USER FOREIGN KEY (USERID) REFERENCES USERS(USERID)
        )';
    EXCEPTION
        WHEN OTHERS THEN NULL; -- Table already exists
    END;

    -- Record stock movement
    INSERT INTO STOCK_MOVEMENTS (
        PRODUCTID, ADJUSTMENTDATE, PREVIOUSSTOCK, NEWSTOCK, 
        ADJUSTMENTQUANTITY, REASON, USERID
    ) VALUES (
        p_product_id, SYSDATE, v_current_stock, v_new_stock, 
        p_quantity_change, p_reason, p_user_id
    );

    -- Create STOCKALERTS table if it doesn't exist (already done in PRC_PRODUCT_ADD)

    -- Check if stock is now below threshold
    IF v_new_stock <= v_low_threshold AND v_current_stock > v_low_threshold THEN
        INSERT INTO STOCKALERTS (PRODUCTID, ALERTDATE, STOCKLEVEL, ISRESOLVED)
        VALUES (p_product_id, SYSDATE, v_new_stock, 0);
    END IF;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_PRODUCT_UPDATE_STOCK;

/
