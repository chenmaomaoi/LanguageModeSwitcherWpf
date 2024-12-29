using System;

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

    public int RefreshDelay { get; set; } = 2;
}

