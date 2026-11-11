-- ============================================================
-- KnowledgeControl — инициализация PostgreSQL
-- ============================================================
-- Скрипт выполняется АВТОМАТИЧЕСКИ при первом старте контейнера
-- (когда volume postgres_data пустой).
--
-- Создаёт 4 логические схемы для четырёх микросервисов.
-- Пароли и пользователи создаются отдельно — при разработке
-- все сервисы ходят в БД под одним пользователем POSTGRES_USER.
-- Разделение по пользователям БД вынесено за рамки курсового
-- (см. пояснительную записку, раздел про изоляцию схем).
-- ============================================================

-- Убедимся, что мы в правильной БД
\connect KnowledgeDb;

-- 1. Схема для UserService
CREATE SCHEMA IF NOT EXISTS user_schema
    AUTHORIZATION admin;

-- 2. Схема для TestCreationService
CREATE SCHEMA IF NOT EXISTS test_creation_schema
    AUTHORIZATION admin;

-- 3. Схема для TestTakingService
CREATE SCHEMA IF NOT EXISTS test_taking_schema
    AUTHORIZATION admin;

-- 4. Схема для SearchService
CREATE SCHEMA IF NOT EXISTS search_schema
    AUTHORIZATION admin;

-- Проверка: вывести все схемы пользователя admin
\dn

-- Полезные расширения
-- pg_trgm нужен для будущих индексов на ILIKE (если понадобится)
-- uuid-ossp не нужен — генерируем UUID в C# через Guid.NewGuid()
CREATE EXTENSION IF NOT EXISTS pg_trgm;
