--------------------------------------------------------
--  DDL for Function GET_SALES_BY_CATEGORY
--------------------------------------------------------

  CREATE OR REPLACE FUNCTION "AARON_IPT"."GET_SALES_BY_CATEGORY" (
    p_start_date IN DATE,
    p_end_date IN DATE
) RETURN SYS_REFCURSOR AS
    v_result SYS_REFCURSOR;
    v_total_revenue NUMBER;
BEGIN
    -- First calculate total revenue in a separate query
    SELECT NVL(SUM(OD.PRICE * OD.QUANTITY), 0)
    INTO v_total_revenue
    FROM ORDERDETAILS OD
    JOIN ORDERS O ON OD.ORDERID = O.ORDERID
    WHERE O.ORDERDATE BETWEEN p_start_date AND p_end_date
    AND O.STATUS NOT IN ('Cancelled', 'Rejected');

    -- Now use this value in the main query
    OPEN v_result FOR
        SELECT
            C.CATEGORYID,
            C.NAME AS CategoryName,
            COUNT(DISTINCT O.ORDERID) AS OrderCount,
            SUM(OD.QUANTITY) AS QuantitySold,
            NVL(SUM(OD.PRICE * OD.QUANTITY), 0) AS Revenue,
            CASE
                WHEN v_total_revenue > 0 THEN
                    ROUND(NVL(SUM(OD.PRICE * OD.QUANTITY), 0) / v_total_revenue * 100, 2)
                ELSE 0
            END AS PercentageOfTotal
        FROM
            CATEGORIES C
            JOIN PRODUCTCATEGORIES PC ON C.CATEGORYID = PC.CATEGORYID -- Corrected Join
            JOIN PRODUCTS P ON PC.PRODUCTID = P.PRODUCTID          -- Corrected Join
            JOIN ORDERDETAILS OD ON P.PRODUCTID = OD.PRODUCTID
            JOIN ORDERS O ON OD.ORDERID = O.ORDERID
        WHERE
            O.ORDERDATE BETWEEN p_start_date AND p_end_date
            AND O.STATUS NOT IN ('Cancelled', 'Rejected')
        GROUP BY
            C.CATEGORYID, C.NAME
        ORDER BY
            Revenue DESC; -- Order by calculated alias

    RETURN v_result;
END GET_SALES_BY_CATEGORY;

/
