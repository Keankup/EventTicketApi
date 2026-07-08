-- хранимые процедуры и функции для event_db, выполнять после 01_schema.sql и 02_seed_data.sql
 
-- билеты на мероприятие с фильтром по статусу
CREATE OR REPLACE FUNCTION fn_get_tickets_by_event(
    p_event_id INTEGER,
    p_status   VARCHAR DEFAULT NULL
)
RETURNS TABLE (
    ticket_id       INTEGER,
    ticket_type     VARCHAR,
    seat_number     VARCHAR,
    status          VARCHAR,
    price           NUMERIC,
    customer_name   TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        t.ticket_id,
        tt.name AS ticket_type,
        t.seat_number,
        t.status,
        t.price,
        (c.first_name || ' ' || c.last_name) AS customer_name
    FROM tickets t
    JOIN ticket_types tt ON tt.ticket_type_id = t.ticket_type_id
    LEFT JOIN customers c ON c.customer_id = t.customer_id
    WHERE t.event_id = p_event_id
      AND (p_status IS NULL OR t.status = p_status)
    ORDER BY t.ticket_id;
END;
$$;

COMMENT ON FUNCTION fn_get_tickets_by_event IS
    'Возвращает билеты мероприятия, опционально отфильтрованные по статусу';

-- статистика продаж по мероприятию
CREATE OR REPLACE FUNCTION fn_get_event_sales_stats(p_event_id INTEGER)
RETURNS TABLE (
    event_id        INTEGER,
    event_name      VARCHAR,
    total_tickets   BIGINT,
    sold_tickets    BIGINT,
    available_tickets BIGINT,
    reserved_tickets BIGINT,
    total_revenue   NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        e.event_id,
        e.name,
        COUNT(t.ticket_id) AS total_tickets,
        COUNT(t.ticket_id) FILTER (WHERE t.status = 'Sold') AS sold_tickets,
        COUNT(t.ticket_id) FILTER (WHERE t.status = 'Available') AS available_tickets,
        COUNT(t.ticket_id) FILTER (WHERE t.status = 'Reserved') AS reserved_tickets,
        COALESCE(SUM(t.price) FILTER (WHERE t.status = 'Sold'), 0) AS total_revenue
    FROM events e
    LEFT JOIN tickets t ON t.event_id = e.event_id
    WHERE e.event_id = p_event_id
    GROUP BY e.event_id, e.name;
END;
$$;

COMMENT ON FUNCTION fn_get_event_sales_stats IS
    'Возвращает сводную статистику продаж билетов по мероприятию';

-- ближайшие мероприятия за N дней
CREATE OR REPLACE FUNCTION fn_get_upcoming_events(p_days_ahead INTEGER DEFAULT 30)
RETURNS TABLE (
    event_id       INTEGER,
    name           VARCHAR,
    venue_name     VARCHAR,
    start_datetime TIMESTAMP,
    status         VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        e.event_id,
        e.name,
        v.name AS venue_name,
        e.start_datetime,
        e.status
    FROM events e
    JOIN venues v ON v.venue_id = e.venue_id
    WHERE e.start_datetime BETWEEN now() AND now() + (p_days_ahead || ' days')::INTERVAL
      AND e.status <> 'Cancelled'
    ORDER BY e.start_datetime;
END;
$$;

COMMENT ON FUNCTION fn_get_upcoming_events IS
    'Возвращает мероприятия, стартующие в ближайшие N дней';

-- продажа билета с проверкой доступности
CREATE OR REPLACE PROCEDURE sp_sell_ticket(
    p_ticket_id   INTEGER,
    p_customer_id INTEGER
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_status VARCHAR;
BEGIN
    SELECT status INTO v_status
    FROM tickets
    WHERE ticket_id = p_ticket_id
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Билет с id % не найден', p_ticket_id;
    END IF;

    IF v_status <> 'Available' THEN
        RAISE EXCEPTION 'Билет % недоступен для продажи (текущий статус: %)', p_ticket_id, v_status;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM customers WHERE customer_id = p_customer_id) THEN
        RAISE EXCEPTION 'Покупатель с id % не найден', p_customer_id;
    END IF;

    UPDATE tickets
    SET status = 'Sold',
        customer_id = p_customer_id,
        purchase_date = now()
    WHERE ticket_id = p_ticket_id;

    COMMIT;
END;
$$;

COMMENT ON PROCEDURE sp_sell_ticket IS
    'Оформляет продажу билета конкретному покупателю с проверкой доступности';

-- отмена билета, освобождает место
CREATE OR REPLACE PROCEDURE sp_cancel_ticket(p_ticket_id INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE
    v_status VARCHAR;
BEGIN
    SELECT status INTO v_status
    FROM tickets
    WHERE ticket_id = p_ticket_id
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Билет с id % не найден', p_ticket_id;
    END IF;

    IF v_status = 'Available' THEN
        RAISE EXCEPTION 'Билет % уже свободен, отменять нечего', p_ticket_id;
    END IF;

    UPDATE tickets
    SET status = 'Available',
        customer_id = NULL,
        purchase_date = NULL
    WHERE ticket_id = p_ticket_id;

    COMMIT;
END;
$$;

COMMENT ON PROCEDURE sp_cancel_ticket IS
    'Отменяет проданный/забронированный билет и делает его снова доступным';

