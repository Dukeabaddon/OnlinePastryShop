--------------------------------------------------------
--  DDL for Table VOUCHERUSAGE
--------------------------------------------------------

  CREATE TABLE "AARON_IPT"."VOUCHERUSAGE" 
   (	"VOUCHERUSAGEID" NUMBER, 
	"VOUCHERID" NUMBER, 
	"USERID" NUMBER, 
	"ORDERID" NUMBER, 
	"REDEEMEDAT" DATE DEFAULT SYSDATE, 
	"ISACTIVE" NUMBER(1,0) DEFAULT 1
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM" ;
