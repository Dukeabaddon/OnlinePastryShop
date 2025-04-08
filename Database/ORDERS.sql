--------------------------------------------------------
--  DDL for Table ORDERS
--------------------------------------------------------

  CREATE TABLE "AARON_IPT"."ORDERS" 
   (	"ORDERID" NUMBER, 
	"USERID" NUMBER, 
	"ORDERDATE" DATE DEFAULT SYSDATE, 
	"TOTALAMOUNT" NUMBER(10,2), 
	"ISACTIVE" NUMBER(1,0) DEFAULT 1, 
	"STATUS" VARCHAR2(20 BYTE) DEFAULT 'Pending', 
	"SHIPPINGADDRESS" VARCHAR2(255 BYTE), 
	"PAYMENTMETHOD" VARCHAR2(50 BYTE) DEFAULT 'Cash on Delivery'
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM" ;

   COMMENT ON COLUMN "AARON_IPT"."ORDERS"."STATUS" IS 'Order status (Pending, Processing, Shipped, Delivered, Cancelled)';
