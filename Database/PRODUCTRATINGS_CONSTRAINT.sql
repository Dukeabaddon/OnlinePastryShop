--------------------------------------------------------
--  Constraints for Table PRODUCTRATINGS
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."PRODUCTRATINGS" ADD PRIMARY KEY ("RATINGID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1 BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE "SYSTEM"  ENABLE;
  ALTER TABLE "AARON_IPT"."PRODUCTRATINGS" ADD CONSTRAINT "CHK_RATING_VALUE" CHECK (RATING BETWEEN 1 AND 5) ENABLE;
  ALTER TABLE "AARON_IPT"."PRODUCTRATINGS" MODIFY ("RATING" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."PRODUCTRATINGS" MODIFY ("USERID" NOT NULL ENABLE);
  ALTER TABLE "AARON_IPT"."PRODUCTRATINGS" MODIFY ("PRODUCTID" NOT NULL ENABLE);
