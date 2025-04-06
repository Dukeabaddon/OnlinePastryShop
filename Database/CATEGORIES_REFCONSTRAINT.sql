--------------------------------------------------------
--  Ref Constraints for Table CATEGORIES
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."CATEGORIES" ADD CONSTRAINT "FK_PARENT_CATEGORY" FOREIGN KEY ("PARENTCATEGORYID")
	  REFERENCES "AARON_IPT"."CATEGORIES" ("CATEGORYID") ON DELETE CASCADE ENABLE;
