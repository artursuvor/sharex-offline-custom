# ShareX Offline Custom

Личная офлайн-версия ShareX для Windows x64.

## Изменения

- Ручной размер кисти и текста до 4096, дополнительные быстрые размеры.
- Кнопка `100%` для непрозрачных кисти и заливки фигур.
- Принудительно отключены загрузчики, обновления, внешние ссылки и загрузка изображений по URL.
- HTTP блокируется до создания соединения; FTP, SFTP, отправка почты и ping также заблокированы.
- Сохранены локальный захват, редактирование, буфер обмена и сохранение файлов.

## Сборка

Требуются Windows и .NET SDK 10. Загрузка зависимостей при сборке требует интернета; готовое приложение работает офлайн.

```powershell
dotnet restore ShareX/ShareX.csproj -p:Platform=x64
dotnet build ShareX/ShareX.csproj -c Release -p:Platform=x64 --no-restore
dotnet publish ShareX/ShareX.csproj -c Release -p:Platform=x64 -r win-x64 --self-contained true -o artifacts/ShareX-Offline
New-Item -ItemType File -Force artifacts/ShareX-Offline/Portable
```

Запуск: `artifacts/ShareX-Offline/ShareX.exe`. Переносите папку целиком. Файл `Portable` включает хранение настроек рядом с приложением.

После сборки проверка фактических DLL:

```powershell
dotnet run --project tests/OfflineSmoke/OfflineSmoke.csproj -c Release
```

Проверка подтверждает запрет HTTP до соединения, запрет ping и принудительное отключение загрузчиков и обновлений.

## Основа

Исходный проект: https://github.com/ShareX/ShareX, исходная ревизия `cb360c1`.
Исходная документация сохранена в `README.upstream.md`, лицензия — в `LICENSE.txt`.
Унаследованные workflows сохранены в `.github/upstream-workflows` и автоматически не запускаются.
Пользовательские конфигурации, снимки экрана и готовые сборки в репозиторий не входят.
