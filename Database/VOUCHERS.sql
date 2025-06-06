--------------------------------------------------------
--  DDL for Table VOUCHERS
--------------------------------------------------------

  CREATE TABLE "AARON_IPT"."VOUCHERS" 
   (	"VOUCHERID" NUMBER, 
	"CODE" VARCHAR2(50 BYTE), 
	"DISCOUNTTYPE" VARCHAR2(10 BYTE), 
	"VALUE" NUMBER(10,2), 
	"MINIMUMPURCHASE" NUMBER(10,2), 
	"MAXUSES" NUMBER, 
	"EXPIRYDATE" DATE, 
	"DATECREATED" DATE DEFAULT SYSDATE, 
	"DATEMODIFIED" DATE DEFAULT SYSDATE, 
	"ISACTIVE" NUMBER(1,0) DEFAULT 1
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM" ;
