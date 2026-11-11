# PostgreSQL — инициализация

## Что здесь

- `init.sql` — SQL-скрипт, который выполняется **один раз** при первом старте контейнера.
- Создаёт 4 логические схемы: `user_schema`, `test_creation_schema`, `test_taking_schema`, `search_schema`.

## Когда выполняется

Только при **пустом volume `postgres_data`**. То есть при самом первом `docker-compose up -d`. При повторных запусках `init.sql` **не выполняется** — данные сохраняются.

## Как пересоздать с нуля

Если схемы уже созданы и что-то надо сбросить:

```powershell
docker-compose down -v
docker-compose up -d
```

**Внимание:** `-v` удалит все данные в БД, RabbitMQ и Redis. Использовать только на этапе разработки.

## Как проверить

```
docker exec -it knowledge-postgres psql -U admin -d KnowledgeDb -c "\dn"
```

Должно показать:

```
      Name        |  Owner   
------------------+----------
 public           | postgres
 search_schema    | admin
 test_creation_schema | admin
 test_taking_schema   | admin
 user_schema      | admin
```

## Замечание про EF Core migrations

В проекте **используются обе стратегии**:

- `init.sql` создаёт **схемы** (контейнеры для таблиц).
- EF Core migrations создают **таблицы** внутри схем (в соответствующем сервисе).

Это стандартный подход: инфраструктура — через Docker-инициализацию, структура данных — через миграции EF Core. См. задачу 0.4.

</BDS:create_file>

<BDS:create_file fileName="KnowledgeControl/docker/README.md">

```markdown
# Docker — инфраструктура KnowledgeControl

## Состав

| Сервис | Образ | Порт | Назначение |
|--------|-------|------|-----------|
| `postgres` | `postgres:16` | 5432 | Единая БД `KnowledgeDb` с 4 логическими схемами |
| `rabbitmq` | `rabbitmq:3-management` | 5672, 15672 | Брокер сообщений + веб-UI |
| `redis` | `redis:7-alpine` | 6379 | Распределённый кэш (решение «на 5») |

## Быстрый старт

```powershell
# 1. Скопировать .env.example в .env (один раз)
copy .env.example .env

# 2. Отредактировать .env — задать свои пароли
notepad .env

# 3. Поднять все сервисы
docker-compose up -d

# 4. Проверить статус
docker-compose ps

Все три контейнера должны быть в статусе `running` / `healthy`.

## Доступ

- **PostgreSQL:** `localhost:5432`, пользователь `admin`, БД `KnowledgeDb`, пароль — из `.env`.
- **RabbitMQ UI:** http://localhost:15672, логин/пароль — из `.env`.
- **Redis:** `localhost:6379`, без пароля (локально; для продакшена надо добавить `requirepass`).

## Полезные команды

```
# Логи конкретного сервиса
docker-compose logs -f postgres

# Зайти в psql
docker exec -it knowledge-postgres psql -U admin -d KnowledgeDb

# Список схем
docker exec -it knowledge-postgres psql -U admin -d KnowledgeDb -c "\dn"

# Зайти в redis-cli
docker exec -it knowledge-redis redis-cli

# Перезапустить сервис
docker-compose restart postgres

# Полностью остановить (данные сохраняются)
docker-compose down

# Полностью сбросить (данные удаляются)
docker-compose down -v
```

## Безопасность

- Все пароли — в `.env`, который **в `.gitignore`**.
- `.env.example` содержит только плейсхолдеры и идёт в git.
- Для защиты проекта на реальной машине — пароли генерируются заново.

