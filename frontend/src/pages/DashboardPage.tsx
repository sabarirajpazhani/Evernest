import { useEffect } from 'react';
import { MessageSquare, Users, Calendar, TrendingUp } from 'lucide-react';
import { useGetUserChatsQuery } from '@/app/api/chatApi';
import { useGetFriendsQuery } from '@/app/api/friendApi';
import { useGetUpcomingEventsQuery } from '@/app/api/eventApi';
import { useGetProfileQuery } from '@/app/api/userApi';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

const DashboardPage = () => {
  const { data: profile } = useGetProfileQuery();
  const { data: chats, isLoading: chatsLoading } = useGetUserChatsQuery();
  const { data: friends, isLoading: friendsLoading } = useGetFriendsQuery();
  const { data: events, isLoading: eventsLoading } = useGetUpcomingEventsQuery();

  const stats = [
    {
      label: 'Active Chats',
      value: chats?.length || 0,
      icon: MessageSquare,
      color: 'text-blue-500',
    },
    {
      label: 'Friends',
      value: friends?.length || 0,
      icon: Users,
      color: 'text-green-500',
    },
    {
      label: 'Upcoming Events',
      value: events?.length || 0,
      icon: Calendar,
      color: 'text-purple-500',
    },
    {
      label: 'Profile Status',
      value: profile?.status || 'Unknown',
      icon: TrendingUp,
      color: profile?.status === 'Approved' ? 'text-green-500' : 'text-yellow-500',
    },
  ];

  const recentChats = chats?.slice(0, 5) || [];
  const upcomingEvents = events?.slice(0, 3) || [];

  if (chatsLoading || friendsLoading || eventsLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-dark-text mb-2">Dashboard</h1>
        <p className="text-dark-muted">
          Welcome back, {profile?.username}! Here's what's happening in your Evernest.
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat, index) => (
          <div key={index} className="card">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-dark-muted">{stat.label}</p>
                <p className="text-2xl font-bold text-dark-text">{stat.value}</p>
              </div>
              <stat.icon className={`text-2xl ${stat.color}`} />
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Chats */}
        <div className="card">
          <h2 className="text-lg font-semibold text-dark-text mb-4">Recent Chats</h2>
          <div className="space-y-3">
            {recentChats.length > 0 ? (
              recentChats.map((chat) => (
                <div
                  key={chat.id}
                  className="flex items-center gap-3 p-3 rounded-lg hover:bg-dark-border cursor-pointer transition-colors"
                >
                  <div className="w-10 h-10 rounded-full bg-primary-500 flex items-center justify-center text-white font-semibold">
                    {chat.type === 'Private' 
                      ? chat.participants[0]?.username?.[0]?.toUpperCase()
                      : chat.name[0]?.toUpperCase()
                    }
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-dark-text truncate">
                      {chat.type === 'Private' 
                        ? chat.participants[0]?.username
                        : chat.name
                      }
                    </p>
                    <p className="text-xs text-dark-muted truncate">
                      {chat.lastMessage?.content || 'No messages yet'}
                    </p>
                  </div>
                  <div className="text-xs text-dark-muted">
                    {chat.unreadCount > 0 && (
                      <span className="bg-primary-500 text-white px-2 py-1 rounded-full">
                        {chat.unreadCount}
                      </span>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <p className="text-dark-muted text-center py-4">No recent chats</p>
            )}
          </div>
        </div>

        {/* Upcoming Events */}
        <div className="card">
          <h2 className="text-lg font-semibold text-dark-text mb-4">Upcoming Events</h2>
          <div className="space-y-3">
            {upcomingEvents.length > 0 ? (
              upcomingEvents.map((event) => (
                <div
                  key={event.id}
                  className="p-3 rounded-lg hover:bg-dark-border cursor-pointer transition-colors"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <p className="text-sm font-medium text-dark-text">{event.title}</p>
                      <p className="text-xs text-dark-muted">{event.location}</p>
                      <p className="text-xs text-dark-muted mt-1">
                        {new Date(event.startDate).toLocaleDateString()} at{' '}
                        {new Date(event.startDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </p>
                    </div>
                    <div className={`px-2 py-1 rounded text-xs ${
                      event.userRSVP === 'Attending' 
                        ? 'bg-green-500/20 text-green-500'
                        : event.userRSVP === 'Maybe'
                        ? 'bg-yellow-500/20 text-yellow-500'
                        : 'bg-gray-500/20 text-gray-500'
                    }`}>
                      {event.userRSVP}
                    </div>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-dark-muted text-center py-4">No upcoming events</p>
            )}
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card">
        <h2 className="text-lg font-semibold text-dark-text mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button className="p-4 rounded-lg border border-dark-border hover:bg-dark-border transition-colors text-left">
            <MessageSquare className="text-primary-500 mb-2" size={24} />
            <p className="font-medium text-dark-text">Start a Chat</p>
            <p className="text-sm text-dark-muted">Connect with friends</p>
          </button>
          <button className="p-4 rounded-lg border border-dark-border hover:bg-dark-border transition-colors text-left">
            <Users className="text-green-500 mb-2" size={24} />
            <p className="font-medium text-dark-text">Find Friends</p>
            <p className="text-sm text-dark-muted">Expand your network</p>
          </button>
          <button className="p-4 rounded-lg border border-dark-border hover:bg-dark-border transition-colors text-left">
            <Calendar className="text-purple-500 mb-2" size={24} />
            <p className="font-medium text-dark-text">Create Event</p>
            <p className="text-sm text-dark-muted">Plan an activity</p>
          </button>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
