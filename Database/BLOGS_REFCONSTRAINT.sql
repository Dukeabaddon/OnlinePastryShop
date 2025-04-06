--------------------------------------------------------
--  Ref Constraints for Table BLOGS
--------------------------------------------------------

  ALTER TABLE "AARON_IPT"."BLOGS" ADD CONSTRAINT "FK_BLOG_AUTHOR" FOREIGN KEY ("AUTHORID")
	  REFERENCES "AARON_IPT"."USERS" ("USERID") ENABLE;
  ALTER TABLE "AARON_IPT"."BLOGS" ADD CONSTRAINT "FK_BLOG_CATEGORY" FOREIGN KEY ("CATEGORYID")
	  REFERENCES "AARON_IPT"."CATEGORIES" ("CATEGORYID") ENABLE;
