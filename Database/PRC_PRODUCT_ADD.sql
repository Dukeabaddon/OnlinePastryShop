--------------------------------------------------------
--  DDL for Procedure PRC_PRODUCT_ADD
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_PRODUCT_ADD" (
    p_name IN VARCHAR2,
    p_description IN CLOB,
    p_price IN NUMBER,
    p_costprice IN NUMBER,
    p_stockquantity IN NUMBER,
    p_category_id IN NUMBER, -- Changed from p_categoryid
    p_image IN VARCHAR2,
    p_shortdescription IN VARCHAR2 DEFAULT NULL,
    p_isfeatured IN NUMBER DEFAULT 0,
    p_isnew IN NUMBER DEFAULT 1,
    p_lowstockthreshold IN NUMBER DEFAULT 5,
    p_product_id OUT NUMBER
) AS
    v_seo_url VARCHAR2(200);
BEGIN
    -- Generate SEO-friendly URL
    v_seo_url := REGEXP_REPLACE(LOWER(p_name), '[^a-z0-9]+', '-');

    -- Insert the product
    INSERT INTO PRODUCTS (
        NAME, DESCRIPTION, PRICE, COSTPRICE, STOCKQUANTITY, 
        CATEGORY_ID, IMAGE, SHORTDESCRIPTION, ISFEATURED, 
        ISNEW, LOWSTOCKTHRESHOLD, SEO_URL, DATE, ISACTIVE
    ) VALUES (
        p_name, p_description, p_price, p_costprice, p_stockquantity, 
        p_category_id, p_image, p_shortdescription, p_isfeatured, 
        p_isnew, p_lowstockthreshold, v_seo_url, SYSDATE, 1
    ) RETURNING PRODUCTID INTO p_product_id;

    -- Create STOCKALERTS table if it doesn't exist
    BEGIN
        EXECUTE IMMEDIATE '
        CREATE TABLE STOCKALERTS (
            ALERTID NUMBER PRIMARY KEY,
            PRODUCTID NUMBER NOT NULL,
            ALERTDATE DATE DEFAULT SYSDATE,
            STOCKLEVEL NUMBER,
            ISRESOLVED NUMBER(1) DEFAULT 0,
            CONSTRAINT FK_STOCKALERT_PRODUCT FOREIGN KEY (PRODUCTID) REFERENCES PRODUCTS(PRODUCTID)
        )';
    EXCEPTION
        WHEN OTHERS THEN NULL; -- Table already exists
    END;

    -- Check if product is below threshold and create alert
    IF p_stockquantity <= p_lowstockthreshold THEN
        INSERT INTO STOCKALERTS (PRODUCTID, ALERTDATE, STOCKLEVEL, ISRESOLVED)
        VALUES (p_product_id, SYSDATE, p_stockquantity, 0);
    END IF;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_PRODUCT_ADD;

/
