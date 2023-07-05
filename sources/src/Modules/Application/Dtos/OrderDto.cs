﻿using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class OrderDto
    {
        public long Id { get; set; }

        public long? CustomerId { get; set; }

        public DateTime? OrderDate { get; set; }

        public string Status { get; set; }

        public decimal? TotalSum { get; set; }

        public List<OrderItemDto> Items { get; set; }
    }
}
