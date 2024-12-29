using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LanguageModeSwitcherWpf.Common;

namespace LanguageModeSwitcherWpf.Models.Domain;

[Table(nameof(Rules))]
public class Rules
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Description("进程名称")]
    public string ProgressName { get; set; }

    [Description("监视IME代码变更")]
    public bool MonitIMECodeChanges {  get; set; }

    [Description("IME代码")]
    public IMECode IMECode { get; set; }

    [Description("锁定")]
    public bool Lock { get; set; }
}
