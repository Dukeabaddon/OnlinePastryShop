--------------------------------------------------------
--  DDL for Procedure PRC_USER_RESET_PASSWORD
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_USER_RESET_PASSWORD" (
    p_email IN VARCHAR2,
    p_token OUT VARCHAR2
) AS
    v_user_id NUMBER;
    v_expiry_date DATE;
BEGIN
    -- Check if user exists
    BEGIN
        SELECT USERID INTO v_user_id FROM USERS WHERE EMAIL = p_email;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20003, 'User not found');
    END;

    -- Generate token and set expiry (24 hours)
    p_token := DBMS_RANDOM.STRING('X', 32);
    v_expiry_date := SYSDATE + 1;

    -- Update user with token
    UPDATE USERS 
    SET PASSWORDRESETTOKEN = p_token,
        TOKENEXPIRYDATE = v_expiry_date
    WHERE USERID = v_user_id;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_USER_RESET_PASSWORD;

/
