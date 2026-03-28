import { useState, useEffect } from 'react';
import { Search, UserPlus, Users, MessageSquare, Calendar } from 'lucide-react';
import { useGetFriendsQuery, useSendFriendRequestMutation } from '@/app/api/friendApi';
import { useSearchUsersQuery } from '@/app/api/userApi';
import { Button } from '@/components/ui/button';
import Input from '@/components/ui/Input';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

const FriendsPage = () => {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<'friends' | 'requests' | 'search'>('friends');
  
  const { data: friends, isLoading: friendsLoading } = useGetFriendsQuery();
  const [sendFriendRequest] = useSendFriendRequestMutation();
  const { data: searchResults, isLoading: searchLoading } = useSearchUsersQuery(searchQuery);

  const handleSendFriendRequest = async (userId: string) => {
    try {
      await sendFriendRequest({ receiverId: userId }).unwrap();
    } catch (error) {
      console.error('Failed to send friend request:', error);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-dark-text mb-2">Friends</h1>
        <p className="text-dark-muted">Connect and communicate with your friends</p>
      </div>

      {/* Tabs */}
      <div className="flex space-x-1 border-b border-dark-border">
        <button
          onClick={() => setActiveTab('friends')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'friends'
              ? 'text-primary-500 border-b-2 border-primary-500'
              : 'text-dark-muted hover:text-dark-text'
          }`}
        >
          <Users size={20} className="inline mr-2" />
          Friends
        </button>
        <button
          onClick={() => setActiveTab('requests')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'requests'
              ? 'text-primary-500 border-b-2 border-primary-500'
              : 'text-dark-muted hover:text-dark-text'
          }`}
        >
          <UserPlus size={20} className="inline mr-2" />
          Requests
        </button>
        <button
          onClick={() => setActiveTab('search')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'search'
              ? 'text-primary-500 border-b-2 border-primary-500'
              : 'text-dark-muted hover:text-dark-text'
          }`}
        >
          <Search size={20} className="inline mr-2" />
          Find Friends
        </button>
      </div>

      {/* Tab Content */}
      {activeTab === 'friends' && (
        <div className="space-y-4">
          {friendsLoading ? (
            <div className="flex items-center justify-center h-32">
              <LoadingSpinner size="lg" />
            </div>
          ) : friends && friends.length > 0 ? (
            friends.map((friend) => (
              <div key={friend.id} className="card p-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-full bg-primary-500 flex items-center justify-center text-white font-semibold">
                      {friend.username[0]?.toUpperCase()}
                    </div>
                    <div>
                      <p className="font-medium text-dark-text">
                        {friend.username}
                      </p>
                      <p className="text-sm text-dark-muted">@{friend.username}</p>
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <Button variant="ghost" size="sm">
                      <MessageSquare size={20} />
                    </Button>
                    <Button variant="ghost" size="sm">
                      <Calendar size={20} />
                    </Button>
                  </div>
                </div>
              </div>
            ))
          ) : (
            <div className="text-center py-8">
              <Users size={48} className="mx-auto text-dark-muted mb-4" />
              <h3 className="text-lg font-medium text-dark-text mb-2">No friends yet</h3>
              <p className="text-dark-muted mb-4">Start connecting with people to see them here</p>
              <Button onClick={() => setActiveTab('search')}>
                Find Friends
              </Button>
            </div>
          )}
        </div>
      )}

      {activeTab === 'requests' && (
        <div className="space-y-4">
          <div className="text-center py-8">
            <UserPlus size={48} className="mx-auto text-dark-muted mb-4" />
            <h3 className="text-lg font-medium text-dark-text mb-2">No pending requests</h3>
            <p className="text-dark-muted">Your friend requests will appear here</p>
          </div>
        </div>
      )}

      {activeTab === 'search' && (
        <div className="space-y-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-dark-muted" size={20} />
            <Input
              placeholder="Search for users..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>

          {searchLoading ? (
            <div className="flex items-center justify-center h-32">
              <LoadingSpinner size="lg" />
            </div>
          ) : searchResults && searchResults.length > 0 ? (
            searchResults.map((user) => (
              <div key={user.id} className="card p-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-full bg-primary-500 flex items-center justify-center text-white font-semibold">
                      {user.username[0]?.toUpperCase()}
                    </div>
                    <div>
                      <p className="font-medium text-dark-text">
                        {user.username}
                      </p>
                      <p className="text-sm text-dark-muted">@{user.username}</p>
                    </div>
                  </div>
                  <Button onClick={() => handleSendFriendRequest(user.id)}>
                    <UserPlus size={20} className="mr-2" />
                    Add Friend
                  </Button>
                </div>
              </div>
            ))
          ) : searchQuery ? (
            <div className="text-center py-8">
              <Search size={48} className="mx-auto text-dark-muted mb-4" />
              <h3 className="text-lg font-medium text-dark-text mb-2">No results found</h3>
              <p className="text-dark-muted">Try searching with different keywords</p>
            </div>
          ) : (
            <div className="text-center py-8">
              <Search size={48} className="mx-auto text-dark-muted mb-4" />
              <h3 className="text-lg font-medium text-dark-text mb-2">Search for users</h3>
              <p className="text-dark-muted">Enter a username or name to find people</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default FriendsPage;
