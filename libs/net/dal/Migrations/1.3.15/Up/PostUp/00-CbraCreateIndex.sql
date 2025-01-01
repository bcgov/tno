DO $$
BEGIN

CREATE INDEX IF NOT EXISTS "IX_content_published_on"
   ON public.content USING btree
   (published_on DESC);
CREATE INDEX IF NOT EXISTS "IX_content_status"
    ON public.content USING btree
    (status ASC);

END $$;
