--------------------------------------------------------
--  DDL for Procedure PRC_USER_REGISTER
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "AARON_IPT"."PRC_USER_REGISTER" (
    p_username IN VARCHAR2,
    p_email IN VARCHAR2,
    p_password IN VARCHAR2,
    p_firstname IN VARCHAR2,
    p_lastname IN VARCHAR2,
    p_phonenumber IN VARCHAR2 DEFAULT NULL,
    p_address IN VARCHAR2 DEFAULT NULL,
    p_role IN VARCHAR2 DEFAULT 'Customer',
    p_newsletter IN NUMBER DEFAULT 0,
    p_user_id OUT NUMBER
) AS
    v_salt VARCHAR2(20);
    v_hashed_password VARCHAR2(100);
    v_email_count NUMBER;
    v_username_count NUMBER;
BEGIN
    -- Check if email already exists
    SELECT COUNT(*) INTO v_email_count FROM USERS WHERE EMAIL = p_email;
    IF v_email_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Email already exists');
    END IF;

    -- Check if username already exists
    SELECT COUNT(*) INTO v_username_count FROM USERS WHERE USERNAME = p_username;
    IF v_username_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Username already exists');
    END IF;

    -- Generate salt and simple hash for password (no DBMS_CRYPTO)
    v_salt := DBMS_RANDOM.STRING('A', 16);
    v_hashed_password := SUBSTR(p_password || v_salt, 1, 100); -- Simple concatenation

    -- Insert new user (using DATE column instead of CREATEDATE)
    INSERT INTO USERS (
        USERNAME, EMAIL, PASSWORD, SALT, FIRSTNAME, LASTNAME, 
        PHONENUMBER, ADDRESS, ROLE, DATE, ISACTIVE, NEWSLETTERSUBSCRIBED
    ) VALUES (
        p_username, p_email, v_hashed_password, v_salt, p_firstname, p_lastname,
        p_phonenumber, p_address, p_role, SYSDATE, 1, p_newsletter
    ) RETURNING USERID INTO p_user_id;

    COMMIT;

    -- Add to newsletter if subscribed
    IF p_newsletter = 1 THEN
        -- Create NEWSLETTER table if it doesn't exist
        BEGIN
            EXECUTE IMMEDIATE '
            CREATE TABLE NEWSLETTER (
                ID NUMBER PRIMARY KEY,
                EMAIL VARCHAR2(100) NOT NULL,
                ISACTIVE NUMBER(1) DEFAULT 1,
                USERID NUMBER,
                CONSTRAINT FK_NEWSLETTER_USER FOREIGN KEY (USERID) REFERENCES USERS(USERID)
            )';
        EXCEPTION
            WHEN OTHERS THEN NULL; -- Table already exists
        END;

        INSERT INTO NEWSLETTER (EMAIL, ISACTIVE, USERID)
        VALUES (p_email, 1, p_user_id);
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END PRC_USER_REGISTER;

/
