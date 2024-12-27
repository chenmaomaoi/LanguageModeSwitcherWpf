using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageModeSwitcherWpf.DB.Domain;

[Table(nameof(ProgrennName_dto))]
public class ProgrennName_dto
{
    [Key]
    [Required]
    public string ProgressName { get; set; }

    public FilterMode FilterMode { get; set; }

    public bool IsChineseMode { get; set; }
}

/// <summary>
/// 筛选模式
/// </summary>
public enum FilterMode
{
    ProgressName = 0x01,
    PID = 0x02,
    WindowTitle = 0x04
}

//对于excel这种，需要按WindowTitle来
[Table(nameof(WindowTitle_dto))]
public class WindowTitle_dto
{
    [Key]
    [Required]
    public string Title { get; set; }

    public bool IsChineseMode { get; set; }

}