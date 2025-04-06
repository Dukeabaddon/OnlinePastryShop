--------------------------------------------------------
--  Ref Constraints for Table NEWSLETTER
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."NEWSLETTER" ADD CONSTRAINT "FK_NEWSLETTER_USER" FOREIGN KEY ("USERID")
	  REFERENCES "AARON_IPT"."USERS" ("USERID") ENABLE;
