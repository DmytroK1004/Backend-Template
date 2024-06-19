using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos
{
    public record SigninDto
    {
        public User User { get; set; } = new User();

        public int Status { get; set; } = 0; // 0: success, 1: not match

        public string Token { get; set; } = string.Empty;
    }
}
