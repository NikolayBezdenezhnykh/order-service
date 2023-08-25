using Application.Clients.AuthService;
using Application.Clients.PaymentService;
using Application.Clients.UserClient;
using Application.Dtos;
using AutoMapper;
using Confluent.Kafka;
using Domain;
using Infrastructure;
using Infrastructure.KafkaProducer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Saga
{
    public class OrderSagaManager : IOrderSagaManager
    {
        private readonly IOrderSagaStepHandler _orderSagaStepHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IAuthServiceClient _authServiceClient;
        private readonly OrderDbContext _orderDbContext;

        public OrderSagaManager(
            IOrderSagaStepHandler orderSagaStepHandler,
            IHttpContextAccessor httpContextAccessor,
            IKafkaProducer kafkaProducer,
            IUserServiceClient userServiceClient,
            IAuthServiceClient authServiceClient,
            OrderDbContext orderDbContext) 
        {
            _orderSagaStepHandler = orderSagaStepHandler;
            _httpContextAccessor = httpContextAccessor;
            _orderDbContext = orderDbContext;
            _userServiceClient = userServiceClient;
            _authServiceClient = authServiceClient;
            _kafkaProducer = kafkaProducer;
        }   

        public async Task<string> GeneratePaymentLinkAsync(Order order)
        {
            var orderSaga = new OrderSaga()
            {
                CreateDateAt = DateTime.UtcNow,
                UpdateDateAt = DateTime.UtcNow,
                Order = order,
                RemainingAttemptsCount = 5,
                OrderSagaSteps = new List<OrderSagaStep>()
                {
                    new OrderSagaStep() { Name = SagaConstants.ReserveProductStep, Status = (int)OrderSagaStatus.New, },
                    new OrderSagaStep() { Name = SagaConstants.ReserveDeliveryStep, Status = (int)OrderSagaStatus.New, },
                    new OrderSagaStep() { Name = SagaConstants.GenerateLinkPaymentStep, Status = (int)OrderSagaStatus.New, },
                },
                Status = (int)OrderSagaStatus.New
            };

            try
            {
                await _orderSagaStepHandler.HandleStepAsync(orderSaga);

                return orderSaga.PaymentLink;
            }
            finally
            {
                if (orderSaga.OrderSagaSteps.All(s => s.Status == (int)OrderSagaStatus.Done))
                {
                    var startNextDate = DateTime.UtcNow.AddMinutes(15);
                    if(_httpContextAccessor.HttpContext.Request.Query.ContainsKey("offsetMinute"))
                    {
                        var offsetMinute = _httpContextAccessor.HttpContext.Request.Query["offsetMinute"].ToString();
                        startNextDate = startNextDate.AddMinutes(-int.Parse(offsetMinute));
                    }

                    orderSaga.StartNextDate = startNextDate;
                    order.Status = (int)OrderStatus.WaitingPay;
                }
                else
                {
                    orderSaga.Status = (int)OrderSagaStatus.Error;
                    orderSaga.StartNextDate = DateTime.UtcNow;
                }

                orderSaga.UpdateDateAt = DateTime.UtcNow;
                _orderDbContext.OrderSaga.Add(orderSaga);
                await _orderDbContext.SaveChangesAsync();
            }
        }

        public async Task AnnulPaymentLinkAsync(OrderSaga orderSaga)
        {
            if (orderSaga.InvoiceId.HasValue
                && !orderSaga.OrderSagaSteps.Any(s => s.Name == SagaConstants.AnnulLinkPaymentStep))
            {
                orderSaga.OrderSagaSteps.Add(new OrderSagaStep() { Name = SagaConstants.AnnulLinkPaymentStep, Status = (int)OrderSagaStatus.New, IsCompensation = true });
            }

            if (orderSaga.ReserveProductId.HasValue
                && !orderSaga.OrderSagaSteps.Any(s => s.Name == SagaConstants.CancelReserveProductStep))
            {
                orderSaga.OrderSagaSteps.Add(new OrderSagaStep() { Name = SagaConstants.CancelReserveProductStep, Status = (int)OrderSagaStatus.New, IsCompensation = true });
            }

            if (orderSaga.ReserveDeliveryId.HasValue
                && !orderSaga.OrderSagaSteps.Any(s => s.Name == SagaConstants.CancelReserveDeliveryStep))
            {
                orderSaga.OrderSagaSteps.Add(new OrderSagaStep() { Name = SagaConstants.CancelReserveDeliveryStep, Status = (int)OrderSagaStatus.New, IsCompensation = true });
            }

            try
            {
                orderSaga.RemainingAttemptsCount--;

                await _orderSagaStepHandler.HandleCompensationStepAsync(orderSaga);

                if (orderSaga.InvoiceId.HasValue)
                {
                    orderSaga.Order.Status = (int)OrderStatus.Canceled;
                }

            }
            finally
            {
                if (orderSaga.OrderSagaSteps.Where(s => s.IsCompensation == true).All(s => s.Status == (int)OrderSagaStatus.Done))
                {
                    orderSaga.Status = (int)OrderSagaStatus.Done;
                }
                else
                {
                    if (orderSaga.RemainingAttemptsCount > 0)
                    {
                        orderSaga.Status = (int)OrderSagaStatus.Error;
                        orderSaga.StartNextDate = DateTime.UtcNow;
                    }
                    else
                    {
                        orderSaga.Status = (int)OrderSagaStatus.Failed;
                    }
                }

                orderSaga.UpdateDateAt = DateTime.UtcNow;

                await _orderDbContext.SaveChangesAsync();
            }
        }

        public async Task PaidPaymentLinkAsync(OrderSaga orderSaga)
        {
            orderSaga.Order.Status = (int)OrderStatus.Paid;
            orderSaga.Status = (int)OrderSagaStatus.Done;

            await _orderDbContext.SaveChangesAsync();

            var responseAuth = await _authServiceClient.GetTokenAsync(new List<string>() { "user-service" });
            var response = await _userServiceClient.GetUserAsync(responseAuth.AccessToken, orderSaga.Order.UserId.Value);

            // отправляем событие заказ оформлен
            var orderPaidMessage = new OrderPaidMessage()
            {
                EmailTo = response.Email,
                DynamicParams = new Dictionary<string, string>()
                {
                    ["{{sum}}"] = orderSaga.Order.TotalSum.ToString(),
                    ["{{details}}"] = GetOrderDetails(orderSaga.Order),
                    ["{{dateDelivery}}"] = orderSaga.Order.Delivery.Date.Value.ToString("dd.MM.yyyy"),
                    ["{{addressDelivery}}"] = orderSaga.Order.Delivery.Address
                }
            };

            await _kafkaProducer.PublishMessageAsync(JsonConvert.SerializeObject(orderPaidMessage));
        }

        public async Task<string> GetPaymentLinkAsync(Order order)
        {
            var orderSaga = await _orderDbContext.OrderSaga.Where(o => o.Order.Id == order.Id).FirstOrDefaultAsync();

            return orderSaga.PaymentLink;
        }

        private string GetOrderDetails(Order order)
        {
            var sb = new StringBuilder();

            foreach(var item in order.Items) 
            {
                sb.Append($"<tr><td>{item.ProductName}</td><td>{item.Quantity}</td></tr>");
            }

            return sb.ToString();
        }
    }
}
