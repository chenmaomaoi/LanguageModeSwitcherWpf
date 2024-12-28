using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageModeSwitcherWpf.Models.Domain;

[Table(nameof(Rules))]
public class Rules
{
    [Key]
    [Required]
    public string ProgressName { get; set; }

    public bool IsChineseMode { get; set; }

    /// <summary>
    /// 锁定
    /// </summary>
    public bool Lock { get; set; }
}