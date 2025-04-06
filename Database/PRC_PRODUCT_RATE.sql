--------------------------------------------------------
--  DDL for Procedure PRC_PRODUCT_RATE
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_PRODUCT_RATE" (
    p_product_id IN NUMBER,
    p_user_id IN NUMBER,
    p_rating IN NUMBER,
    p_review IN CLOB DEFAULT NULL,
    p_rating_id OUT NUMBER
) AS
    v_existing_rating NUMBER;
    v_avg_rating NUMBER;
BEGIN
    -- Create PRODUCTRATINGS table if it doesn't exist
    BEGIN
        EXECUTE IMMEDIATE '
        CREATE TABLE PRODUCTRATINGS (
            RATINGID NUMBER PRIMARY KEY,
            PRODUCTID NUMBER NOT NULL,
            USERID NUMBER NOT NULL,
            RATING NUMBER(1) NOT NULL,
            REVIEW CLOB,
            DATESUBMITTED DATE DEFAULT SYSDATE,
            ISAPPROVED NUMBER(1) DEFAULT 0,
            CONSTRAINT FK_RATING_PRODUCT FOREIGN KEY (PRODUCTID) REFERENCES PRODUCTS(PRODUCTID),
            CONSTRAINT FK_RATING_USER FOREIGN KEY (USERID) REFERENCES USERS(USERID)
        )';
    EXCEPTION
        WHEN OTHERS THEN NULL; -- Table already exists
    END;

    -- Check if user has already rated this product
    BEGIN
        SELECT RATINGID INTO v_existing_rating
        FROM PRODUCTRATINGS
        WHERE PRODUCTID = p_product_id AND USERID = p_user_id;

        -- Update existing rating
        UPDATE PRODUCTRATINGS
        SET RATING = p_rating,
            REVIEW = p_review,
            DATESUBMITTED = SYSDATE,
            ISAPPROVED = 0  -- Reset approval status for moderation
        WHERE RATINGID = v_existing_rating;

        p_rating_id := v_existing_rating;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            -- Insert new rating
            INSERT INTO PRODUCTRATINGS (
                PRODUCTID, USERID, RATING, REVIEW, DATESUBMITTED, ISAPPROVED
            ) VALUES (
                p_product_id, p_user_id, p_rating, p_review, SYSDATE, 0
            ) RETURNING RATINGID INTO p_rating_id;
    END;

    -- Calculate average rating without updating product table
    SELECT AVG(RATING) INTO v_avg_rating
    FROM PRODUCTRATINGS 
    WHERE PRODUCTID = p_product_id AND ISAPPROVED = 1;

    -- Output to log for debugging
    DBMS_OUTPUT.PUT_LINE('Product ' || p_product_id || ' has average rating of ' || v_avg_rating);

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_PRODUCT_RATE;

/
