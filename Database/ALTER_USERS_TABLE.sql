-- Add new columns to USERS table
ALTER TABLE USERS ADD (
    FIRSTNAME VARCHAR2(100),
    LASTNAME VARCHAR2(100),
    PHONENUMBER VARCHAR2(20),
    GOOGLE_ID VARCHAR2(100),
    IS_GOOGLE_AUTH NUMBER(1) DEFAULT 0,
    LOGIN_ATTEMPTS NUMBER(3) DEFAULT 0,
    LOCKOUT_UNTIL DATE
);

-- Add unique constraint to phone number
ALTER TABLE USERS ADD CONSTRAINT UQ_USERS_PHONENUMBER UNIQUE (PHONENUMBER);

-- Add unique constraint to Google ID
ALTER TABLE USERS ADD CONSTRAINT UQ_USERS_GOOGLE_ID UNIQUE (GOOGLE_ID);

-- Create index on Google ID for faster lookups
CREATE INDEX IDX_USERS_GOOGLE_ID ON USERS(GOOGLE_ID);

-- Create index on lockout field for faster lookups
CREATE INDEX IDX_USERS_LOCKOUT ON USERS(LOCKOUT_UNTIL);

-- Update existing stored procedures
-- Update PRC_USER_LOGIN to handle lockout and return new fields

CREATE OR REPLACE PROCEDURE PRC_USER_LOGIN (
    p_username IN VARCHAR2,
    p_password_out OUT VARCHAR2,
    p_user_id OUT NUMBER,
    p_user_role OUT VARCHAR2,
    p_email OUT VARCHAR2,
    p_first_name OUT VARCHAR2,
    p_last_name OUT VARCHAR2,
    p_phone_number OUT VARCHAR2,
    p_is_google_auth OUT NUMBER,
    p_is_locked OUT NUMBER,
    p_login_attempts OUT NUMBER,
    p_status OUT NUMBER,
    p_error_message OUT VARCHAR2
)
AS
BEGIN
    p_status := 0;
    p_is_locked := 0;
    
    -- Check if user exists
    BEGIN
        SELECT 
            USER_ID, 
            PASSWORD, 
            ROLE, 
            EMAIL, 
            FIRSTNAME, 
            LASTNAME, 
            PHONENUMBER,
            IS_GOOGLE_AUTH,
            LOGIN_ATTEMPTS,
            CASE 
                WHEN LOCKOUT_UNTIL IS NULL THEN 0
                WHEN LOCKOUT_UNTIL > SYSDATE THEN 1
                ELSE 0
            END AS IS_LOCKED
        INTO 
            p_user_id, 
            p_password_out, 
            p_user_role, 
            p_email, 
            p_first_name, 
            p_last_name, 
            p_phone_number,
            p_is_google_auth,
            p_login_attempts,
            p_is_locked
        FROM USERS
        WHERE USERNAME = p_username;
        
        -- User exists
        p_status := 1;
        
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_status := 0;
            p_error_message := 'User not found';
        WHEN OTHERS THEN
            p_status := 0;
            p_error_message := 'Error: ' || SQLERRM;
    END;
END;
/

-- Update PRC_USER_REGISTER to include new fields

CREATE OR REPLACE PROCEDURE PRC_USER_REGISTER (
    p_username IN VARCHAR2,
    p_password IN VARCHAR2,
    p_email IN VARCHAR2,
    p_role IN VARCHAR2,
    p_first_name IN VARCHAR2,
    p_last_name IN VARCHAR2,
    p_phone_number IN VARCHAR2,
    p_status OUT NUMBER,
    p_error_message OUT VARCHAR2
)
AS
    v_user_count NUMBER;
    v_phone_count NUMBER;
    v_user_id NUMBER;
BEGIN
    p_status := 0;
    
    -- Check if username already exists
    SELECT COUNT(*) INTO v_user_count FROM USERS WHERE USERNAME = p_username;
    
    IF v_user_count > 0 THEN
        p_error_message := 'Username already exists';
        RETURN;
    END IF;
    
    -- Check if phone number already exists (if provided)
    IF p_phone_number IS NOT NULL AND LENGTH(p_phone_number) > 0 THEN
        SELECT COUNT(*) INTO v_phone_count FROM USERS WHERE PHONENUMBER = p_phone_number;
        
        IF v_phone_count > 0 THEN
            p_error_message := 'Phone number already exists';
            RETURN;
        END IF;
    END IF;
    
    -- Insert new user
    INSERT INTO USERS (
        USERNAME, 
        PASSWORD, 
        EMAIL, 
        ROLE, 
        CREATED_DATE,
        FIRSTNAME,
        LASTNAME,
        PHONENUMBER,
        IS_GOOGLE_AUTH,
        LOGIN_ATTEMPTS
    ) VALUES (
        p_username, 
        p_password, 
        p_email, 
        p_role, 
        SYSDATE,
        p_first_name,
        p_last_name,
        p_phone_number,
        0,
        0
    ) RETURNING USER_ID INTO v_user_id;
    
    -- Success
    p_status := 1;
    
EXCEPTION
    WHEN OTHERS THEN
        p_status := 0;
        p_error_message := 'Error: ' || SQLERRM;
END;
/

-- Create stored procedure for Google authentication

CREATE OR REPLACE PROCEDURE PRC_GOOGLE_AUTH_LOGIN (
    p_google_id IN VARCHAR2,
    p_email IN VARCHAR2,
    p_first_name IN VARCHAR2,
    p_last_name IN VARCHAR2,
    p_username IN VARCHAR2,
    p_user_id OUT NUMBER,
    p_user_role OUT VARCHAR2,
    p_status OUT NUMBER,
    p_error_message OUT VARCHAR2
)
AS
    v_user_count NUMBER;
    v_username VARCHAR2(50);
BEGIN
    p_status := 0;
    
    -- Check if user with this Google ID exists
    SELECT COUNT(*) INTO v_user_count FROM USERS WHERE GOOGLE_ID = p_google_id;
    
    IF v_user_count > 0 THEN
        -- User exists, get details
        SELECT USER_ID, ROLE INTO p_user_id, p_user_role
        FROM USERS
        WHERE GOOGLE_ID = p_google_id;
        
        -- Update last login
        UPDATE USERS
        SET LAST_LOGIN = SYSDATE
        WHERE USER_ID = p_user_id;
        
        p_status := 1;
    ELSE
        -- Check if email exists
        SELECT COUNT(*) INTO v_user_count FROM USERS WHERE EMAIL = p_email;
        
        IF v_user_count > 0 THEN
            -- Link Google ID to existing account
            UPDATE USERS
            SET GOOGLE_ID = p_google_id,
                IS_GOOGLE_AUTH = 1,
                LAST_LOGIN = SYSDATE
            WHERE EMAIL = p_email
            RETURNING USER_ID, ROLE INTO p_user_id, p_user_role;
            
            p_status := 1;
        ELSE
            -- Create new user
            v_username := p_username;
            
            -- Generate username if not provided
            IF v_username IS NULL THEN
                v_username := REGEXP_REPLACE(p_email, '@.*$', '');
            END IF;
            
            -- Check if username exists
            SELECT COUNT(*) INTO v_user_count FROM USERS WHERE USERNAME = v_username;
            
            -- If username exists, append random number
            IF v_user_count > 0 THEN
                v_username := v_username || DBMS_RANDOM.STRING('X', 4);
            END IF;
            
            -- Insert new user
            INSERT INTO USERS (
                USERNAME,
                PASSWORD,
                EMAIL,
                ROLE,
                CREATED_DATE,
                GOOGLE_ID,
                IS_GOOGLE_AUTH,
                FIRSTNAME,
                LASTNAME,
                LOGIN_ATTEMPTS,
                LAST_LOGIN
            ) VALUES (
                v_username,
                DBMS_RANDOM.STRING('X', 20), -- Random password since auth is via Google
                p_email,
                'Customer', -- Default role
                SYSDATE,
                p_google_id,
                1,
                p_first_name,
                p_last_name,
                0,
                SYSDATE
            ) RETURNING USER_ID INTO p_user_id;
            
            p_user_role := 'Customer';
            p_status := 1;
        END IF;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        p_status := 0;
        p_error_message := 'Error: ' || SQLERRM;
END;
/ 