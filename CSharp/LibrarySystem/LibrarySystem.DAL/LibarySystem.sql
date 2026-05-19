
drop function sp_get_books_currently_borrowed();

CREATE OR REPLACE FUNCTION sp_get_books_currently_borrowed()
RETURNS TABLE (
    borrowing_id INT,
    member_name TEXT,
    book_title TEXT,
    author TEXT,
    copy_code TEXT,
    borrowed_at TIMESTAMP WITH TIME ZONE,
    due_date TIMESTAMP WITH TIME ZONE
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        br."BorrowingId",
        m."Name",
        b."Title",
        b."Author",
        bc."CopyCode",
        br."BorrowedAt",
        br."DueDate"
    FROM "Borrowings" br
    JOIN "Members" m ON br."MemberId" = m."MemberId"
    JOIN "BookCopies" bc ON br."CopyId" = bc."CopyId"
    JOIN "Books" b ON bc."BookId" = b."BookId"
    WHERE br."Status" = 1; 
END;
$$;

select * from sp_get_books_currently_borrowed();


CREATE OR REPLACE FUNCTION sp_get_overdue_books_by_member(p_member_id INT default NULL)
RETURNS TABLE (
    borrowing_id INT,
    member_name TEXT,
    member_email TEXT,
    book_title TEXT,
    copy_code TEXT,
    borrowed_at TIMESTAMP WITH TIME ZONE,
    due_date TIMESTAMP WITH TIME ZONE,
    days_overdue INT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        br."BorrowingId",
        m."Name",
        m."Email",
        b."Title",
        bc."CopyCode",
        br."BorrowedAt",
        br."DueDate",
        EXTRACT(DAY FROM NOW() - br."DueDate")::INT AS days_overdue
    FROM "Borrowings" br
    JOIN "Members" m ON br."MemberId" = m."MemberId"
    JOIN "BookCopies" bc ON br."CopyId" = bc."CopyId"
    JOIN "Books" b ON bc."BookId" = b."BookId"
    WHERE br."Status" = 1
      AND br."DueDate" < NOW()
      AND br."MemberId" = p_member_id;
END;
$$;

SELECT * FROM sp_get_overdue_books_by_member();


drop function sp_get_members_with_pending_fines();

CREATE OR REPLACE FUNCTION sp_get_members_with_pending_fines()
RETURNS TABLE (
    member_id INT,
    member_name TEXT,
    member_email TEXT,
    total_fines BIGINT,
    total_amount NUMERIC
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        m."MemberId",
        m."Name",
        m."Email",
        COUNT(f."FineId"),
        SUM(f."Amount")
    FROM "Fines" f
    JOIN "Members" m ON f."MemberId" = m."MemberId"
    WHERE f."IsPaid" = FALSE
    GROUP BY m."MemberId", m."Name", m."Email"
    ORDER BY SUM(f."Amount") DESC;
END;
$$;

SELECT * FROM sp_get_members_with_pending_fines();


CREATE OR REPLACE FUNCTION sp_get_most_borrowed_books(p_limit INT DEFAULT 10)
RETURNS TABLE (
    book_id INT,
    title TEXT,
    author TEXT,
    isbn TEXT,
    category_name TEXT,
    borrow_count BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        b."BookId",
        b."Title",
        b."Author",
        b."ISBN",
        cat."CategoryName",
        COUNT(br."BorrowingId") AS borrow_count
    FROM "Books" b
    JOIN "BookCategories" cat ON b."CategoryId" = cat."CategoryId"
    JOIN "BookCopies" bc ON bc."BookId" = b."BookId"
    JOIN "Borrowings" br ON br."CopyId" = bc."CopyId"
    GROUP BY b."BookId", b."Title", b."Author", b."ISBN", cat."CategoryName"
    ORDER BY borrow_count DESC
    LIMIT p_limit;
END;
$$;

SELECT * FROM sp_get_most_borrowed_books(5);


CREATE OR REPLACE FUNCTION sp_get_available_books_by_category(p_category_id INT DEFAULT NULL)
RETURNS TABLE (
    category_name TEXT,
    book_id INT,
    title TEXT,
    author TEXT,
    isbn TEXT,
    available_copies BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        cat."CategoryName",
        b."BookId",
        b."Title",
        b."Author",
        b."ISBN",
        COUNT(bc."CopyId") AS available_copies
    FROM "Books" b
    JOIN "BookCategories" cat ON b."CategoryId" = cat."CategoryId"
    JOIN "BookCopies" bc ON bc."BookId" = b."BookId"
    WHERE bc."Status" = 1  -- Available
      AND (p_category_id IS NULL OR cat."CategoryId" = p_category_id)
    GROUP BY cat."CategoryName", b."BookId", b."Title", b."Author", b."ISBN"
    ORDER BY cat."CategoryName", b."Title";
END;
$$;


SELECT * FROM sp_get_available_books_by_category();

SELECT * FROM sp_get_available_books_by_category();


CREATE OR REPLACE FUNCTION sp_get_member_borrowing_history(p_member_id INT)
RETURNS TABLE (
    borrowing_id INT,
    book_title TEXT,
    author TEXT,
    copy_code TEXT,
    borrowed_at TIMESTAMP WITH TIME ZONE,
    due_date TIMESTAMP WITH TIME ZONE,
    returned_at TIMESTAMP WITH TIME ZONE,
    status INT,
    fine_amount NUMERIC,
    fine_paid BOOLEAN
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        br."BorrowingId",
        b."Title",
        b."Author",
        bc."CopyCode",
        br."BorrowedAt",
        br."DueDate",
        br."ReturnedAt",
        br."Status",
        f."Amount",
        f."IsPaid"
    FROM "Borrowings" br
    JOIN "BookCopies" bc ON br."CopyId" = bc."CopyId"
    JOIN "Books" b ON bc."BookId" = b."BookId"
    LEFT JOIN "Fines" f ON f."BorrowingId" = br."BorrowingId"
    WHERE br."MemberId" = p_member_id
    ORDER BY br."BorrowedAt" DESC;
END;
$$;

SELECT * FROM sp_get_member_borrowing_history(8);
