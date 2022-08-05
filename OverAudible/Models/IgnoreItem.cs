using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    public class IgnoreItem
    {
        [Key]
        public string Asin { get; set; }
    }
}
