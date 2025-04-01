-- Simplified Database Reconstruction Script (FIXED)
-- Created for: Online Pastry Shop (Ocakes)
-- Username: Aaron_IPT
-- Password: qwen123
-- Date: Current Date
--
-- This script handles already existing data by checking before insertion

-- Clear existing data (if needed)
-- Uncomment these if you want to start with empty tables
/*
DELETE FROM ORDERDETAILS;
DELETE FROM ORDERS;
DELETE FROM PRODUCTCATEGORIES;
DELETE FROM PRODUCTS;
DELETE FROM CATEGORIES;
DELETE FROM USERS;
COMMIT;
*/

-- Step 1: Insert Users (check if they exist first)
DECLARE
  user_exists NUMBER;
BEGIN
  -- Check if admin exists
  SELECT COUNT(*) INTO user_exists FROM USERS WHERE USERNAME = 'admin';
  IF user_exists = 0 THEN
    INSERT INTO USERS (USERNAME, PASSWORDHASH, EMAIL, ROLE, LASTLOGIN, ISACTIVE) 
    VALUES ('admin', 'hashed_password_here', 'admin@ocakes.com', 'Admin', SYSDATE-3, 1);
    DBMS_OUTPUT.PUT_LINE('Admin user created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Admin user already exists');
  END IF;
  
  -- Check if johndoe exists
  SELECT COUNT(*) INTO user_exists FROM USERS WHERE USERNAME = 'johndoe';
  IF user_exists = 0 THEN
    INSERT INTO USERS (USERNAME, PASSWORDHASH, EMAIL, ROLE, LASTLOGIN, ISACTIVE) 
    VALUES ('johndoe', 'hashed_password_here', 'john.doe@example.com', 'Customer', SYSDATE-2, 1);
    DBMS_OUTPUT.PUT_LINE('johndoe user created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('johndoe user already exists');
  END IF;
  
  -- Check if janesmith exists
  SELECT COUNT(*) INTO user_exists FROM USERS WHERE USERNAME = 'janesmith';
  IF user_exists = 0 THEN
    INSERT INTO USERS (USERNAME, PASSWORDHASH, EMAIL, ROLE, LASTLOGIN, ISACTIVE) 
    VALUES ('janesmith', 'hashed_password_here', 'jane.smith@example.com', 'Customer', SYSDATE-1, 1);
    DBMS_OUTPUT.PUT_LINE('janesmith user created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('janesmith user already exists');
  END IF;
  
  -- Check if mikebrown exists
  SELECT COUNT(*) INTO user_exists FROM USERS WHERE USERNAME = 'mikebrown';
  IF user_exists = 0 THEN
    INSERT INTO USERS (USERNAME, PASSWORDHASH, EMAIL, ROLE, LASTLOGIN, ISACTIVE) 
    VALUES ('mikebrown', 'hashed_password_here', 'mike.brown@example.com', 'Customer', SYSDATE-5, 1);
    DBMS_OUTPUT.PUT_LINE('mikebrown user created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('mikebrown user already exists');
  END IF;
  
  -- Check if sarahlee exists
  SELECT COUNT(*) INTO user_exists FROM USERS WHERE USERNAME = 'sarahlee';
  IF user_exists = 0 THEN
    INSERT INTO USERS (USERNAME, PASSWORDHASH, EMAIL, ROLE, LASTLOGIN, ISACTIVE) 
    VALUES ('sarahlee', 'hashed_password_here', 'sarah.lee@example.com', 'Customer', SYSDATE-7, 1);
    DBMS_OUTPUT.PUT_LINE('sarahlee user created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('sarahlee user already exists');
  END IF;
END;
/

COMMIT;

-- Step 2: Insert Categories (check if they exist first)
DECLARE
  category_exists NUMBER;
  cakes_id NUMBER;
BEGIN
  -- Main categories
  -- Check if Cakes exists
  SELECT COUNT(*) INTO category_exists FROM CATEGORIES WHERE NAME = 'Cakes';
  IF category_exists = 0 THEN
    INSERT INTO CATEGORIES (NAME, DESCRIPTION, PARENTCATEGORYID, ISACTIVE)
    VALUES ('Cakes', 'All varieties of cakes', NULL, 1);
    DBMS_OUTPUT.PUT_LINE('Cakes category created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Cakes category already exists');
  END IF;
  
  -- Check if Cupcakes exists
  SELECT COUNT(*) INTO category_exists FROM CATEGORIES WHERE NAME = 'Cupcakes';
  IF category_exists = 0 THEN
    INSERT INTO CATEGORIES (NAME, DESCRIPTION, PARENTCATEGORYID, ISACTIVE)
    VALUES ('Cupcakes', 'Individual cupcakes and mini desserts', NULL, 1);
    DBMS_OUTPUT.PUT_LINE('Cupcakes category created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Cupcakes category already exists');
  END IF;
  
  -- Check if Pastries exists
  SELECT COUNT(*) INTO category_exists FROM CATEGORIES WHERE NAME = 'Pastries';
  IF category_exists = 0 THEN
    INSERT INTO CATEGORIES (NAME, DESCRIPTION, PARENTCATEGORYID, ISACTIVE)
    VALUES ('Pastries', 'Flaky and sweet pastry items', NULL, 1);
    DBMS_OUTPUT.PUT_LINE('Pastries category created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Pastries category already exists');
  END IF;
  
  -- Check if Seasonal Specials exists
  SELECT COUNT(*) INTO category_exists FROM CATEGORIES WHERE NAME = 'Seasonal Specials';
  IF category_exists = 0 THEN
    INSERT INTO CATEGORIES (NAME, DESCRIPTION, PARENTCATEGORYID, ISACTIVE)
    VALUES ('Seasonal Specials', 'Limited time seasonal offerings', NULL, 1);
    DBMS_OUTPUT.PUT_LINE('Seasonal Specials category created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Seasonal Specials category already exists');
  END IF;
  
  -- Get the Cakes category ID for subcategories
  SELECT CATEGORYID INTO cakes_id FROM CATEGORIES WHERE NAME = 'Cakes';
  
  -- Check if Birthday Cakes exists
  SELECT COUNT(*) INTO category_exists FROM CATEGORIES WHERE NAME = 'Birthday Cakes';
  IF category_exists = 0 THEN
    INSERT INTO CATEGORIES (NAME, DESCRIPTION, PARENTCATEGORYID, ISACTIVE)
    VALUES ('Birthday Cakes', 'Special cakes for birthday celebrations', cakes_id, 1);
    DBMS_OUTPUT.PUT_LINE('Birthday Cakes subcategory created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Birthday Cakes subcategory already exists');
  END IF;
  
  -- Check if Wedding Cakes exists
  SELECT COUNT(*) INTO category_exists FROM CATEGORIES WHERE NAME = 'Wedding Cakes';
  IF category_exists = 0 THEN
    INSERT INTO CATEGORIES (NAME, DESCRIPTION, PARENTCATEGORYID, ISACTIVE)
    VALUES ('Wedding Cakes', 'Elegant multi-tier cakes for weddings', cakes_id, 1);
    DBMS_OUTPUT.PUT_LINE('Wedding Cakes subcategory created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Wedding Cakes subcategory already exists');
  END IF;
END;
/

COMMIT;

-- Step 3: Insert Products (check if they exist first)
DECLARE
  product_exists NUMBER;
BEGIN
  -- Check if Chocolate Fudge Cake exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Chocolate Fudge Cake';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Chocolate Fudge Cake', 'Rich chocolate cake with fudge frosting', 125.25, 25, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Chocolate Fudge Cake product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Chocolate Fudge Cake product already exists');
  END IF;
  
  -- Check if Vanilla Bean Cake exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Vanilla Bean Cake';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Vanilla Bean Cake', 'Light vanilla sponge with vanilla bean frosting', 140.25, 20, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Vanilla Bean Cake product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Vanilla Bean Cake product already exists');
  END IF;
  
  -- Check if Red Velvet Cake exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Red Velvet Cake';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Red Velvet Cake', 'Classic red velvet with cream cheese frosting', 100.00, 15, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Red Velvet Cake product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Red Velvet Cake product already exists');
  END IF;
  
  -- Check if Strawberry Cheesecake exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Strawberry Cheesecake';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Strawberry Cheesecake', 'Creamy cheesecake with fresh strawberry topping', 49.88, 8, 0, 1);
    DBMS_OUTPUT.PUT_LINE('Strawberry Cheesecake product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Strawberry Cheesecake product already exists');
  END IF;
  
  -- Check if Blueberry Muffins exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Blueberry Muffins (6 pack)';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Blueberry Muffins (6 pack)', 'Moist muffins loaded with fresh blueberries', 35.50, 30, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Blueberry Muffins product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Blueberry Muffins product already exists');
  END IF;
  
  -- Check if Chocolate Chip Cookies exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Chocolate Chip Cookies (12 pack)';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Chocolate Chip Cookies (12 pack)', 'Classic cookies with semi-sweet chocolate chunks', 30.00, 40, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Chocolate Chip Cookies product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Chocolate Chip Cookies product already exists');
  END IF;
  
  -- Check if Tiramisu Cake exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Tiramisu Cake';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Tiramisu Cake', 'Italian dessert with espresso and mascarpone', 150.75, 5, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Tiramisu Cake product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Tiramisu Cake product already exists');
  END IF;
  
  -- Check if Carrot Cake exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Carrot Cake';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Carrot Cake', 'Spiced carrot cake with cream cheese frosting', 175.25, 12, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Carrot Cake product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Carrot Cake product already exists');
  END IF;
  
  -- Check if French Croissants exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'French Croissants (4 pack)';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('French Croissants (4 pack)', 'Buttery and flaky authentic croissants', 42.00, 22, 1, 1);
    DBMS_OUTPUT.PUT_LINE('French Croissants product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('French Croissants product already exists');
  END IF;
  
  -- Check if Custom Birthday Cake exists
  SELECT COUNT(*) INTO product_exists FROM PRODUCTS WHERE NAME = 'Custom Birthday Cake';
  IF product_exists = 0 THEN
    INSERT INTO PRODUCTS (NAME, DESCRIPTION, PRICE, STOCKQUANTITY, ISLATEST, ISACTIVE)
    VALUES ('Custom Birthday Cake', 'Personalized cake for special celebrations', 250.00, 0, 1, 1);
    DBMS_OUTPUT.PUT_LINE('Custom Birthday Cake product created');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Custom Birthday Cake product already exists');
  END IF;
END;
/

COMMIT;

-- Step 4: Link Products to Categories
-- First, check if ProductCategory connection already exists to avoid duplicates
DECLARE
  PROCEDURE link_product_category(p_product_name VARCHAR2, p_category_name VARCHAR2) IS
    product_id NUMBER;
    category_id NUMBER;
    link_exists NUMBER;
  BEGIN
    -- Get the product ID
    BEGIN
      SELECT PRODUCTID INTO product_id FROM PRODUCTS WHERE NAME = p_product_name;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Product not found: ' || p_product_name);
        RETURN;
    END;
    
    -- Get the category ID
    BEGIN
      SELECT CATEGORYID INTO category_id FROM CATEGORIES WHERE NAME = p_category_name;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Category not found: ' || p_category_name);
        RETURN;
    END;
    
    -- Check if link already exists
    SELECT COUNT(*) INTO link_exists 
    FROM PRODUCTCATEGORIES 
    WHERE PRODUCTID = product_id AND CATEGORYID = category_id;
    
    IF link_exists = 0 THEN
      INSERT INTO PRODUCTCATEGORIES (PRODUCTID, CATEGORYID)
      VALUES (product_id, category_id);
      DBMS_OUTPUT.PUT_LINE('Linked ' || p_product_name || ' to ' || p_category_name);
    ELSE
      DBMS_OUTPUT.PUT_LINE('Link already exists between ' || p_product_name || ' and ' || p_category_name);
    END IF;
  END;
BEGIN
  -- Link products to Cakes category
  link_product_category('Chocolate Fudge Cake', 'Cakes');
  link_product_category('Vanilla Bean Cake', 'Cakes');
  link_product_category('Red Velvet Cake', 'Cakes');
  link_product_category('Strawberry Cheesecake', 'Cakes');
  link_product_category('Tiramisu Cake', 'Cakes');
  link_product_category('Carrot Cake', 'Cakes');
  
  -- Link products to Cupcakes category
  link_product_category('Blueberry Muffins (6 pack)', 'Cupcakes');
  
  -- Link products to Pastries category
  link_product_category('Chocolate Chip Cookies (12 pack)', 'Pastries');
  link_product_category('French Croissants (4 pack)', 'Pastries');
  
  -- Link products to Birthday Cakes subcategory
  link_product_category('Chocolate Fudge Cake', 'Birthday Cakes');
  link_product_category('Custom Birthday Cake', 'Birthday Cakes');
  
  -- Link products to Seasonal Specials category
  link_product_category('Tiramisu Cake', 'Seasonal Specials');
END;
/

COMMIT;

-- Step 5: Create Orders
-- First create the most recent orders with proper status
DECLARE
  PROCEDURE create_order(p_username VARCHAR2, p_days_ago NUMBER, p_amount NUMBER, p_status VARCHAR2) IS
    user_id NUMBER;
    order_exists NUMBER := 0;
  BEGIN
    -- Get user ID
    BEGIN
      SELECT USERID INTO user_id FROM USERS WHERE USERNAME = p_username;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('User not found: ' || p_username);
        RETURN;
    END;
    
    -- Check if an order already exists at this exact date for this user
    SELECT COUNT(*) INTO order_exists
    FROM ORDERS
    WHERE USERID = user_id
    AND TRUNC(ORDERDATE) = TRUNC(SYSDATE - p_days_ago);
    
    IF order_exists = 0 THEN
      INSERT INTO ORDERS (USERID, ORDERDATE, TOTALAMOUNT, ISACTIVE, STATUS)
      VALUES (user_id, SYSDATE - p_days_ago, p_amount, 1, p_status);
      DBMS_OUTPUT.PUT_LINE('Created order for ' || p_username || ' from ' || p_days_ago || ' days ago with status ' || p_status);
    ELSE
      DBMS_OUTPUT.PUT_LINE('Order already exists for ' || p_username || ' from ' || p_days_ago || ' days ago');
    END IF;
  END;
BEGIN
  -- Create orders for johndoe
  create_order('johndoe', 0, 275.25, 'Pending');
  create_order('johndoe', 1, 270.50, 'Delivered');
  create_order('johndoe', 8, 320.00, 'Delivered');
  
  -- Create orders for janesmith
  create_order('janesmith', 0, 350.50, 'Processing');
  create_order('janesmith', 4, 550.25, 'Delivered');
  create_order('janesmith', 20, 420.75, 'Delivered');
  
  -- Create orders for mikebrown
  create_order('mikebrown', 1, 125.25, 'Delivered');
  create_order('mikebrown', 5, 180.50, 'Delivered');
  
  -- Create orders for sarahlee
  create_order('sarahlee', 3, 225.00, 'Delivered');
  create_order('sarahlee', 10, 205.25, 'Delivered');
END;
/

COMMIT;

-- Step 6: Add products to orders
DECLARE
  PROCEDURE add_to_order(p_username VARCHAR2, p_product_name VARCHAR2, p_days_ago NUMBER, p_quantity NUMBER) IS
    user_id NUMBER;
    order_id NUMBER;
    product_id NUMBER;
    product_price NUMBER;
    detail_exists NUMBER := 0;
  BEGIN
    -- Get user ID
    BEGIN
      SELECT USERID INTO user_id FROM USERS WHERE USERNAME = p_username;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('User not found: ' || p_username);
        RETURN;
    END;
    
    -- Get order ID
    BEGIN
      SELECT ORDERID INTO order_id
      FROM ORDERS
      WHERE USERID = user_id
      AND TRUNC(ORDERDATE) = TRUNC(SYSDATE - p_days_ago)
      AND ROWNUM = 1;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Order not found for ' || p_username || ' from ' || p_days_ago || ' days ago');
        RETURN;
    END;
    
    -- Get product info
    BEGIN
      SELECT PRODUCTID, PRICE INTO product_id, product_price
      FROM PRODUCTS
      WHERE NAME = p_product_name;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Product not found: ' || p_product_name);
        RETURN;
    END;
    
    -- Check if this product is already in this order
    SELECT COUNT(*) INTO detail_exists
    FROM ORDERDETAILS
    WHERE ORDERID = order_id
    AND PRODUCTID = product_id;
    
    IF detail_exists = 0 THEN
      INSERT INTO ORDERDETAILS (ORDERID, PRODUCTID, QUANTITY, PRICE)
      VALUES (order_id, product_id, p_quantity, product_price);
      DBMS_OUTPUT.PUT_LINE('Added ' || p_quantity || ' of ' || p_product_name || ' to order for ' || p_username);
    ELSE
      DBMS_OUTPUT.PUT_LINE('Product ' || p_product_name || ' already in order for ' || p_username);
    END IF;
  END;
BEGIN
  -- Add products to johndoe's pending order
  add_to_order('johndoe', 'Chocolate Fudge Cake', 0, 2);
  add_to_order('johndoe', 'Strawberry Cheesecake', 0, 1);
  
  -- Add products to johndoe's delivered order
  add_to_order('johndoe', 'Tiramisu Cake', 1, 1);
  add_to_order('johndoe', 'French Croissants (4 pack)', 1, 3);
  
  -- Add products to janesmith's processing order
  add_to_order('janesmith', 'Red Velvet Cake', 0, 1);
  add_to_order('janesmith', 'Carrot Cake', 0, 1);
  add_to_order('janesmith', 'Blueberry Muffins (6 pack)', 0, 2);
  
  -- Add products to mikebrown's delivered order
  add_to_order('mikebrown', 'Chocolate Fudge Cake', 1, 1);
  
  -- Add products to sarahlee's delivered order
  add_to_order('sarahlee', 'Red Velvet Cake', 3, 1);
  add_to_order('sarahlee', 'Chocolate Chip Cookies (12 pack)', 3, 4);
END;
/

COMMIT;

-- Confirm successful completion
BEGIN
  DBMS_OUTPUT.PUT_LINE('-----------------------------------------------');
  DBMS_OUTPUT.PUT_LINE('Database reconstruction completed successfully.');
  DBMS_OUTPUT.PUT_LINE('All duplicate records were handled gracefully.');
  DBMS_OUTPUT.PUT_LINE('-----------------------------------------------');
END;
/ 