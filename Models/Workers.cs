using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebSheduler.Models
{
    public class Workers
    {
        public IEnumerable<Users> Users { get; set; }
        public List<List<Events>> Events { get; set; }
    }
}