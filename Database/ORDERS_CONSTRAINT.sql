--------------------------------------------------------
--  Constraints for Table ORDERS
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."ORDERS" ADD PRIMARY KEY ("ORDERID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM"  ENABLE;
  ALTER TABLE "AARON_IPT"."ORDERS" MODIFY ("STATUS" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."ORDERS" MODIFY ("TOTALAMOUNT" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."ORDERS" MODIFY ("USERID" NOT NULL ENABLE);
