using Application.Dtos;
using AutoMapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    internal class OrderMapping : Profile
    {
        public OrderMapping() 
        {
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<Delivery, DeliveryDto>().ReverseMap();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(source => Enum.GetName(typeof(OrderStatus), source.Status)));

        }
    }
}
