
-- сумма выполненных заказов по каждому клиенту в день его рождения
CREATE OR REPLACE FUNCTION fn_orders_sum_on_birthday()
RETURNS TABLE (
    customer_id    INTEGER,
    first_name     VARCHAR,
    last_name      VARCHAR,
    birth_date     DATE,
    orders_count   BIGINT,
    orders_sum     NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        c.customer_id,
        c.first_name,
        c.last_name,
        c.birth_date,
        COUNT(t.ticket_id) AS orders_count,
        COALESCE(SUM(t.price), 0) AS orders_sum
    FROM customers c
    JOIN tickets t ON t.customer_id = c.customer_id
    WHERE t.status = 'Sold'
      AND EXTRACT(MONTH FROM t.purchase_date) = EXTRACT(MONTH FROM c.birth_date)
      AND EXTRACT(DAY FROM t.purchase_date)   = EXTRACT(DAY FROM c.birth_date)
    GROUP BY c.customer_id, c.first_name, c.last_name, c.birth_date
    ORDER BY orders_sum DESC;
END;
$$;

COMMENT ON FUNCTION fn_orders_sum_on_birthday IS
    'Сумма выполненных заказов по каждому клиенту, оформленных в день его рождения';

-- средний чек по каждому часу суток, по убыванию часа
CREATE OR REPLACE FUNCTION fn_avg_check_by_hour()
RETURNS TABLE (
    hour_of_day    INTEGER,
    orders_count   BIGINT,
    avg_check      NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        h.hour_of_day,
        COUNT(t.ticket_id) AS orders_count,
        CASE
            WHEN COUNT(t.ticket_id) = 0 THEN 0
            ELSE ROUND(SUM(t.price) / COUNT(t.ticket_id), 2)
        END AS avg_check
    FROM generate_series(0, 23) AS h(hour_of_day)
    LEFT JOIN tickets t
        ON t.status = 'Sold'
       AND EXTRACT(HOUR FROM t.purchase_date) = h.hour_of_day
    GROUP BY h.hour_of_day
    ORDER BY h.hour_of_day DESC;
END;
$$;

COMMENT ON FUNCTION fn_avg_check_by_hour IS
    'Средний чек (сумма заказов / кол-во заказов) по каждому часу суток для выполненных заказов, по убыванию часа';