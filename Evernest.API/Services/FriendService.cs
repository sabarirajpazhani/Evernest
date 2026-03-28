using Evernest.API.DTOs.User;
using Evernest.API.DTOs.Friend;
using Evernest.API.Models;
using Evernest.API.Services.Interfaces;
using Evernest.Repository.Repositories.Interfaces;
using AutoMapper;

namespace Evernest.API.Services
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public FriendService(
            IFriendRequestRepository friendRequestRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _friendRequestRepository = friendRequestRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<FriendRequestDto> SendFriendRequestAsync(string senderId, SendFriendRequestDto request)
        {
            var sender = await _userRepository.GetByIdAsync(senderId);
            var receiver = await _userRepository.GetByIdAsync(request.ReceiverId);

            if (sender == null || receiver == null)
                throw new KeyNotFoundException("User not found");

            if (await _userRepository.IsFriendAsync(senderId, request.ReceiverId))
                throw new InvalidOperationException("Users are already friends");

            if (await _friendRequestRepository.ExistsAsync(senderId, request.ReceiverId))
                throw new InvalidOperationException("Friend request already sent");

            var friendRequest = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Message = request.Message,
                Status = FriendRequestStatus.Pending
            };

            var createdRequest = await _friendRequestRepository.CreateAsync(friendRequest);
            return await MapFriendRequestDtoAsync(createdRequest);
        }

        public async Task<List<FriendRequestDto>> GetSentRequestsAsync(string userId)
        {
            var requests = await _friendRequestRepository.GetSentRequestsAsync(userId);
            var dtos = new List<FriendRequestDto>();

            foreach (var request in requests)
            {
                dtos.Add(await MapFriendRequestDtoAsync(request));
            }

            return dtos;
        }

        public async Task<List<FriendRequestDto>> GetReceivedRequestsAsync(string userId)
        {
            var requests = await _friendRequestRepository.GetReceivedRequestsAsync(userId);
            var dtos = new List<FriendRequestDto>();

            foreach (var request in requests)
            {
                dtos.Add(await MapFriendRequestDtoAsync(request));
            }

            return dtos;
        }

        public async Task<FriendRequestDto> RespondToFriendRequestAsync(string requestId, string userId, RespondFriendRequestDto response)
        {
            var friendRequest = await _friendRequestRepository.GetByIdAsync(requestId);
            if (friendRequest == null)
                throw new KeyNotFoundException("Friend request not found");

            if (friendRequest.ReceiverId != userId)
                throw new UnauthorizedAccessException("You can only respond to requests sent to you");

            if (friendRequest.Status != FriendRequestStatus.Pending)
                throw new InvalidOperationException("This request has already been responded to");

            friendRequest.Status = response.Status;
            var updatedRequest = await _friendRequestRepository.UpdateAsync(friendRequest);

            if (response.Status == FriendRequestStatus.Accepted)
            {
                await _userRepository.AddFriendAsync(friendRequest.SenderId, friendRequest.ReceiverId);
            }

            return await MapFriendRequestDtoAsync(updatedRequest);
        }

        public async Task<bool> CancelFriendRequestAsync(string requestId, string userId)
        {
            var friendRequest = await _friendRequestRepository.GetByIdAsync(requestId);
            if (friendRequest == null)
                return false;

            if (friendRequest.SenderId != userId)
                throw new UnauthorizedAccessException("You can only cancel requests you sent");

            if (friendRequest.Status != FriendRequestStatus.Pending)
                return false;

            await _friendRequestRepository.DeleteAsync(requestId);
            return true;
        }

        public async Task<List<UserDto>> GetFriendsAsync(string userId)
        {
            var friends = await _userRepository.GetFriendsAsync(userId);
            return friends.Select(u =>
            {
                var userDto = _mapper.Map<UserDto>(u);
                userDto.FriendCount = u.FriendIds?.Count ?? 0;
                return userDto;
            }).ToList();
        }

        public async Task<bool> RemoveFriendAsync(string userId, string friendId)
        {
            return await _userRepository.RemoveFriendAsync(userId, friendId);
        }

        public async Task<bool> IsFriendAsync(string userId, string friendId)
        {
            return await _userRepository.IsFriendAsync(userId, friendId);
        }

        private async Task<FriendRequestDto> MapFriendRequestDtoAsync(FriendRequest request)
        {
            var sender = await _userRepository.GetByIdAsync(request.SenderId);
            var receiver = await _userRepository.GetByIdAsync(request.ReceiverId);

            return new FriendRequestDto
            {
                Id = request.Id,
                Sender = _mapper.Map<UserDto>(sender),
                Receiver = _mapper.Map<UserDto>(receiver),
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                RespondedAt = request.RespondedAt,
                Message = request.Message
            };
        }
    }
}
