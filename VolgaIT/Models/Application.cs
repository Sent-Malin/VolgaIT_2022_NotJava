using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VolgaIT.Models
{
    public class Application
    {
        [Required(ErrorMessage = "required")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Приложение не может существовать без названия")]
        public string Name { get; set; }
        public DateTime? Date_create { get; set; }
        public int? User_Id { get; set; }
    }
}
