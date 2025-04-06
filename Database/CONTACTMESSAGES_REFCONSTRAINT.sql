--------------------------------------------------------
--  Ref Constraints for Table CONTACTMESSAGES
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."CONTACTMESSAGES" ADD CONSTRAINT "FK_CONTACT_USER" FOREIGN KEY ("USERID")
	  REFERENCES "AARON_IPT"."USERS" ("USERID") ENABLE;
