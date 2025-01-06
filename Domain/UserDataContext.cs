using System.Data.Entity;

namespace LanguageModeSwitcherWpf.Domain;

[DbConfigurationType(typeof(SQLiteConfiguration))]
public class UserDataContext : DbContext
{
    public UserDataContext() : base("UserData")
    {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        Database.SetInitializer(new UserDataInitializer(modelBuilder));
    }

    //在此处添加实体
    public virtual DbSet<Rules> Rules { get; set; }
}
