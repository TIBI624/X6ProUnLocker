using System.Collections.Generic;

namespace X6ProUnLocker.Core
{
    public static class LanguageManager
    {
        public static string Current { get; private set; } = "en";

        private static readonly Dictionary<string, Dictionary<string, string>> _dict = new()
        {
            ["en"] = new()
            {
                ["Title"] = "X6ProUnLocker v1.1.0 — System Reanimator",
                ["Subtitle"] = "Recovery in the toughest cases",
                ["EnvNormal"] = "Environment: Normal mode",
                ["EnvSafe"] = "Environment: Safe Mode",
                ["EnvWinRE"] = "Environment: WinRE (Recovery)",
                ["EnvWinPE"] = "Environment: WinPE (Boot)",
                ["NoAdmin"] = " | ⚠️ Without admin rights",
                ["Refresh"] = "🔄 Refresh",
                ["EndTask"] = "⏹️ End task",
                ["ForceKill"] = "💥 Force kill",
                ["Properties"] = "📋 Properties",
                ["OpenFolder"] = "📂 Open folder",
                ["TaskMgr"] = "📊 Task Manager",
                ["Terminal"] = "💻 CMD Terminal",
                ["SysTools"] = "🛠️ System Tools",
                ["Malware"] = "🦠 Malware",
                ["Issues"] = "⚠️ System Issues",
                ["Startup"] = "🚀 Startup",
                ["Drivers"] = "🔧 Drivers",
                ["Services"] = "⚙️ Services",
                ["Logs"] = "📋 Logs",
                ["Ready"] = "Ready",
                ["AddToStartup"] = "Add to startup",
                ["RemoveFromStartup"] = "Remove from startup",
                ["ApplySelected"] = "✅ Apply selected",
                ["RemoveAll"] = "❌ Remove all",
                ["ScanViruses"] = "🦠 Scan for viruses",
                ["RestoreFonts"] = "🔤 Restore system fonts",
                ["ReplaceCmd"] = "🔄 Replace CMD",
                ["ReplaceSethc"] = "🔄 Replace SETHC",
                ["ReplaceUtilman"] = "🔄 Replace UTILMAN",
                ["RestoreOriginals"] = "↩️ Restore originals",
                ["BrowseExe"] = "📁 Browse .exe",
                ["StatusSuccess"] = "✅ {0}",
                ["StatusError"] = "❌ {0}",
                ["StatusWarning"] = "⚠️ {0}"
            },
            ["ru"] = new()
            {
                ["Title"] = "X6ProUnLocker v1.1.0 — Системный Реаниматор",
                ["Subtitle"] = "Восстановление в самых сложных случаях",
                ["EnvNormal"] = "Среда: Обычный режим",
                ["EnvSafe"] = "Среда: Безопасный режим",
                ["EnvWinRE"] = "Среда: WinRE (Восстановление)",
                ["EnvWinPE"] = "Среда: WinPE (Загрузочная)",
                ["NoAdmin"] = " | ⚠️ Без прав администратора",
                ["Refresh"] = "🔄 Обновить",
                ["EndTask"] = "⏹️ Завершить задачу",
                ["ForceKill"] = "💥 Принудительно",
                ["Properties"] = "📋 Свойства",
                ["OpenFolder"] = "📂 Открыть папку",
                ["TaskMgr"] = "📊 Диспетчер задач",
                ["Terminal"] = "💻 Терминал CMD",
                ["SysTools"] = "🛠️ Системные утилиты",
                ["Malware"] = "🦠 Вредоносное ПО",
                ["Issues"] = "⚠️ Проблемы системы",
                ["Startup"] = "🚀 Автозагрузка",
                ["Drivers"] = "🔧 Драйверы",
                ["Services"] = "⚙️ Службы",
                ["Logs"] = "📋 Журнал",
                ["Ready"] = "Готово",
                ["AddToStartup"] = "Добавить в автозагрузку",
                ["RemoveFromStartup"] = "Удалить из автозагрузки",
                ["ApplySelected"] = "✅ Применить выбранные",
                ["RemoveAll"] = "❌ Удалить все",
                ["ScanViruses"] = "🦠 Сканировать на вирусы",
                ["RestoreFonts"] = "🔤 Восстановить шрифты",
                ["ReplaceCmd"] = "🔄 Заменить CMD",
                ["ReplaceSethc"] = "🔄 Заменить SETHC",
                ["ReplaceUtilman"] = "🔄 Заменить UTILMAN",
                ["RestoreOriginals"] = "↩️ Восстановить оригиналы",
                ["BrowseExe"] = "📁 Выбрать .exe",
                ["StatusSuccess"] = "✅ {0}",
                ["StatusError"] = "❌ {0}",
                ["StatusWarning"] = "⚠️ {0}"
            }
        };

        public static void SetLanguage(string lang) => Current = _dict.ContainsKey(lang) ? lang : "en";
        public static string Get(string key) => _dict[Current].ContainsKey(key) ? _dict[Current][key] : key;
    }
}