-- calculate_member_fine(member_id)
-- Returns total unpaid fine for a given member
CREATE OR REPLACE FUNCTION calculate_member_fine(p_member_id INT)
RETURNS DECIMAL AS $$
BEGIN
    RETURN COALESCE(
        (SELECT SUM(amount - paid_amount)
         FROM fines
         WHERE member_id = p_member_id AND is_paid = FALSE),
        0
    );
END;
$$ LANGUAGE plpgsql;


-- get_available_books_by_category(category_id)
-- Returns available book copies under a specific category
CREATE OR REPLACE FUNCTION get_available_books_by_category(p_category_id INT)
RETURNS TABLE(
    isbn VARCHAR,
    title VARCHAR,
    author VARCHAR,
    copy_id INT,
    status VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT b.isbn, b.title, b.author, bc.copy_id, bc.status
    FROM books b
    INNER JOIN book_copies bc ON b.isbn = bc.isbn
    WHERE b.category_id = p_category_id
      AND bc.status IN ('Available', 'MinorDamage');
END;
$$ LANGUAGE plpgsql;


-- get_member_borrowing_summary(member_id)
-- Returns active borrowings, returned borrowings, and total unpaid fine
CREATE OR REPLACE FUNCTION get_member_borrowing_summary(p_member_id INT)
RETURNS TABLE(
    active_borrowings BIGINT,
    returned_borrowings BIGINT,
    total_unpaid_fine DECIMAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        (SELECT COUNT(*) FROM borrowings WHERE member_id = p_member_id AND status = 'Active'),
        (SELECT COUNT(*) FROM borrowings WHERE member_id = p_member_id AND status = 'Returned'),
        COALESCE(
            (SELECT SUM(amount - paid_amount) FROM fines WHERE member_id = p_member_id AND is_paid = FALSE),
            0
        );
END;
$$ LANGUAGE plpgsql;
