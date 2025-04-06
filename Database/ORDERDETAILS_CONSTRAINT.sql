--------------------------------------------------------
--  Constraints for Table ORDERDETAILS
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."ORDERDETAILS" ADD PRIMARY KEY ("ORDERDETAILID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM"  ENABLE;
  ALTER TABLE "AARON_IPT"."ORDERDETAILS" MODIFY ("PRICE" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."ORDERDETAILS" MODIFY ("QUANTITY" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."ORDERDETAILS" MODIFY ("PRODUCTID" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."ORDERDETAILS" MODIFY ("ORDERID" NOT NULL ENABLE);
