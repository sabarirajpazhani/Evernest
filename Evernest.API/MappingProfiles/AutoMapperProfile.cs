using AutoMapper;
using Evernest.API.Models;
using Evernest.API.DTOs.Auth;
using UserDto = Evernest.API.DTOs.User.UserDto;
using Evernest.API.DTOs.User;
using Evernest.API.DTOs.Chat;
using Evernest.API.DTOs.Friend;
using Evernest.API.DTOs.Event;

namespace Evernest.API.MappingProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FriendCount, opt => opt.MapFrom(src => src.FriendIds != null ? src.FriendIds.Count : 0));

            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Auth mappings
            CreateMap<User, AuthResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore());

            // Chat mappings
            CreateMap<Chat, ChatDto>()
                .ForMember(dest => dest.Participants, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessage, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsTyping, opt => opt.Ignore());

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.Sender, opt => opt.Ignore())
                .ForMember(dest => dest.ReplyToMessage, opt => opt.Ignore());

            // Friend request mappings
            CreateMap<FriendRequest, FriendRequestDto>()
                .ForMember(dest => dest.Sender, opt => opt.Ignore())
                .ForMember(dest => dest.Receiver, opt => opt.Ignore());

            // Event mappings
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Attendees, opt => opt.Ignore())
                .ForMember(dest => dest.InvitedUsers, opt => opt.Ignore())
                .ForMember(dest => dest.UserRSVP, opt => opt.Ignore())
                .ForMember(dest => dest.AttendingCount, opt => opt.MapFrom(src => src.AttendeeIds != null ? src.AttendeeIds.Count : 0))
                .ForMember(dest => dest.InvitedCount, opt => opt.MapFrom(src => src.InvitedUserIds != null ? src.InvitedUserIds.Count : 0));

            CreateMap<CreateEventDto, Event>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateEventDto, Event>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
