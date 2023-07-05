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
            CreateMap<CreateOrderCommand, Order>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.New))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<OrderItemDto, OrderItem>().ReverseMap();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(source => Enum.GetName(typeof(OrderStatus), source.Status)));

        }
    }
}
