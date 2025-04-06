--------------------------------------------------------
--  DDL for Function GET_RECENT_ACTIVITY
--------------------------------------------------------

  CREATE OR REPLACE FUNCTION "AARON_IPT"."GET_RECENT_ACTIVITY" (
    p_limit IN NUMBER DEFAULT 10
) RETURN SYS_REFCURSOR AS
    v_result SYS_REFCURSOR;
BEGIN
    OPEN v_result FOR
        SELECT * FROM (
            SELECT
                'order' AS ActivityType,
                O.ORDERID AS ID,
                U.USERNAME AS Username,
                'placed an order for ' || TO_CHAR(O.TOTALAMOUNT, 'FM$999,999,990.00') AS Description,
                O.ORDERDATE AS ActivityDate
            FROM
                ORDERS O
                JOIN USERS U ON O.USERID = U.USERID
            WHERE
                O.STATUS NOT IN ('Cancelled', 'Rejected')

            UNION ALL

            SELECT
                'review' AS ActivityType,
                PR.RATINGID AS ID,
                U.USERNAME AS Username,
                'reviewed ' || P.NAME || ' with ' || PR.RATING || ' stars' AS Description,
                PR.DATESUBMITTED AS ActivityDate
            FROM
                PRODUCTRATINGS PR
                JOIN USERS U ON PR.USERID = U.USERID
                JOIN PRODUCTS P ON PR.PRODUCTID = P.PRODUCTID
            WHERE
                PR.ISAPPROVED = 1

            UNION ALL

            SELECT
                'registration' AS ActivityType,
                U.USERID AS ID,
                U.USERNAME AS Username,
                'registered an account' AS Description,
                U.DATE AS ActivityDate
            FROM
                USERS U

            ORDER BY ActivityDate DESC
        ) WHERE ROWNUM <= p_limit;

    RETURN v_result;
END GET_RECENT_ACTIVITY;

/
