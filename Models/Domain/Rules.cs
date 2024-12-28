using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LanguageModeSwitcherWpf.Common;

namespace LanguageModeSwitcherWpf.Models.Domain;

[Table(nameof(Rules))]
public class Rules
{
    [Key]
    [Required]
    public string ProgressName { get; set; }

    
    public MonitMode MonitMode { get; set; }


    public IMECode IMECode { get; set; }

    /// <summary>
    /// 锁定
    /// </summary>
    public bool Lock { get; set; }
}
