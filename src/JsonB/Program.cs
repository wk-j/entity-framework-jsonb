using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace JsonB {
    public class StudentSettings {
        public int A { set; get; }
        public int B { set; get; }

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
    class Program {
        static void Main(string[] args) {
            var connectionString = "Host=localhost;Database=JsonB;User Id=postgres;Password=1234";
            var options = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;


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
    }
}