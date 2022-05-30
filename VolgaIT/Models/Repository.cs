using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace VolgaIT.Models
{
    public class Repository
    {
        public static NpgsqlConnection nc { get; set; }
        //Инициализирует строку подключения, схему базы данных и вставляет тестовые значения
        static Repository()
        {
            Console.WriteLine("Alive");
            string connStr = string.Format("Host={0};Port={1};Username={2};Password={3};Database={4}", "postgres", "5432","postgres","1234567","postgres");
            NpgsqlConnection Nc = new NpgsqlConnection(connStr);
            try
            {
                Nc.Open();
            }catch(Exception ex)
            {
                Console.WriteLine($"Исключение: {ex.Message}");
            }
            nc = Nc;
            //Инициализирует схему и тестовые значения
            
            string path = @"schema/0001_init.up.sql";

            string scriptInit = File.ReadAllText(path).Replace("\n","");

            NpgsqlCommand schemaQuery = new NpgsqlCommand(scriptInit, nc);

            schemaQuery.ExecuteNonQuery();

            path = @"schema/0002_insert_test_values.up.sql";

            scriptInit = File.ReadAllText(path);

            schemaQuery = new NpgsqlCommand(scriptInit, nc);

            schemaQuery.ExecuteNonQuery();
        }
        public User GetUser(string email)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT us.id, us.email, us.password, (SELECT COUNT(*) FROM applications WHERE user_id=us.id) AS count FROM users AS us WHERE email=$1", nc)
            {
                Parameters =
                {
                    new(){Value=email}
                }            
            };
            using(NpgsqlDataReader ndr = cmd.ExecuteReader())
                while(ndr.Read())
                {
                    //Парсинг данных
                    int id=Int32.Parse(ndr["id"].ToString());
                    string em = ndr["email"] as string;
                    string password = ndr["password"] as string;
                    int count = Int32.Parse(ndr["count"].ToString());
                    ndr.Close();
                    return new User { Id=id, Email=em, Password=password, CountApplication=count};
                }
            return null;
        }
        public void CreateUser(string email, string password)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO users (email, password) VALUES ($1,$2)", nc)
            {
                Parameters =
                {
                    new(){Value=email},
                    new(){Value=password}
                }
            };
            cmd.ExecuteNonQuery();
        }
        //Получает все приложения одного пользователя
        public List<Application> GetApplications(string email)
        {
            List<Application> listAp = new List<Application>();
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT id, name, date_create FROM applications WHERE user_id=(SELECT id FROM users WHERE email=$1)", nc)
            {
                Parameters =
                {
                    new(){Value=email}
                }
            };
            NpgsqlDataReader ndr = cmd.ExecuteReader();
            while (ndr.Read())
            {
                //Парсинг данных
                int id = Int32.Parse(ndr["id"].ToString());
                string name = ndr["name"] as string;
                DateTime date_create = DateTime.Parse(ndr["date_create"].ToString());
                listAp.Add(new Application { Id = id, Name = name, Date_create = date_create });
            }
            ndr.Close();
            return listAp;
        }
        //Получает приложения всех пользователей
        public List<Application> GetAllApplications()
        {
            List<Application> listAp = new List<Application>();
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT id, name, date_create FROM applications", nc);
            NpgsqlDataReader ndr = cmd.ExecuteReader();
            while (ndr.Read())
            {
                //Парсинг данных
                int id = Int32.Parse(ndr["id"].ToString());
                string name = ndr["name"] as string;
                DateTime date_create = DateTime.Parse(ndr["date_create"].ToString());
                listAp.Add(new Application { Id = id, Name = name, Date_create = date_create });
            }
            ndr.Close();
            return listAp;
        }
        public List<User_requests> GetRequests(int appId)
        {
            List<User_requests> listEv = new List<User_requests>();
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT id, application_id, name, date_request, extra_data FROM user_requests WHERE application_id=$1", nc)
            {
                Parameters =
                {
                    new(){Value=appId}
                }
            };
            NpgsqlDataReader ndr = cmd.ExecuteReader();
            while (ndr.Read())
            {
                //Парсинг данных
                int id = Int32.Parse(ndr["id"].ToString());
                string name= ndr["name"] as string;
                int application_id = Int32.Parse(ndr["application_id"].ToString());
                DateTime date_request = DateTime.Parse(ndr["date_request"].ToString());
                string extra_data = ndr["extra_data"] as string;
                listEv.Add(new User_requests { Id = id, Name=name, Application_Id=application_id, Date_request = date_request, Extra_data=extra_data });
            }
            ndr.Close();
            return listEv;
        }
        public void CreateRequest(int appID, string extraData, string name)
        {
            Console.WriteLine(appID);
            Console.WriteLine(extraData);
            Console.WriteLine(name);
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO user_requests (application_id, name, date_request, extra_data) VALUES ($1,$2,$3,$4)", nc)
            {
                Parameters =
                {
                    new(){Value=appID},
                    new(){Value=name},
                    new(){Value=DateTime.Now},
                    new(){Value=extraData}
                }
            };
            cmd.ExecuteNonQuery();
        }
        public void CreateApplication(Application app)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO applications (name, date_create, user_id) VALUES ($1,$2,$3)", nc)
            {
                Parameters =
                {
                    new(){Value=app.Name},
                    new(){Value=app.Date_create},
                    new(){Value=app.User_Id}
                }
            };
            cmd.ExecuteNonQuery();
        }
    }
}
