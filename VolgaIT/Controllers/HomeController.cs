using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VolgaIT.Models;

namespace VolgaIT.Controllers
{
    public class HomeController : Controller
    {
        //Инициализация подключения к базе данных
        Repository repo = new Repository();
        public IActionResult Index()
        {
            //Записываем в состояние сенса пустого пользователя, так как на
            //главной странице проверяется авторизация 
            User u = new User() { Email = "" };
            HttpContext.Session.Set("user", u);
            return View(u);
        }
        public ActionResult GrandPage()
        {
            return View("Index", HttpContext.Session.Get<User>("user"));
        }
        public ActionResult Login()
        {
            return View(HttpContext.Session.Get<User>("user"));
        }
        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            //Проверки на ввод
            if (Email != null && Password != null)
            {
                User us = repo.GetUser(Email);
                if (us != null && us.Password== Password)
                {
                    HttpContext.Session.Set("user", us);
                    ViewBag.ErrorMessage = "Вы вошли как " + us.Email;
                }
                else { ViewBag.ErrorMessage = "Неверный email или пароль"; }
            }
            else { ViewBag.ErrorMessage = "Введите email и пароль"; }
            return View("Login",HttpContext.Session.Get<User>("user"));
        }
        [HttpGet]
        public ActionResult Registration()
        {
            return View(HttpContext.Session.Get<User>("user"));
        }
        [HttpPost]
        public ActionResult Registration(string Email, string Password, string PasswordRepeat)
        {
            //Проверки на ввод
            if (Email != null && Password != null && PasswordRepeat!=null && (Password==PasswordRepeat))
            {
                User us=repo.GetUser(Email);
                if (us == null)
                {
                    repo.CreateUser(Email, Password);
                    int id = repo.GetUser(Email).Id;
                    HttpContext.Session.Set("user", new User {Id=id, Email=Email, Password=Password });
                    ViewBag.ErrorMessage = "Пользователь успешно добавлен";
                }
                else { ViewBag.ErrorMessage = "Данный адрес уже зарегистрирован"; }
            }
            else 
            {
                if (Password != PasswordRepeat){ ViewBag.ErrorMessage = "Пароли не совпадают";}
            }
            return View("Registration", HttpContext.Session.Get<User>("user"));
        }
        [HttpGet]
        public ActionResult Menu()
        {
            return View();
        }
        public ActionResult Exit()
        {
            User u = new User() { Email = "" };
            HttpContext.Session.Set("user", u);
            return View("Index",u);
        }
        public ActionResult ListApplication()
        {
            return View(repo.GetApplications(HttpContext.Session.Get<User>("user").Email));
        }
        public ActionResult SendRequest(int appId)
        {
            if (HttpContext.Session.Get<User>("user").Email != "")
            {
                List<string> l = new List<string>() { "getInfo", "postJSON", "postText" };
                IEnumerable<string> ie = l;
                ViewBag.Requests = new SelectList(ie);
                return View(new User_requests() { Application_Id = appId });
            }
            return View("Index");
        }
        public ActionResult GetImage(int appId)
        {
            ViewBag.Check = "Check";
            repo.CreateRequest(appId, "flowers", "getImage");
            return View("Image", appId);
        }
        [HttpPost]
        public ActionResult SendRequest(User_requests req)
        {
            ViewBag.Message = "Событие добавлено";
            repo.CreateRequest(req.Application_Id, req.Extra_data, req.Name);
            List<string> l = new List<string>() { "getInfo", "postJSON", "postText" };
            IEnumerable<string> ie = l;
            ViewBag.Requests = new SelectList(ie);
            return View("SendRequest", new User_requests() { Application_Id = req.Application_Id });
        }
        [HttpGet]
        public ActionResult ShowStat(int appId)
        {
            //Проверяем пользователя чтобы защитить данные
            if (HttpContext.Session.Get<User>("user").Email != "")
            {
                //Передаем название приложения для удержания этих данных на странице для запросов
                ViewBag.Application = appId;
                List<User_requests> list = repo.GetRequests(appId);
                int[] diaData = new int[] { list.Count(i => i.Name == "getInfo"),
                list.Count(i => i.Name == "getImage"),
                list.Count(i => i.Name == "postJSON"),
                list.Count(i => i.Name == "postText")};
                ViewBag.DiaData = diaData;
                return View(list);
            }
            return View("Index", HttpContext.Session.Get<User>("user"));
        }
        public ActionResult ShowStatRule(string status, int appId)
        {
            //Удержание данных на странице для запросов
            ViewBag.Application = appId;
            List<User_requests> list = repo.GetRequests(appId);
            List<User_requests> result = new List<User_requests>();
            //Фильтрация данных по отрезкам времени
            if (status == "week")
            {
                DateTime date1 = DateTime.Now.AddDays(-7);
                foreach(User_requests item in list)
                {
                    int r = DateTime.Compare(date1, item.Date_request);
                    if (r < 0)
                    {
                        result.Add(item);
                    }
                }
            }
            if (status == "month")
            {
                DateTime date1 = DateTime.Now.AddMonths(-1);
                foreach (User_requests item in list)
                {
                    int r = DateTime.Compare(date1, item.Date_request);
                    if (r < 0)
                    {
                        result.Add(item);
                    }
                }
            }
            if (status == "year")
            {
                DateTime date1 = DateTime.Now.AddYears(-1);
                foreach (User_requests item in list)
                {
                    int r = DateTime.Compare(date1, item.Date_request);
                    if (r < 0)
                    {
                        result.Add(item);
                    }
                }
            }
            int[] diaData = new int[] { result.Count(i => i.Name == "getInfo"),
            result.Count(i => i.Name == "getImage"),
            result.Count(i => i.Name == "postJSON"),
            result.Count(i => i.Name == "postText")};
            ViewBag.DiaData = diaData;
            return View("ShowStat", result);
        }
        public ActionResult AddApplication()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddApplication(Application app)
        {
            if (app.Name != null && HttpContext.Session.Get<User>("user").Email != "")
            {
                List<Application> list = repo.GetAllApplications();
                Application a = list.Find(i => i.Name == app.Name);
                if (a == null)
                {
                    app.Date_create = DateTime.Now;
                    app.User_Id = HttpContext.Session.Get<User>("user").Id;
                    repo.CreateApplication(app);
                    ViewBag.ErrorMessage = "Приложение добавлено";
                }
                else { ViewBag.ErrorMessage = "Данное имя приложения занято"; }
            }
            else { ViewBag.ErrorMessage = "Введите имя приложения"; }
            return View("AddApplication");
        }
    }
}
