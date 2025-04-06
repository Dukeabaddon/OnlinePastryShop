--------------------------------------------------------
--  DDL for Function GET_TOP_SELLING_PRODUCTS
--------------------------------------------------------

  CREATE OR REPLACE FUNCTION "AARON_IPT"."GET_TOP_SELLING_PRODUCTS" (
    p_start_date IN DATE,
    p_end_date IN DATE,
    p_limit IN NUMBER
) RETURN SYS_REFCURSOR AS
    v_result SYS_REFCURSOR;
BEGIN
    OPEN v_result FOR
        SELECT * FROM (
            SELECT 
                p.PRODUCTID,
                p.NAME AS ProductName,
                NVL(SUM(od.QUANTITY), 0) AS QuantitySold,
                NVL(SUM(od.QUANTITY * od.PRICE), 0) AS Revenue
            FROM 
                PRODUCTS p
            LEFT JOIN 
                ORDERDETAILS od ON p.PRODUCTID = od.PRODUCTID
            LEFT JOIN 
                ORDERS o ON od.ORDERID = o.ORDERID
            WHERE 
                (o.ORDERDATE BETWEEN p_start_date AND p_end_date OR o.ORDERDATE IS NULL)
                AND (o.STATUS = 'Approved' OR o.STATUS IS NULL)
                AND p.ISACTIVE = 1
                AND (o.ISACTIVE = 1 OR o.ISACTIVE IS NULL)
            GROUP BY 
                p.PRODUCTID, p.NAME
            ORDER BY 
                QuantitySold DESC
        ) WHERE ROWNUM <= p_limit;

    RETURN v_result;
END;

/
