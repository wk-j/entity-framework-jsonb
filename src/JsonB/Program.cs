using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using Dapper;
using static Dapper.SqlMapper;
using Newtonsoft.Json.Linq;
using System.Data;

namespace JsonB {
    public class JObjectHandler : TypeHandler<JObject> {
        private JObjectHandler() { }
        public static JObjectHandler Instance { get; } = new JObjectHandler();
        public override JObject Parse(object value) {
            var json = (string)value;
            return json == null ? null : JObject.Parse(json);
        }
        public override void SetValue(IDbDataParameter parameter, JObject value) {
            parameter.Value = value?.ToString(Newtonsoft.Json.Formatting.None);
        }
    }

    public class StudentSettingsHandler : TypeHandler<StudentSettings> {
        private StudentSettingsHandler() { }
        public static StudentSettingsHandler Instance { get; } = new StudentSettingsHandler();
        public override StudentSettings Parse(object value) {
            var json = (string)value;
            var obj = JObject.Parse(json);
            return obj.ToObject<StudentSettings>();
        }
        public override void SetValue(IDbDataParameter parameter, StudentSettings value) {
            parameter.Value = JsonConvert.SerializeObject(value);
        }
    }

    public class StudentSettings {
        public int A { set; get; }
        public int B { set; get; }
        public DateTime Min { set; get; } = DateTime.MinValue;
        public DateTime Max { set; get; } = DateTime.MaxValue;

    }
    public class Student {
        [Key]
        public int Id { set; get; }

        [Column(TypeName = "jsonb")]
        public string Settings { set; get; }
    }

    class MyContext : DbContext {
        public MyContext(DbContextOptions options) : base(options) {

        }

        public DbSet<Student> Students { set; get; }
    }

    class FakeStudent {
        public StudentSettings Settings { set; get; }
    }

    class Program {
        static void Save(DbContextOptions options) {
            using (var context = new MyContext(options)) {
                var settings = new StudentSettings {
                    A = 100,
                    B = 200
                };

                var json = JsonConvert.SerializeObject(settings);

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Students.Add(new Student { Settings = json });
                context.SaveChanges();
            }
        }

        static void Load(string connectionString) {
            SqlMapper.AddTypeHandler(JObjectHandler.Instance);
            SqlMapper.AddTypeHandler(StudentSettingsHandler.Instance);

            using (var conn = new NpgsqlConnection(connectionString)) {
                conn.Open();
                var sql = @"select * from ""Students""";
                var a = conn.QuerySingle<FakeStudent>(sql);
                var settings = a.Settings;

                Console.WriteLine(settings.A);
                Console.WriteLine(settings.B);
                Console.WriteLine(settings.Min);
                Console.WriteLine(settings.Max);
            }
        }

        static void Main(string[] args) {
            var connectionString = "Host=localhost;Database=JsonB;User Id=postgres;Password=1234";
            var options = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;

            Save(options);
            Load(connectionString);
        }
    }
}