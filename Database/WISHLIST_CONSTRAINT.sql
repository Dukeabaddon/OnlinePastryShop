--------------------------------------------------------
--  Constraints for Table WISHLIST
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."WISHLIST" ADD PRIMARY KEY ("WISHLISTID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM"  ENABLE;
  ALTER TABLE "AARON_IPT"."WISHLIST" MODIFY ("PRODUCTID" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."WISHLIST" MODIFY ("USERID" NOT NULL ENABLE);
