using System.Data.Entity;
using LanguageModeSwitcherWpf.Models.Domain;
using SQLite.CodeFirst;

namespace LanguageModeSwitcherWpf.Models;

[DbConfigurationType(typeof(SQLiteConfiguration))]
public class DatabaseContext : DbContext
{
    public DatabaseContext() : base("DataBase")
    {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        //如果不存在数据库，则创建
        Database.SetInitializer(new SqliteCreateDatabaseIfNotExists<DatabaseContext>(modelBuilder));
    }

    //在此处添加实体
    public virtual DbSet<Rules> Records { get; set; }
}
