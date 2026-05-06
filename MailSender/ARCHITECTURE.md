# MailSender Architecture

## Назначение сервиса

`MailSender` отвечает за асинхронную доставку писем по паттерну transactional outbox.

Сервис не генерирует письма сам и не принимает HTTP-запросы от UI. Он работает как фоновый воркер и решает две отдельные задачи:

1. Забирает из Postgres новые записи из outbox-таблицы `OutboxEmails` и публикует их в RabbitMQ.
2. Забирает сообщения из RabbitMQ, отправляет письма через SMTP и помечает их доставленными в Postgres.

Идея архитектуры в том, что подготовка писем и их отправка разделены:

- backend создаёт записи в `OutboxEmails`
- `MailSender` занимается только доставкой

Это позволяет:

- не держать HTTP-запрос пользователя открытым во время SMTP-отправки
- переживать временные падения SMTP или RabbitMQ
- безопасно повторять обработку
- масштабировать отправку независимо от backend

## Общая схема

Поток данных выглядит так:

1. Пользователь инициирует рассылку через frontend.
2. Backend генерирует HTML для каждого адресата и сохраняет строки в `OutboxEmails`.
3. `RabbitOutboxPublisher` читает из `OutboxEmails` только неотправленные записи.
4. Publisher публикует их в RabbitMQ.
5. `RabbitMailConsumer` читает сообщения из очереди.
6. `SmtpSender` отправляет письмо через SMTP.
7. После успеха `PostgresOutboxRepository` помечает письмо как отправленное:
   - `OutboxEmails.Sent = true`
   - `mail_sender_state.sent_at = now()`

На этом жизненный цикл письма завершается.

## Архитектурные слои

Структура папок:

- `Host` — композиция приложения и DI
- `Configuration` — strongly typed options
- `Contracts` — интерфейсы, DTO и доменные записи
- `Infrastructure/Postgres` — работа с состоянием доставки в Postgres
- `Infrastructure/Rabbit` — подключение к RabbitMQ и декларация topology
- `Services/Smtp` — SMTP-отправка
- `Workers` — фоновые процессы publisher и consumer
- `Legacy` — старый код, исключённый из сборки
- `Examples` — вспомогательный пример ручной публикации в RabbitMQ

По сути внутри `MailSender` есть две независимые подсистемы:

- publisher pipeline: `Postgres -> RabbitMQ`
- delivery pipeline: `RabbitMQ -> SMTP -> Postgres`

## Точка входа

Файл: [Host/Program.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Host/Program.cs)

Этот файл собирает приложение как .NET Worker Service.

Что он делает:

1. Загружает секции конфигурации `Rabbit` и `Outbox`.
2. Создаёт `PostgresOptions` со строкой подключения.
3. Регистрирует `IOutboxRepository` как `PostgresOutboxRepository`.
4. Регистрирует `ISmtpSender` как `SmtpSender`.
5. Регистрирует два фоновых воркера:
   - `RabbitOutboxPublisher`
   - `RabbitMailConsumer`

Это означает, что после старта процесса оба воркера начинают жить параллельно в одном контейнере.

## Конфигурация

### `RabbitOptions`

Файл: [Configuration/RabbitOptions.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Configuration/RabbitOptions.cs)

Содержит настройки RabbitMQ:

- `Host`, `Port`, `User`, `Pass`
- имена exchange/queue/routing key
- retry topology
- dead-letter topology
- `Prefetch`
- `Consumers`

Это runtime-настройки пропускной способности и поведения очередей.

### `OutboxProcessingOptions`

Файл: [Configuration/OutboxProcessingOptions.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Configuration/OutboxProcessingOptions.cs)

Содержит настройки логики обработки:

- `BatchSize` — сколько записей забирать из outbox за один проход
- `PollIntervalMilliseconds` — как часто publisher опрашивает БД
- `PublishLeaseSeconds` — на сколько берётся lease на публикацию
- `DeliveryLeaseSeconds` — на сколько берётся lease на доставку
- `RetryDelaySeconds` — задержка между повторными попытками
- `MaxDeliveryAttempts` — максимум попыток перед DLQ

### `PostgresOptions`

Файл: [Configuration/PostgresOptions.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Configuration/PostgresOptions.cs)

Минимальный объект, содержащий строку подключения к Postgres.

## Контракты и доменные записи

### `EmailSendRequested`

Файл: [Contracts/EmailSendRequested.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Contracts/EmailSendRequested.cs)

Это payload сообщения, который публикуется в RabbitMQ.

Поля нужны consumer’у для SMTP-отправки:

- `MessageId`
- `To`
- `HtmlBody`
- `Subject`
- `FromEmail`
- `FromName`

Сейчас `MessageId` совпадает с `OutboxEmails.Id`. Это ключевой идентификатор всей цепочки.

### `OutboxEmail`

Файл: [Contracts/OutboxEmail.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Contracts/OutboxEmail.cs)

Это проекция строки из `OutboxEmails`, которую publisher забирает из базы.

### `IOutboxRepository`

Файл: [Contracts/IOutboxRepository.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Contracts/IOutboxRepository.cs)

Главный интерфейс состояния доставки.

Он описывает весь жизненный цикл:

- `InitializeAsync`
- `AcquirePublishBatchAsync`
- `MarkPublishedAsync`
- `ReleasePublishLeaseAsync`
- `TryAcquireDeliveryAsync`
- `MarkDeliveredAsync`
- `MarkDeliveryFailedAsync`

Именно через этот контракт worker’ы взаимодействуют с БД.

### `OutboxDeliveryAttempt`

Файл: [Contracts/OutboxDeliveryAttempt.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Contracts/OutboxDeliveryAttempt.cs)

Возвращает результат попытки взять письмо в обработку на стороне consumer.

Содержит:

- `Status`
- число предыдущих попыток

### `DeliveryAcquireStatus`

Тоже в [Contracts/OutboxDeliveryAttempt.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Contracts/OutboxDeliveryAttempt.cs)

Статусы:

- `Ready` — письмо можно отправлять
- `Completed` — письмо уже отправлено или обработка завершена
- `Busy` — письмо уже обрабатывается другим воркером

### `DeliveryFailureResult`

Файл: [Contracts/DeliveryFailureResult.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Contracts/DeliveryFailureResult.cs)

Возвращает результат неуспешной попытки SMTP-доставки:

- сколько уже было попыток
- нужно ли письмо отправить в dead-letter queue

### `ISmtpSender`

Файл: [Contracts/ISmtpSender.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Contracts/ISmtpSender.cs)

Абстракция над SMTP-отправкой. Позволяет отделить бизнес-пайплайн от конкретного MailKit-клиента.

## Инфраструктура RabbitMQ

### `RabbitConnectionFactoryProvider`

Файл: [Infrastructure/Rabbit/RabbitConnectionFactoryProvider.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Infrastructure/Rabbit/RabbitConnectionFactoryProvider.cs)

Создаёт `ConnectionFactory` для RabbitMQ.

Зачем нужен отдельный класс:

- чтобы не размазывать создание соединения по worker’ам
- чтобы централизованно настраивать reconnect-friendly поведение

### `RabbitTopology`

Файл: [Infrastructure/Rabbit/RabbitTopology.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Infrastructure/Rabbit/RabbitTopology.cs)

Отвечает за декларацию exchange и queue.

Создаёт:

- основную exchange
- retry exchange
- dead-letter exchange
- основную очередь
- retry очередь
- dead-letter очередь

#### Основная очередь

Основная очередь получает сообщения publisher’а.

Если consumer делает `nack` без `requeue`, сообщение уходит в retry exchange через `x-dead-letter-exchange`.

#### Retry очередь

Retry queue хранит сообщение временно по TTL:

- `x-message-ttl = RetryDelaySeconds * 1000`

После TTL сообщение автоматически возвращается в основную exchange.

Это даёт простой delayed retry без отдельного scheduler.

#### Dead-letter очередь

Если число попыток превышает лимит, consumer публикует сообщение в DLQ вручную.

Эта очередь нужна для:

- диагностики
- ручного разбора проблемных писем
- исключения бесконечной переработки

## Инфраструктура Postgres

### `PostgresOutboxRepository`

Файл: [Infrastructure/Postgres/PostgresOutboxRepository.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Infrastructure/Postgres/PostgresOutboxRepository.cs)

Это центральный класс всей архитектуры. Он реализует `IOutboxRepository`.

Он работает сразу с двумя таблицами:

1. `OutboxEmails`
2. `mail_sender_state`

### Роль `OutboxEmails`

`OutboxEmails` создаётся backend’ом и является источником истины для бизнес-факта отправки.

Ключевой флаг:

- `"Sent"`

Если `"Sent" = true`, письмо считается завершённым на уровне предметной области.

### Роль `mail_sender_state`

`mail_sender_state` создаётся самим `MailSender` в `InitializeAsync`.

Это техническая таблица состояния пайплайна.

Она хранит:

- когда письмо было опубликовано
- сколько было попыток публикации
- lease на публикацию
- lease на доставку
- число SMTP-попыток
- когда назначен следующий retry
- последнюю ошибку
- когда письмо реально доставлено
- когда письмо ушло в DLQ

Это позволяет не использовать локальные файлы и не хранить идемпотентность в памяти процесса.

### `InitializeAsync`

Создаёт `mail_sender_state` и индексы.

Важно:

- `MailSender` не создаёт `OutboxEmails`
- `OutboxEmails` создаёт backend через EF migration

### `AcquirePublishBatchAsync`

Эта функция:

1. Находит записи из `OutboxEmails`, где `"Sent" = false`.
2. Смотрит, что по ним нет финального `sent_at` и `dead_lettered_at`.
3. Проверяет, что publish lease отсутствует или истёк.
4. Создаёт строку в `mail_sender_state`, если её ещё нет.
5. Ставит `publish_lease_until`.
6. Увеличивает `publish_attempts`.
7. Возвращает пачку сообщений publisher’у.

Это защита от повторной одновременной публикации.

### `MarkPublishedAsync`

Помечает письмо как опубликованное:

- ставит `published_at`
- снимает `publish_lease_until`
- очищает `last_error`

### `ReleasePublishLeaseAsync`

Нужен, если публикация в RabbitMQ не удалась.

Он:

- снимает publish lease
- сохраняет текст ошибки

После этого письмо можно будет снова подобрать на следующем polling cycle.

### `TryAcquireDeliveryAsync`

Это gatekeeper на стороне SMTP-consumer.

Логика:

1. Гарантирует наличие строки в `mail_sender_state`.
2. Пытается взять `delivery_lease_until`.
3. Проверяет, что письмо ещё не отправлено и не dead-lettered.
4. Возвращает:
   - `Ready`, если письмо можно слать
   - `Busy`, если письмо уже кто-то взял
   - `Completed`, если письмо уже завершено

Именно этот шаг делает consumer идемпотентным на уровне БД.

### `MarkDeliveredAsync`

После успешного SMTP этот метод в одной транзакции:

1. Ставит `OutboxEmails.Sent = true`
2. Ставит `mail_sender_state.sent_at = now()`
3. Снимает `delivery_lease_until`
4. Очищает retry state и ошибки

Это финальная фиксация успешной доставки.

### `MarkDeliveryFailedAsync`

После SMTP-ошибки этот метод:

1. Снимает lease
2. Увеличивает `delivery_attempts`
3. Записывает `last_error`
4. Заполняет `last_attempt_at`
5. Либо назначает `next_retry_at`
6. Либо ставит `dead_lettered_at`, если превышен лимит попыток

## SMTP-сервис

### `SmtpSender`

Файл: [Services/Smtp/SmtpSender.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Services/Smtp/SmtpSender.cs)

Этот класс реализует реальную доставку через MailKit.

Что он делает:

1. Читает SMTP-конфигурацию из env vars.
2. Формирует `MimeMessage`.
3. Выставляет:
   - `From`
   - `To`
   - `Subject`
   - HTML body
   - стабильный `Message-Id`
   - `X-Outbox-Id`
4. Подключается к SMTP.
5. Аутентифицируется.
6. Отправляет письмо.

### Почему внутри есть `SemaphoreSlim`

`MailKit.Net.Smtp.SmtpClient` не должен использоваться конкурентно из нескольких потоков.

Поэтому `SmtpSender` сериализует доступ через `_lock`.

Важное следствие:

- один экземпляр `SmtpSender` отправляет письма последовательно
- параллелизм достигается количеством consumer’ов, а не количеством одновременных `SendAsync` внутри одного клиента

### Почему сохраняется соединение

`_client` переиспользуется между отправками, если соединение ещё живо.

Это уменьшает latency, потому что не нужно на каждое письмо заново:

- открывать TCP
- делать STARTTLS
- проходить SMTP AUTH

### Что происходит при ошибке

Если отправка ломается:

- клиент disconnect/dispose
- состояние соединения сбрасывается
- следующая попытка будет с нового подключения

Это защищает от повторного использования сломанного SMTP-сокета.

## Worker 1: Publisher

### `RabbitOutboxPublisher`

Файл: [Workers/RabbitOutboxPublisher.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Workers/RabbitOutboxPublisher.cs)

Этот воркер отвечает за стадию `Postgres -> RabbitMQ`.

### Что происходит на старте

В `StartAsync` он вызывает:

- `_outboxRepository.InitializeAsync(...)`

То есть гарантирует наличие `mail_sender_state`.

### Основной цикл

В `ExecuteAsync` воркер бесконечно:

1. Создаёт подключение к RabbitMQ.
2. Создаёт channel.
3. Декларирует topology.
4. Берёт batch из Postgres через `AcquirePublishBatchAsync`.
5. Для каждого письма сериализует `EmailSendRequested`.
6. Публикует сообщение в основную exchange.
7. После успеха зовёт `MarkPublishedAsync`.
8. Если публикация сломалась, зовёт `ReleasePublishLeaseAsync`.
9. Ждёт `PollIntervalMilliseconds` и повторяет цикл.

### Почему publisher отдельно от consumer

Это архитектурное разделение даёт:

- независимую буферизацию через RabbitMQ
- decoupling между чтением из БД и SMTP
- возможность временно пережить деградацию SMTP без давления на backend

### Почему он логирует `Outbox table is not ready yet`

Если backend ещё не успел применить миграции и таблица `OutboxEmails` отсутствует, publisher не падает насовсем.

Он ловит `PostgresException` с `42P01` и просто ждёт следующий цикл.

Это защита от гонки старта контейнеров.

## Worker 2: Consumer

### `RabbitMailConsumer`

Файл: [Workers/RabbitMailConsumer.cs](/d:/Bulk-e-mail-service-with-a-template-based-e-mail-builder/MailSender/Workers/RabbitMailConsumer.cs)

Этот воркер отвечает за стадию `RabbitMQ -> SMTP -> Postgres`.

### Что происходит на старте

В `StartAsync` он тоже вызывает:

- `_outboxRepository.InitializeAsync(...)`

Само подключение к RabbitMQ вынесено в рабочий цикл.

Это важно, потому что consumer теперь не роняет весь host, если RabbitMQ ещё не готов.

### Основной цикл

`ExecuteAsync` бесконечно:

1. Пытается подключиться к RabbitMQ.
2. Создаёт нужное число consumer channels.
3. На каждый channel создаёт отдельный DI scope.
4. Внутри scope получает свой `ISmtpSender`.
5. Подписывается на очередь через `BasicConsumeAsync`.
6. Если цикл ломается, логирует ошибку и повторяет попытку позже.

### Почему на каждого consumer создаётся свой scope

Чтобы каждый consumer работал со своим экземпляром `SmtpSender`.

Это позволяет:

- открыть несколько независимых SMTP-соединений
- добиться параллельной отправки писем

Именно `RabbitOptions.Consumers` задаёт фактический верхний предел параллелизма.

### Обработка одного сообщения

Метод `HandleMessageAsync` делает следующее:

1. Десериализует JSON body в `EmailSendRequested`.
2. Валидирует `MessageId` и `To`.
3. Пытается взять delivery lease через `TryAcquireDeliveryAsync`.
4. Если статус:
   - `Completed` — ACK и выходим
   - `Busy` — ACK и выходим
   - `Ready` — продолжаем отправку
5. Отправляет письмо через `smtpSender.SendAsync`.
6. После успеха вызывает `MarkDeliveredAsync`.
7. Делает `BasicAckAsync`.

### Что происходит при SMTP-ошибке

Если `SmtpSender.SendAsync` выбросил exception:

1. Вызывается `MarkDeliveryFailedAsync`.
2. Если попытки исчерпаны:
   - публикуется сообщение в DLQ
   - исходное сообщение ACK’ается
3. Иначе:
   - сообщение `nack` без `requeue`
   - RabbitMQ отправляет его в retry queue

Потом retry queue через TTL вернёт его обратно в основную queue.

### Что происходит при инфраструктурной ошибке

Если ошибка не связана с валидным SMTP-сценарием, а например сломалась БД в середине обработки:

- consumer делает `BasicNackAsync(..., requeue: true)`

То есть сообщение вернётся в ту же очередь как незавершённое.

### Почему invalid message ACK’ается

Если сообщение битое:

- нет `MessageId`
- пустой `To`
- не удалось корректно распарсить полезную нагрузку

то consumer его не ретраит бесконечно, а подтверждает и дропает.

Иначе очередь можно было бы легко заблокировать навсегда ядовитым сообщением.

## Идемпотентность и модель надёжности

### Что гарантируется

Архитектура ориентирована на effectively-once поведение в рамках практического transactional outbox.

Она строится на нескольких механизмах:

1. `OutboxEmails` как источник истины.
2. `mail_sender_state` как техническое состояние.
3. publish lease для защиты от повторной публикации.
4. delivery lease для защиты от повторной SMTP-отправки.
5. `OutboxEmails.Sent = true` как финальный commit успеха.
6. retry queue для временных ошибок.
7. DLQ для окончательно проваленных писем.

### Что не гарантируется

Exactly-once для SMTP в общем случае недостижим.

Например возможен редкий сценарий:

1. SMTP-сервер уже принял письмо.
2. Процесс умер до `MarkDeliveredAsync`.

Тогда при следующем восстановлении возможен дубль.

Это фундаментальное ограничение SMTP как внешней side effect системы.

### Почему текущая модель всё равно хорошая

Она:

- минимизирует число дублей
- не теряет письма при временных сбоях
- даёт повторную обработку
- сохраняет диагностику ошибок
- пригодна для нескольких инстансов `MailSender`

## Use cases

### Use case 1. Нормальная отправка письма

Сценарий:

1. Backend создаёт запись в `OutboxEmails`.
2. Publisher подбирает запись в batch.
3. Publisher публикует сообщение в RabbitMQ.
4. Consumer получает сообщение.
5. Consumer берёт delivery lease.
6. SMTP успешно отправляет письмо.
7. Repository помечает письмо отправленным.

Результат:

- `OutboxEmails.Sent = true`
- `mail_sender_state.sent_at != null`

### Use case 2. RabbitMQ временно недоступен

Сценарий:

1. Publisher или consumer не может подключиться к RabbitMQ.
2. Воркер логирует ошибку.
3. Ждёт retry delay.
4. Пытается подключиться снова.

Результат:

- процесс не падает окончательно
- после восстановления RabbitMQ работа продолжается

### Use case 3. Backend ещё не создал `OutboxEmails`

Сценарий:

1. `MailSender` стартует раньше backend migration.
2. Publisher получает `42P01 relation "OutboxEmails" does not exist`.
3. Логирует, что outbox table ещё не готова.
4. Ждёт и повторяет попытку.

Результат:

- `MailSender` не умирает
- после появления таблицы продолжает обычную работу

### Use case 4. SMTP временно недоступен

Сценарий:

1. Consumer получает письмо.
2. SMTP send падает.
3. `MarkDeliveryFailedAsync` увеличивает счётчик попыток.
4. Сообщение отправляется в retry queue.
5. Через TTL сообщение возвращается в основную очередь.
6. Consumer пробует снова.

Результат:

- письмо не теряется
- retry распределён во времени

### Use case 5. Письмо системно неотправляемо

Сценарий:

1. Каждая SMTP-попытка завершается ошибкой.
2. `delivery_attempts` доходит до `MaxDeliveryAttempts`.
3. Repository помечает письмо `dead_lettered_at`.
4. Consumer публикует сообщение в DLQ.

Результат:

- письмо больше не крутится бесконечно
- проблема остаётся доступной для разбора

### Use case 6. Повторное получение уже обработанного сообщения

Сценарий:

1. Сообщение снова приходит consumer’у.
2. `TryAcquireDeliveryAsync` видит, что письмо уже завершено.
3. Возвращает `Completed`.
4. Consumer ACK’ает сообщение и ничего не отправляет.

Результат:

- повторного SMTP-send не происходит

### Use case 7. Два consumer’а пытаются взять одно письмо

Сценарий:

1. Два процесса почти одновременно видят одно и то же сообщение.
2. Только один сможет взять delivery lease.
3. Второй получит `Busy`.

Результат:

- параллельного двойного send не происходит

## Производительность

На throughput сильнее всего влияют:

1. `RabbitOptions.Consumers`
2. `RabbitOptions.Prefetch`
3. `OutboxProcessingOptions.BatchSize`
4. `OutboxProcessingOptions.PollIntervalMilliseconds`
5. реальный SMTP rate limit провайдера

Текущая модель параллелизма такая:

- один consumer = один экземпляр `SmtpSender`
- один `SmtpSender` шлёт письма последовательно
- несколько consumer’ов дают несколько параллельных SMTP-потоков

То есть пропускная способность масштабируется в основном через число consumer’ов.

## Ограничения текущей архитектуры

### 1. Нет сущности "рассылка"

`MailSender` знает только отдельные письма.

Он не знает:

- campaign id
- batch id
- mailing id
- имя рассылки

Поэтому статистика доступна только на уровне письма, а не на уровне бизнес-рассылки.

### 2. Нет собственной HTTP/API observability

Сервис не экспонирует:

- metrics endpoint
- health endpoint
- dashboard

Наблюдаемость сейчас в основном через:

- логи
- `OutboxEmails`
- `mail_sender_state`
- RabbitMQ UI

### 3. Нет provider-specific rate limiting

Сейчас нет специальной логики:

- ограничивать gmail отдельно от outlook
- адаптироваться под throttling

Есть только общий параллелизм через число consumer’ов.

### 4. Нет отдельного мигратора

`MailSender` не создаёт `OutboxEmails`.

Таблица появляется только через backend migration. Это важная зависимость на внешний сервис.

## Legacy-код

Папка: `Legacy`

Там лежат старые компоненты:

- `RabbitWorker`
- `FileIdempotencyStore`
- `SqlitePublicationTracker`
- связанные интерфейсы

Они больше не участвуют в сборке и оставлены только как исторический след.

Главные отличия старого подхода от нового:

- старый код опирался на локальные file/sqlite-механизмы
- новый код хранит operational state в Postgres
- старый код был хуже приспособлен к нескольким инстансам
- новый код лучше соответствует cluster-safe transactional outbox

## Краткое резюме

`MailSender` — это worker-сервис, реализующий двухступенчатую доставку:

- `OutboxEmails -> RabbitMQ`
- `RabbitMQ -> SMTP -> OutboxEmails`

Ключевая архитектурная идея:

- бизнес-факт письма хранится в `OutboxEmails`
- техническое состояние пайплайна хранится в `mail_sender_state`

Основные сильные стороны текущей реализации:

- асинхронная отправка
- retry и DLQ
- идемпотентность на уровне БД
- устойчивость к временным сбоям RabbitMQ/SMTP
- пригодность к горизонтальному масштабированию

Основные ограничения:

- нет понятия "конкретная рассылка"
- нет built-in метрик и dashboard
- нет exactly-once для SMTP как внешнего side effect

Тем не менее для задачи transactional outbox для email это уже полноценная и достаточно зрелая архитектура.
