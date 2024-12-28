using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageModeSwitcherWpf.Common;

namespace LanguageModeSwitcherWpf.Models;

[Serializable]
public class Settings
{

    public int RefreshDelay { get; set; } = 300;

    public bool StartMonitor { get; set; } = true;

    public bool MonitConfigSaveToDB { get; set; } = false;

    public MonitMode DefaultMonitMode { get; set; } = MonitMode.Monitor;

    public MonitConfigSaveMode DefaultMonitConfigSaveMode { get; set; } = MonitConfigSaveMode.Memory;

}
