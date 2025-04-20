# Oracle 11g Database Schema

## Connection Information
- **Host**: localhost
- **Port**: 1521
- **Service**: xe
- **Username**: zen
- **Password**: qwen123

## Core Tables

### USERS Table
```sql
CREATE TABLE USERS (
    user_id NUMBER PRIMARY KEY,
    username VARCHAR2(50) NOT NULL UNIQUE,
    password VARCHAR2(64) NOT NULL, -- For storing hashed passwords
    email VARCHAR2(100) NOT NULL UNIQUE,
    first_name VARCHAR2(50) NOT NULL,
    last_name VARCHAR2(50) NOT NULL,
    user_type VARCHAR2(20) NOT NULL CHECK (user_type IN ('admin', 'customer', 'staff')),
    address VARCHAR2(200),
    phone VARCHAR2(20),
    created_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    last_login TIMESTAMP,
    status VARCHAR2(10) DEFAULT 'active' CHECK (status IN ('active', 'inactive', 'blocked'))
);
```

### PRODUCTS Table
```sql
CREATE TABLE PRODUCTS (
    product_id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    description VARCHAR2(500),
    price NUMBER(10,2) NOT NULL,
    category VARCHAR2(50) NOT NULL,
    stock_quantity NUMBER DEFAULT 0,
    image_url VARCHAR2(200),
    created_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    updated_date TIMESTAMP,
    status VARCHAR2(10) DEFAULT 'active' CHECK (status IN ('active', 'inactive', 'discontinued'))
);
```

### ORDERS Table
```sql
CREATE TABLE ORDERS (
    order_id NUMBER PRIMARY KEY,
    user_id NUMBER NOT NULL,
    total_amount NUMBER(10,2) NOT NULL,
    status VARCHAR2(20) DEFAULT 'pending' CHECK (status IN ('pending', 'processing', 'shipped', 'delivered', 'cancelled')),
    payment_method VARCHAR2(20) NOT NULL,
    payment_status VARCHAR2(20) DEFAULT 'pending' CHECK (payment_status IN ('pending', 'completed', 'failed', 'refunded')),
    delivery_address VARCHAR2(200) NOT NULL,
    delivery_date TIMESTAMP,
    created_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    updated_date TIMESTAMP,
    CONSTRAINT fk_orders_users FOREIGN KEY (user_id) REFERENCES USERS(user_id)
);
```

### ORDER_DETAILS Table
```sql
CREATE TABLE ORDER_DETAILS (
    order_detail_id NUMBER PRIMARY KEY,
    order_id NUMBER NOT NULL,
    product_id NUMBER NOT NULL,
    quantity NUMBER NOT NULL,
    unit_price NUMBER(10,2) NOT NULL,
    CONSTRAINT fk_orderdetails_orders FOREIGN KEY (order_id) REFERENCES ORDERS(order_id),
    CONSTRAINT fk_orderdetails_products FOREIGN KEY (product_id) REFERENCES PRODUCTS(product_id)
);
```

### FEEDBACK Table
```sql
CREATE TABLE FEEDBACK (
    feedback_id NUMBER PRIMARY KEY,
    user_id NUMBER NOT NULL,
    product_id NUMBER NOT NULL,
    rating NUMBER(1) NOT NULL CHECK (rating BETWEEN 1 AND 5),
    feedback_comment VARCHAR2(500),
    created_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT fk_feedback_users FOREIGN KEY (user_id) REFERENCES USERS(user_id),
    CONSTRAINT fk_feedback_products FOREIGN KEY (product_id) REFERENCES PRODUCTS(product_id)
);
```

## Additional Tables

### AUDIT_LOG Table
```sql
CREATE TABLE AUDIT_LOG (
    log_id NUMBER PRIMARY KEY,
    table_name VARCHAR2(50) NOT NULL,
    operation VARCHAR2(10) NOT NULL CHECK (operation IN ('INSERT', 'UPDATE', 'DELETE')),
    record_id VARCHAR2(50) NOT NULL,
    changed_by NUMBER, -- User ID of who made the change
    old_values CLOB,
    new_values CLOB,
    change_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT fk_auditlog_users FOREIGN KEY (changed_by) REFERENCES USERS(user_id)
);
```

### INVENTORY_LOG Table
```sql
CREATE TABLE INVENTORY_LOG (
    log_id NUMBER PRIMARY KEY,
    product_id NUMBER NOT NULL,
    quantity_change NUMBER NOT NULL, -- Positive for additions, negative for reductions
    reason VARCHAR2(100), -- e.g., 'order', 'restock', 'adjustment', 'return'
    reference_id VARCHAR2(50), -- Could be an order_id, etc.
    created_by NUMBER,
    created_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT fk_inventorylog_products FOREIGN KEY (product_id) REFERENCES PRODUCTS(product_id),
    CONSTRAINT fk_inventorylog_users FOREIGN KEY (created_by) REFERENCES USERS(user_id)
);
```

### SETTINGS Table
```sql
CREATE TABLE SETTINGS (
    setting_key VARCHAR2(50) PRIMARY KEY,
    setting_value VARCHAR2(500) NOT NULL,
    setting_description VARCHAR2(200),
    created_date TIMESTAMP DEFAULT SYSTIMESTAMP,
    updated_date TIMESTAMP
);
```

## Sequences for ID Generation

```sql
CREATE SEQUENCE user_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE product_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE order_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE order_detail_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE feedback_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE log_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE inventory_log_id_seq START WITH 1 INCREMENT BY 1;
```

## Triggers

### ID Generation Triggers
```sql
-- User ID Trigger
CREATE OR REPLACE TRIGGER trg_users_before_insert
BEFORE INSERT ON USERS
FOR EACH ROW
BEGIN
    SELECT user_id_seq.NEXTVAL INTO :NEW.user_id FROM DUAL;
END;
/

-- Similar triggers for other tables (PRODUCTS, ORDERS, etc.)
```

### Audit Logging Triggers
```sql
-- Products Audit Trigger
CREATE OR REPLACE TRIGGER trg_products_audit
AFTER INSERT OR UPDATE OR DELETE ON PRODUCTS
FOR EACH ROW
DECLARE
    v_operation VARCHAR2(10);
    v_old_values CLOB;
    v_new_values CLOB;
BEGIN
    -- Determine operation type
    IF INSERTING THEN
        v_operation := 'INSERT';
        v_old_values := NULL;
        v_new_values := 'ID=' || :NEW.product_id || ', NAME=' || :NEW.name || 
                        ', PRICE=' || :NEW.price || ', STOCK=' || :NEW.stock_quantity;
    ELSIF UPDATING THEN
        v_operation := 'UPDATE';
        v_old_values := 'ID=' || :OLD.product_id || ', NAME=' || :OLD.name || 
                        ', PRICE=' || :OLD.price || ', STOCK=' || :OLD.stock_quantity;
        v_new_values := 'ID=' || :NEW.product_id || ', NAME=' || :NEW.name || 
                        ', PRICE=' || :NEW.price || ', STOCK=' || :NEW.stock_quantity;
    ELSIF DELETING THEN
        v_operation := 'DELETE';
        v_old_values := 'ID=' || :OLD.product_id || ', NAME=' || :OLD.name || 
                        ', PRICE=' || :OLD.price || ', STOCK=' || :OLD.stock_quantity;
        v_new_values := NULL;
    END IF;
    
    -- Insert audit record
    INSERT INTO AUDIT_LOG (
        log_id, table_name, operation, record_id, 
        changed_by, old_values, new_values
    )
    VALUES (
        log_id_seq.NEXTVAL, 'PRODUCTS', v_operation, 
        CASE 
            WHEN v_operation IN ('UPDATE', 'DELETE') THEN TO_CHAR(:OLD.product_id)
            ELSE TO_CHAR(:NEW.product_id)
        END,
        NULL, -- This would be the user ID in a real application
        v_old_values, v_new_values
    );
END;
/

-- Similar audit triggers would be created for other important tables
```

## Views for Reporting

```sql
-- Active Users View
CREATE OR REPLACE VIEW VW_ACTIVE_USERS AS
SELECT user_id, username, email, first_name, last_name, user_type, 
       created_date, last_login
FROM USERS
WHERE status = 'active';

-- Product Inventory View
CREATE OR REPLACE VIEW VW_PRODUCT_INVENTORY AS
SELECT p.product_id, p.name, p.category, p.price, p.stock_quantity,
       p.status, p.created_date, p.updated_date
FROM PRODUCTS p
ORDER BY p.category, p.name;

-- Low Stock Products View
CREATE OR REPLACE VIEW VW_LOW_STOCK_PRODUCTS AS
SELECT product_id, name, category, stock_quantity
FROM PRODUCTS
WHERE stock_quantity < 20 AND status = 'active'
ORDER BY stock_quantity ASC;

-- Monthly Sales Report View
CREATE OR REPLACE VIEW VW_MONTHLY_SALES AS
SELECT 
    EXTRACT(YEAR FROM o.created_date) AS year,
    EXTRACT(MONTH FROM o.created_date) AS month,
    COUNT(DISTINCT o.order_id) AS total_orders,
    SUM(o.total_amount) AS total_revenue
FROM ORDERS o
WHERE o.status != 'cancelled'
GROUP BY EXTRACT(YEAR FROM o.created_date), EXTRACT(MONTH FROM o.created_date)
ORDER BY year DESC, month DESC;

-- Product Sales Report View
CREATE OR REPLACE VIEW VW_PRODUCT_SALES AS
SELECT 
    p.product_id, p.name, p.category,
    COUNT(od.order_detail_id) AS times_ordered,
    SUM(od.quantity) AS total_units_sold,
    SUM(od.quantity * od.unit_price) AS total_revenue
FROM PRODUCTS p
LEFT JOIN ORDER_DETAILS od ON p.product_id = od.product_id
LEFT JOIN ORDERS o ON od.order_id = o.order_id AND o.status != 'cancelled'
GROUP BY p.product_id, p.name, p.category
ORDER BY total_units_sold DESC;
```