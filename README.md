<div align="center">

<b>English</b> &nbsp;·&nbsp; <a href="README.ru.md">Русский</a>

<br><br>

<img src="docs/banner.en.png" alt="CoBu — smart clipboard manager for Windows" width="100%">

<br>

<a href="https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Setup.exe"><img src="https://img.shields.io/badge/Download_for_Windows-CoBu--Setup.exe-3DFF9A?style=for-the-badge&logo=windows&logoColor=white&labelColor=0D0F1B" alt="Download for Windows"></a>
&nbsp;
<a href="https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Portable.zip"><img src="https://img.shields.io/badge/Portable-.zip-69F0AE?style=for-the-badge&logo=7zip&logoColor=white&labelColor=0D0F1B" alt="Portable"></a>
&nbsp;
<a href="https://github.com/perrarancino/Cobu/stargazers"><img src="https://img.shields.io/github/stars/perrarancino/Cobu?style=for-the-badge&logo=github&logoColor=white&color=FFD54F&labelColor=0D0F1B&label=Stars" alt="Stars"></a>

<p>
  <a href="#about"><b>About</b></a> &nbsp;·&nbsp;
  <a href="#features"><b>Features</b></a> &nbsp;·&nbsp;
  <a href="#installation"><b>Installation</b></a> &nbsp;·&nbsp;
  <a href="#controls"><b>Controls</b></a> &nbsp;·&nbsp;
  <a href="#build-from-source"><b>Build</b></a> &nbsp;·&nbsp;
  <a href="https://github.com/perrarancino/Cobu/issues"><b>Bugs &amp; ideas</b></a>
</p>

<a href="https://github.com/perrarancino/Cobu/releases/latest"><img src="https://img.shields.io/github/v/release/perrarancino/Cobu?style=flat-square&logo=github&logoColor=white&color=3DFF9A&labelColor=0D0F1B" alt="Version"></a>
<a href="https://github.com/perrarancino/Cobu/releases"><img src="https://img.shields.io/github/downloads/perrarancino/Cobu/total?style=flat-square&logo=githubactions&logoColor=white&color=69F0AE&labelColor=0D0F1B" alt="Downloads"></a>
<a href="LICENSE"><img src="https://img.shields.io/github/license/perrarancino/Cobu?style=flat-square&color=C9D1D9&labelColor=0D0F1B" alt="License"></a>
<img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?style=flat-square&logo=windows11&logoColor=white&labelColor=0D0F1B" alt="Windows 10 and 11">
<img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white&labelColor=0D0F1B" alt=".NET 10">
<img src="https://img.shields.io/github/last-commit/perrarancino/Cobu?style=flat-square&logo=git&logoColor=white&color=3DFF9A&labelColor=0D0F1B" alt="Last commit">

<br><br>

<img src="https://skillicons.dev/icons?i=cs,dotnet,windows,visualstudio&theme=dark" alt="C#, .NET, Windows">

</div>

<br>

## <img src="https://api.iconify.design/lucide/info.svg?color=%2300E676&height=26" height="26" align="absmiddle"> About

**CoBu** (Copy Buffer) is a lightweight clipboard manager. It automatically remembers everything you copy: text, links, screenshots and images. Any entry can be put back into the clipboard with one click.

The window opens with **`Alt + V`**, lives in the tray and stays out of your way: while it is hidden, the program uses almost nothing.

<br>

<div align="center">
<table>
  <tr>
    <td align="center"><img src="docs/screenshot-main.en.png" alt="Clipboard history" width="340"><br><sub><b>History: text and images</b></sub></td>
    <td align="center"><img src="docs/screenshot-settings.en.png" alt="Settings" width="340"><br><sub><b>Settings</b></sub></td>
  </tr>
</table>
</div>

<br>

## <img src="https://api.iconify.design/lucide/sparkles.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Features

<table>
  <tr>
    <td width="33%" valign="top">
      <h3><img src="https://api.iconify.design/lucide/feather.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Lightweight</h3>
      No polling: CoBu listens to the Windows clipboard notification. About 50 MB in the tray and no CPU load when idle. Memory does not grow over time: the list is virtualized, long texts are kept on disk, and the hidden window hands memory back to the system.
    </td>
    <td width="33%" valign="top">
      <h3><img src="https://api.iconify.design/lucide/image.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Text and images</h3>
      Texts are saved as <code>.txt</code>, images as <code>.png</code>. Exact duplicates are not created: the entry simply moves to the top.
    </td>
    <td width="33%" valign="top">
      <h3><img src="https://api.iconify.design/lucide/star.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Pinning</h3>
      Pin important entries with a star and they will never be pushed out by the history limit.
    </td>
  </tr>
  <tr>
    <td valign="top">
      <h3><img src="https://api.iconify.design/lucide/lock.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Private</h3>
      Everything stays on your disk, next to the program. Anything password managers mark as secret is never recorded.
    </td>
    <td valign="top">
      <h3><img src="https://api.iconify.design/lucide/rocket.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Silent autostart</h3>
      Starts with Windows straight into the tray, without a sliding window.
    </td>
    <td valign="top">
      <h3><img src="https://api.iconify.design/lucide/app-window.svg?color=%2300E676&height=22" height="22" align="absmiddle"> Beautiful glass window</h3>
      Dark frosted glass, smooth animations, vector icons.
    </td>
  </tr>
</table>

<br>

## <img src="https://api.iconify.design/lucide/download.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Installation

**1.** Download **[CoBu-Setup.exe](https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Setup.exe)**.<br>
**2.** Run it. The installer checks for the [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0) and offers to download and install it if it is missing.<br>
**3.** Tick the options (autostart, desktop shortcut) and click Install.

You can install just for yourself (no administrator rights needed) or for all users, into `Program Files`.

> [!NOTE]
> The installer is not code-signed yet, so Windows SmartScreen may show "Windows protected your PC". Click **More info → Run anyway**. The source code is open, so you can also build the program yourself.

**Portable:** [CoBu-Portable.zip](https://github.com/perrarancino/Cobu/releases/latest/download/CoBu-Portable.zip). Unpack and run `CoBu.exe` (needs the .NET Desktop Runtime 10).

<br>

## <img src="https://api.iconify.design/lucide/keyboard.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Controls

| | Action | How |
|:-:|---|---|
| <img src="https://api.iconify.design/lucide/eye.svg?color=%2300E676&height=20" height="20"> | Show / hide the window | `Alt + V` (Settings also offers `Ctrl + Shift + V`, `Ctrl + ~`, `Alt + C`) |
| <img src="https://api.iconify.design/lucide/clipboard-copy.svg?color=%2300E676&height=20" height="20"> | Copy an entry back | Click the card |
| <img src="https://api.iconify.design/lucide/star.svg?color=%2300E676&height=20" height="20"> | Pin an entry | Star on the card |
| <img src="https://api.iconify.design/lucide/x.svg?color=%2300E676&height=20" height="20"> | Close the window | `Esc` or `✕` |
| <img src="https://api.iconify.design/lucide/languages.svg?color=%2300E676&height=20" height="20"> | Change the language | Settings → Language (English or Русский; by default it follows Windows) |
| <img src="https://api.iconify.design/lucide/settings.svg?color=%2300E676&height=20" height="20"> | Menu, settings, exit | Right-click the tray icon |

<br>

## <img src="https://api.iconify.design/lucide/wrench.svg?color=%2300E676&height=26" height="26" align="absmiddle"> Build from source

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
git clone https://github.com/perrarancino/Cobu.git
cd Cobu
dotnet run --project src/CoBu
```

Installer and portable archive (needs [Inno Setup 6.3+](https://jrsoftware.org/isinfo.php)):

```powershell
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
```

The result appears in the `dist` folder.

<details>
<summary>Project structure</summary>

```
Cobu/
├── src/CoBu/
│   ├── Services/
│   │   ├── ClipboardMonitor.cs    # clipboard listener, writes images through Win32
│   │   ├── StorageService.cs      # history storage (.txt / .png / history.json)
│   │   ├── SettingsService.cs     # settings and autostart
│   │   ├── Loc.cs                 # English / Russian interface strings
│   │   ├── HotkeyManager.cs       # global hotkeys
│   │   ├── TrayIconManager.cs     # tray icon
│   │   ├── WindowBlurHelper.cs    # frosted glass effect
│   │   ├── MemoryTrimmer.cs       # gives memory back to the system while hidden
│   │   └── AppPaths.cs            # where the data lives
│   ├── Models/                    # ClipItem, AppSettings
│   ├── Converters/                # UI converters
│   ├── Styles/Theme.xaml          # theme
│   └── MainWindow.xaml(.cs)       # main window
├── installer/                     # Inno Setup script and installer build
└── docs/                          # images for the README
```

</details>

<br>

## <img src="https://api.iconify.design/lucide/scale.svg?color=%2300E676&height=26" height="26" align="absmiddle"> License

[MIT](LICENSE)

<br>

<div align="center">

### <img src="https://api.iconify.design/lucide/star.svg?color=%23FFD54F&height=26" height="26" align="absmiddle"> Like CoBu?

If CoBu is useful to you, give the repository a star: it helps other people find the project.

<a href="https://github.com/perrarancino/Cobu"><img src="https://img.shields.io/github/stars/perrarancino/Cobu?style=social" alt="Star this repository"></a>

<br>

<a href="https://star-history.com/#perrarancino/Cobu&Date">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=perrarancino/Cobu&type=Date&theme=dark">
    <img alt="Star history" src="https://api.star-history.com/svg?repos=perrarancino/Cobu&type=Date" width="520">
  </picture>
</a>

</div>
