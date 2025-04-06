--------------------------------------------------------
--  Ref Constraints for Table ORDERS
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."ORDERS" ADD CONSTRAINT "FK_ORDERS_USER" FOREIGN KEY ("USERID")
	  REFERENCES "AARON_IPT"."USERS" ("USERID") ON DELETE CASCADE ENABLE;
