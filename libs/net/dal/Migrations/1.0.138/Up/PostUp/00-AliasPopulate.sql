DO $$
BEGIN
    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Andrew Macleod"' WHERE "name" = 'Andrew Macleod';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Andrew Macleod.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Dirk Meissner"' WHERE "name" = 'Dirk Meissner';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Dirk Meissner.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Jonathon Bartlett"' WHERE "name" = 'Jonathon Bartlett';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Jonathon Bartlett.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Justine Hunter"' WHERE "name" = 'Justine Hunter';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Justine Hunter.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Keith Baldrey"' WHERE "name" = 'Keith Baldrey';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Keith Baldrey.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Kylie Stanton"' WHERE "name" = 'Kylie Stanton';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Kylie Stanton.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Les Leyne"' WHERE "name" = 'Les Leyne';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Les Leyne.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Mike McArthur"' WHERE "name" = 'Mike McArthur';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Mike McArthur.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Mike Smyth"' WHERE "name" = 'Mike Smyth';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Mike Smyth.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Richard Zussman"' WHERE "name" = 'Richard Zussman';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Richard Zussman.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Rob Shaw"' WHERE "name" = 'Rob Shaw';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Rob Shaw.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Robert Buffam"' WHERE "name" = 'Robert Buffam';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Robert Buffam.';
    END;

    BEGIN
        UPDATE public."contributor" SET "aliases" = '"Vaughn Palmer"' WHERE "name" = 'Vaughn Palmer';
    EXCEPTION WHEN undefined_column THEN
        RAISE NOTICE 'Column alias does not exist for Vaughn Palmer.';
    END;
END $$;
