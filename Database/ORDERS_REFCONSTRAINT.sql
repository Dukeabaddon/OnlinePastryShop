--------------------------------------------------------
--  Ref Constraints for Table ORDERS
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."ORDERS" ADD FOREIGN KEY ("USERID")
	  REFERENCES "AARON_IPT"."USERS" ("USERID") ON DELETE CASCADE ENABLE;
