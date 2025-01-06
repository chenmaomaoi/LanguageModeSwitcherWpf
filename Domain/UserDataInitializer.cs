using System.Data.Entity;
using SQLite.CodeFirst;

namespace LanguageModeSwitcherWpf.Domain;

public class UserDataInitializer : SqliteDropCreateDatabaseWhenModelChanges<UserDataContext>
{
    public UserDataInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
    {
    }

    protected override void Seed(UserDataContext context)
    {
        Rules defaultRule = new()
        {
            Id = 1,
            ProgressName = "默认",
            MonitIMECodeChanges = true,
            IMECode = Common.IMECode.Native,
            Lock = true
        };
        context.Set<Rules>().Add(defaultRule);
    }
}
