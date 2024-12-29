using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageModeSwitcherWpf.Common;

namespace LanguageModeSwitcherWpf;

[Serializable]
public class Configs
{
    public static readonly string Name = "输入法切换器";

    public static readonly string ConfigFilePath = $@"{Environment.CurrentDirectory}/Configs.yaml";

    /// <summary>
    /// 在程序启动时，删除所有没被锁定的
    /// </summary>
    public bool DeleteUnlockRules { get; set; } = true;

    public NotifyIconConfig NotifyIconConfig { get; set; } = new NotifyIconConfig();
}

[Serializable]
public class NotifyIconConfig
{
    public int RefreshDelay { get; set; } = 300;

    public int BalloonTipDelay { get; set; } = 5;
}
