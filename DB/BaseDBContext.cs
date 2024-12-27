using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LanguageModeSwitcherWpf.DB.Domain;
using SQLite.CodeFirst;

namespace LanguageModeSwitcherWpf.DB;

public class SQLiteConfiguration : DbConfiguration, IDbConnectionFactory
{
    public SQLiteConfiguration()
    {
        SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
        SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);

        var providerServices = (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices));

        SetProviderServices("System.Data.SQLite", providerServices);
        SetProviderServices("System.Data.SQLite.EF6", providerServices);

        SetDefaultConnectionFactory(this);
    }

    public DbConnection CreateConnection(string connectionString)
        => new SQLiteConnection(connectionString);
}

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
    public virtual DbSet<ProgrennName_dto> Records { get; set; }
}
