# AlbionOnlineDiscordAlarm

Дискорд бот, проверяющий статус серверов игры. Когда статус меняется, пишет об этом сообщение, тегая всех.
У игры есть 3 статуса серверов: offline, starting, online.

## Как использовать

Скомпилировать.
Создать в папке с приложением файл config.ini

Если пост идёт через бота

Вбить в него 2 строки:
```
BotToken=тут токен бота
ChannelId=тут айди канала, куда писать сообщения
```

Если через вебхук
```
WebhookUrl=тут юрл вебхука
```

Можно не указывать ни то, ни другое. Или и то, и другое.

Текст можно указать следующими строками:
```
OnlineText=Включился
OfflineText=Выключился
StartingText=Включается
```

Если апи точка для проверки статуса серверов опять изменится, можно указать юрл в конфиге
Url=https://serverstatus.albiononline.com

И по умолчанию бот проверяет статус западных серверов, для проверки восточных серверов нужно дописать в конфиг
Url=https://serverstatus-sgp.albiononline.com/