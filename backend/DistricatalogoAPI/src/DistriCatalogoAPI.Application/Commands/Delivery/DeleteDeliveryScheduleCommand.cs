using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Delivery
{
    public class DeleteDeliveryScheduleCommand : IRequest
    {
        public int Id { get; set; }
    }
}