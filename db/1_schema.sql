-- безопасная очистка перед пересозданием
DROP TABLE IF EXISTS tickets CASCADE;
DROP TABLE IF EXISTS customers CASCADE;
DROP TABLE IF EXISTS ticket_types CASCADE;
DROP TABLE IF EXISTS events CASCADE;
DROP TABLE IF EXISTS venues CASCADE;
DROP TYPE IF EXISTS ticket_status CASCADE;
DROP TYPE IF EXISTS event_status CASCADE;

-- площадки проведения мероприятий
CREATE TABLE venues (
    venue_id     SERIAL PRIMARY KEY,
    name         VARCHAR(200) NOT NULL,
    address      VARCHAR(300),
    city         VARCHAR(100) NOT NULL,
    capacity     INTEGER NOT NULL CHECK (capacity > 0),
    created_at   TIMESTAMP NOT NULL DEFAULT now()
);

COMMENT ON TABLE venues IS 'Площадки, на которых проводятся мероприятия';

-- мероприятия
CREATE TABLE events (
    event_id        SERIAL PRIMARY KEY,
    name            VARCHAR(200) NOT NULL,
    description     TEXT,
    venue_id        INTEGER NOT NULL REFERENCES venues(venue_id) ON DELETE RESTRICT,
    category        VARCHAR(100),
    start_datetime  TIMESTAMP NOT NULL,
    end_datetime    TIMESTAMP NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'Planned'
                    CHECK (status IN ('Planned','Ongoing','Completed','Cancelled')),
    created_at      TIMESTAMP NOT NULL DEFAULT now(),
    CONSTRAINT chk_event_dates CHECK (end_datetime > start_datetime)
);

COMMENT ON TABLE events IS 'Мероприятия (концерты, конференции, лекции и т.п.)';

CREATE INDEX idx_events_start_datetime ON events(start_datetime);
CREATE INDEX idx_events_status ON events(status);
CREATE INDEX idx_events_venue ON events(venue_id);

-- типы билетов на мероприятие 
CREATE TABLE ticket_types (
    ticket_type_id   SERIAL PRIMARY KEY,
    event_id         INTEGER NOT NULL REFERENCES events(event_id) ON DELETE CASCADE,
    name             VARCHAR(100) NOT NULL,
    price            NUMERIC(10,2) NOT NULL CHECK (price >= 0),
    total_quantity   INTEGER NOT NULL CHECK (total_quantity >= 0),
    CONSTRAINT uq_event_tickettype UNIQUE (event_id, name)
);

COMMENT ON TABLE ticket_types IS 'Категории билетов на мероприятие с ценой и лимитом мест';

CREATE INDEX idx_ticket_types_event ON ticket_types(event_id);

-- покупатели
CREATE TABLE customers (
    customer_id   SERIAL PRIMARY KEY,
    first_name    VARCHAR(100) NOT NULL,
    last_name     VARCHAR(100) NOT NULL,
    birth_date    DATE NOT NULL,
    email         VARCHAR(150) NOT NULL UNIQUE,
    phone         VARCHAR(20),
    created_at    TIMESTAMP NOT NULL DEFAULT now()
);

COMMENT ON TABLE customers IS 'Покупатели билетов';

-- билеты 
CREATE TABLE tickets (
    ticket_id        SERIAL PRIMARY KEY,
    ticket_type_id   INTEGER NOT NULL REFERENCES ticket_types(ticket_type_id) ON DELETE CASCADE,
    event_id         INTEGER NOT NULL REFERENCES events(event_id) ON DELETE CASCADE,
    seat_number      VARCHAR(20),
    status           VARCHAR(20) NOT NULL DEFAULT 'Available'
                     CHECK (status IN ('Available','Reserved','Sold','Cancelled')),
    customer_id      INTEGER REFERENCES customers(customer_id) ON DELETE RESTRICT,
    purchase_date    TIMESTAMP,
    price            NUMERIC(10,2) NOT NULL CHECK (price >= 0),
    CONSTRAINT chk_sold_has_customer CHECK (
        (status = 'Sold' AND customer_id IS NOT NULL AND purchase_date IS NOT NULL)
        OR (status <> 'Sold')
    )
);

COMMENT ON TABLE tickets IS 'Билеты на мероприятия (аналог заказов), привязанные к типу билета и (опционально) покупателю';

CREATE INDEX idx_tickets_event ON tickets(event_id);
CREATE INDEX idx_tickets_status ON tickets(status);
CREATE INDEX idx_tickets_customer ON tickets(customer_id);

-- =====================================================================
-- Конец скрипта схемы
-- =====================================================================
