--------------------------------------------------------
--  Ref Constraints for Table CATEGORIES
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."CATEGORIES" ADD FOREIGN KEY ("PARENTCATEGORYID")
	  REFERENCES "AARON_IPT"."CATEGORIES" ("CATEGORYID") ON DELETE CASCADE ENABLE;
