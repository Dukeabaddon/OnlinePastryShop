--------------------------------------------------------
--  DDL for Procedure PRC_USER_LOGIN
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_USER_LOGIN" (
    p_email IN VARCHAR2,
    p_password IN VARCHAR2,
    p_user_id OUT NUMBER,
    p_username OUT VARCHAR2,
    p_role OUT VARCHAR2,
    p_success OUT NUMBER
) AS
    v_stored_password VARCHAR2(100);
    v_failed_attempts NUMBER;
    v_account_status VARCHAR2(20);
BEGIN
    p_success := 0;

    -- Get user information without referencing PASSWORDSALT
    -- Assume PASSWORD already contains hashed value
    BEGIN
        SELECT USERID, USERNAME, PASSWORDHASH, ROLE, 
               NVL(FAILEDLOGINATTEMPTS, 0), NVL(ACCOUNTSTATUS, 'Active')
        INTO p_user_id, p_username, v_stored_password, p_role, 
             v_failed_attempts, v_account_status
        FROM USERS
        WHERE EMAIL = p_email;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_success := 0;
            RETURN;
    END;

    -- Check account status
    IF v_account_status != 'Active' THEN
        p_success := 0;
        RETURN;
    END IF;

    -- Check for locked account
    IF v_failed_attempts >= 5 THEN
        UPDATE USERS SET ACCOUNTSTATUS = 'Locked' WHERE EMAIL = p_email;
        p_success := 0;
        RETURN;
    END IF;

    -- Verify password directly (no hashing)
    IF v_stored_password = p_password THEN
        -- Login successful
        p_success := 1;

        -- Reset failed attempts and update last login
        UPDATE USERS 
        SET FAILEDLOGINATTEMPTS = 0,
            LASTLOGINDATE = SYSDATE
        WHERE USERID = p_user_id;
    ELSE
        -- Login failed
        p_success := 0;

        -- Increment failed attempts
        UPDATE USERS 
        SET FAILEDLOGINATTEMPTS = v_failed_attempts + 1
        WHERE USERID = p_user_id;
    END IF;

    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_USER_LOGIN;

/
