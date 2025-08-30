using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Users;
using DistriCatalogoAPI.Domain.Interfaces;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class GetUserNotificationPreferencesQueryHandler : IRequestHandler<GetUserNotificationPreferencesQuery, UserNotificationPreferencesDto?>
    {
        private readonly IUserNotificationPreferencesRepository _repository;
        private readonly IMapper _mapper;

        public GetUserNotificationPreferencesQueryHandler(
            IUserNotificationPreferencesRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserNotificationPreferencesDto?> Handle(GetUserNotificationPreferencesQuery request, CancellationToken cancellationToken)
        {
            var preferences = await _repository.GetByUserIdAsync(request.UserId);
            
            if (preferences == null)
                return null;

            return _mapper.Map<UserNotificationPreferencesDto>(preferences);
        }
    }
}