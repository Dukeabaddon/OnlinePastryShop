--------------------------------------------------------
--  Ref Constraints for Table VOUCHERUSAGE
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."VOUCHERUSAGE" ADD CONSTRAINT "FK_VOUCHERUSAGE_ORDER" FOREIGN KEY ("ORDERID")
	  REFERENCES "AARON_IPT"."ORDERS" ("ORDERID") ON DELETE CASCADE ENABLE;
  ALTER TABLE "AARON_IPT"."VOUCHERUSAGE" ADD CONSTRAINT "FK_VOUCHERUSAGE_USER" FOREIGN KEY ("USERID")
	  REFERENCES "AARON_IPT"."USERS" ("USERID") ON DELETE CASCADE ENABLE;
  ALTER TABLE "AARON_IPT"."VOUCHERUSAGE" ADD CONSTRAINT "FK_VOUCHERUSAGE_VOUCHER" FOREIGN KEY ("VOUCHERID")
	  REFERENCES "AARON_IPT"."VOUCHERS" ("VOUCHERID") ON DELETE CASCADE ENABLE;
