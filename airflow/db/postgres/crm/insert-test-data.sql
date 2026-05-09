-- Вставка тестовых пользователей
INSERT INTO users (user_id, username, email, first_name, last_name) VALUES
    ('11111111-1111-1111-1111-111111111111', 'user1', 'user1@example.com', 'Ольга', 'Смирнова'),
    ('22222222-2222-2222-2222-222222222222', 'user2', 'user2@example.com', 'Дмитрий', 'Кузнецов');

-- Вставка протезов
INSERT INTO prostheses (prosthesis_id, user_id, model, serial_number, warranty_end, firmware_version) VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'BionicPRO X1', 'BPX1001', '2026-01-15', 'v2.1.0'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '22222222-2222-2222-2222-222222222222', 'BionicPRO X2', 'BPX2001', '2026-02-20', 'v2.1.0');