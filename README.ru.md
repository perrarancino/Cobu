<div align="center">

<a href="README.md">English</a> &nbsp;·&nbsp; <b>Русский</b>

<br><br>

<img src="docs/banner.ru.png" alt="CoBu — умный буфер обмена для Windows" width="100%">

<br>

<a href="https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Setup.exe"><img src="https://img.shields.io/badge/%D0%A1%D0%BA%D0%B0%D1%87%D0%B0%D1%82%D1%8C_%D0%B4%D0%BB%D1%8F_Windows-CoBu--Setup.exe-3DFF9A?style=for-the-badge&logo=windows&logoColor=white&labelColor=0D0F1B" alt="Скачать для Windows"></a>
&nbsp;
<a href="https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Portable.zip"><img src="https://img.shields.io/badge/Portable-.zip-69F0AE?style=for-the-badge&logo=7zip&logoColor=white&labelColor=0D0F1B" alt="Portable"></a>
&nbsp;
<a href="https://github.com/perrarancino/Cobu/stargazers"><img src="https://img.shields.io/github/stars/perrarancino/Cobu?style=for-the-badge&logo=github&logoColor=white&color=FFD54F&labelColor=0D0F1B&label=%D0%97%D0%B2%D1%91%D0%B7%D0%B4%D1%8B" alt="Звёзды"></a>

<p>
  <a href="#о-программе"><b>О программе</b></a> &nbsp;·&nbsp;
  <a href="#возможности"><b>Возможности</b></a> &nbsp;·&nbsp;
  <a href="#установка"><b>Установка</b></a> &nbsp;·&nbsp;
  <a href="#управление"><b>Управление</b></a> &nbsp;·&nbsp;
  <a href="#сборка-из-исходников"><b>Сборка</b></a> &nbsp;·&nbsp;
  <a href="https://github.com/perrarancino/Cobu/issues"><b>Баги и идеи</b></a>
</p>

<a href="https://github.com/perrarancino/Cobu/releases/latest"><img src="https://img.shields.io/github/v/release/perrarancino/Cobu?style=flat-square&logo=github&logoColor=white&color=3DFF9A&labelColor=0D0F1B" alt="Версия"></a>
<a href="https://github.com/perrarancino/Cobu/releases"><img src="https://img.shields.io/github/downloads/perrarancino/Cobu/total?style=flat-square&logo=githubactions&logoColor=white&color=69F0AE&labelColor=0D0F1B" alt="Скачиваний"></a>
<a href="LICENSE"><img src="https://img.shields.io/github/license/perrarancino/Cobu?style=flat-square&color=C9D1D9&labelColor=0D0F1B" alt="Лицензия"></a>
<img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?style=flat-square&logo=windows11&logoColor=white&labelColor=0D0F1B" alt="Windows 10 и 11">
<img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white&labelColor=0D0F1B" alt=".NET 10">
<img src="https://img.shields.io/github/last-commit/perrarancino/Cobu?style=flat-square&logo=git&logoColor=white&color=3DFF9A&labelColor=0D0F1B" alt="Последний коммит">

<br><br>

<img src="https://skillicons.dev/icons?i=cs,dotnet,windows,visualstudio&theme=dark" alt="C#, .NET, Windows">

</div>

<br>

## <img src="https://api.iconify.design/lucide/info.svg?color=%2300E676&height=26" height="26" align="absmiddle"> О программе

**CoBu** (Copy Buffer) — лёгкий менеджер буфера обмена. Он сам запоминает всё, что вы копируете: текст, ссылки, скриншоты и картинки. Любую запись можно вернуть в буфер одним кликом.

Окно открывается по **`Alt + V`**, живёт в трее и не мешает работе: когда оно скрыто, программа почти ничего не потребляет.

<br>

<div align="center">
<table>
  <tr>
    <td align="center"><img src="docs/screenshot-main.ru.png" alt="История буфера обмена" width="340"><br><sub><b>История: текст и картинки</b></sub></td>
    <td align="center"><img src="docs/screenshot-settings.ru.png" alt="Настройки" width="340"><br><sub><b>Настройки</b></sub></td>
  </tr>
</table>
</div>

<br>

## <img src="https://api.iconify.design/lucide/sparkles.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Возможности

<table>
  <tr>
    <td width="33%" valign="top">
      <h3><img src="https://api.iconify.design/lucide/feather.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Лёгкая</h3>
      Опрос буфера не нужен: CoBu слушает системное уведомление Windows. В трее занимает около 50 МБ, процессор в простое не нагружает. Память не растёт со временем: список виртуализирован, большие тексты лежат на диске, а скрытое окно возвращает память системе.
    </td>
    <td width="33%" valign="top">
      <h3><img src="https://api.iconify.design/lucide/image.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Текст и картинки</h3>
      Тексты сохраняются как <code>.txt</code>, изображения как <code>.png</code>. Точные дубликаты не плодятся: запись просто поднимается наверх.
    </td>
    <td width="33%" valign="top">
      <h3><img src="https://api.iconify.design/lucide/star.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Закрепление</h3>
      Нужные записи можно закрепить звездой, и они не вытеснятся лимитом истории.
    </td>
  </tr>
  <tr>
    <td valign="top">
      <h3><img src="https://api.iconify.design/lucide/lock.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Приватность</h3>
      Всё хранится только на вашем диске, рядом с программой. То, что менеджеры паролей помечают как секрет, CoBu не записывает.
    </td>
    <td valign="top">
      <h3><img src="https://api.iconify.design/lucide/rocket.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Тихий автозапуск</h3>
      С Windows стартует сразу в трей, без выезжающего окна.
    </td>
    <td valign="top">
      <h3><img src="https://api.iconify.design/lucide/app-window.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Красивое стеклянное окно</h3>
      Тёмное матовое стекло, плавные анимации, векторные иконки.
    </td>
  </tr>
</table>

<br>

## <img src="https://api.iconify.design/lucide/download.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Установка

**1.** Скачайте **[CoBu-Setup.exe](https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Setup.exe)**.<br>
**2.** Запустите. Установщик сам проверит [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0) и, если его нет, предложит скачать и поставить.<br>
**3.** Отметьте галочки (автозапуск, ярлык на рабочем столе) и нажмите «Установить».

Установить можно только для себя (без прав администратора) или для всех пользователей, в `Program Files`.

> [!NOTE]
> Установщик пока не подписан сертификатом, поэтому Windows SmartScreen может показать «Защитник Windows предотвратил запуск». Нажмите **Подробнее → Выполнить в любом случае**. Исходный код открыт, программу можно собрать самому.

**Portable:** [CoBu-Portable.zip](https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Portable.zip) — распаковать и запустить `CoBu.exe` (нужен .NET Desktop Runtime 10).

<br>

## <img src="https://api.iconify.design/lucide/keyboard.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Управление

| | Действие | Как |
|:-:|---|---|
| <img src="https://api.iconify.design/lucide/eye.svg?color=%2300E676&height=20" height="20"> | Показать / скрыть окно | `Alt + V` (в настройках есть ещё `Ctrl + Shift + V`, `Ctrl + ~`, `Alt + C`) |
| <img src="https://api.iconify.design/lucide/clipboard-copy.svg?color=%2300E676&height=20" height="20"> | Скопировать запись обратно | Клик по карточке |
| <img src="https://api.iconify.design/lucide/star.svg?color=%2300E676&height=20" height="20"> | Закрепить запись | Звезда на карточке |
| <img src="https://api.iconify.design/lucide/x.svg?color=%2300E676&height=20" height="20"> | Закрыть окно | `Esc` или `✕` |
| <img src="https://api.iconify.design/lucide/languages.svg?color=%2300E676&height=20" height="20"> | Сменить язык | Настройки → Язык (English или Русский; по умолчанию выбирается по языку Windows) |
| <img src="https://api.iconify.design/lucide/settings.svg?color=%2300E676&height=20" height="20"> | Меню, настройки, выход | Правый клик по значку в трее |

<br>

## <img src="https://api.iconify.design/lucide/wrench.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Сборка из исходников

Нужен [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
git clone https://github.com/perrarancino/Cobu.git
cd Cobu
dotnet run --project src/CoBu
```

Установщик и portable-архив (нужен [Inno Setup 6.3+](https://jrsoftware.org/isinfo.php)):

```powershell
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
```

Результат появится в папке `dist`.

<details>
<summary>Структура проекта</summary>

```
Cobu/
├── src/CoBu/
│   ├── Services/
│   │   ├── ClipboardMonitor.cs    # слушатель буфера обмена, запись картинок через Win32
│   │   ├── StorageService.cs      # хранение истории (.txt / .png / history.json)
│   │   ├── SettingsService.cs     # настройки и автозапуск
│   │   ├── Loc.cs                 # строки интерфейса на английском и русском
│   │   ├── HotkeyManager.cs       # глобальные горячие клавиши
│   │   ├── TrayIconManager.cs     # значок в трее
│   │   ├── WindowBlurHelper.cs    # эффект матового стекла
│   │   ├── MemoryTrimmer.cs       # возврат памяти системе, пока окно скрыто
│   │   └── AppPaths.cs            # где лежат данные
│   ├── Models/                    # ClipItem, AppSettings
│   ├── Converters/                # конвертеры для интерфейса
│   ├── Styles/Theme.xaml          # тема оформления
│   └── MainWindow.xaml(.cs)       # главное окно
├── installer/                     # скрипт Inno Setup и сборка установщика
└── docs/                          # картинки для README
```

</details>

<br>

## <img src="https://api.iconify.design/lucide/scale.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Лицензия

[MIT](LICENSE)

<br>

<div align="center">

### <img src="https://api.iconify.design/lucide/star.svg?color=%23FFD54F&height=26" height="26" align="absmiddle"> Понравился CoBu?

Если CoBu пригодился, поставьте звезду репозиторию: так проект проще найти другим.

<a href="https://github.com/perrarancino/Cobu"><img src="https://img.shields.io/github/stars/perrarancino/Cobu?style=social" alt="Поставить звезду"></a>

<br>

<a href="https://star-history.com/#perrarancino/Cobu&Date">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=perrarancino/Cobu&type=Date&theme=dark">
    <img alt="История звёзд" src="https://api.star-history.com/svg?repos=perrarancino/Cobu&type=Date" width="520">
  </picture>
</a>

</div>
