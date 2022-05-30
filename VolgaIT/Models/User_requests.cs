using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VolgaIT.Models
{
    public class User_requests
    {
        [Required(ErrorMessage = "required")]
        public int Id { get; set; }
        public int Application_Id { get; set; }
        public string Name { get; set; }
        public DateTime Date_request { get; set; }
        public string Extra_data { get; set; }
    }
}
