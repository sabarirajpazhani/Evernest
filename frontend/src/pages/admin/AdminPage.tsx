import { useState } from 'react';
import { Users, Calendar, MessageSquare, Settings, CheckCircle, XCircle, Clock } from 'lucide-react';
import { useGetPendingUsersQuery, useApproveUserMutation } from '@/app/api/userApi';
import { Button } from '@/components/ui/button';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

const AdminPage = () => {
  const [activeTab, setActiveTab] = useState<'users' | 'events' | 'stats'>('users');
  
  const { data: pendingUsers, isLoading: usersLoading } = useGetPendingUsersQuery();
  const [approveUser] = useApproveUserMutation();

  const handleApproveUser = async (userId: string, approved: boolean) => {
    try {
      await approveUser({ 
        id: userId, 
        status: { status: approved ? 'Approved' : 'Rejected' } 
      }).unwrap();
    } catch (error) {
      console.error('Failed to approve user:', error);
    }
  };

  const stats = {
    totalUsers: 150,
    activeUsers: 89,
    pendingApproval: pendingUsers?.length || 0,
    totalEvents: 25,
    upcomingEvents: 8,
    totalMessages: 1250,
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-dark-text mb-2">Admin Dashboard</h1>
        <p className="text-dark-muted">Manage users, events, and system settings</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-dark-muted">Total Users</p>
              <p className="text-2xl font-bold text-dark-text">{stats.totalUsers}</p>
            </div>
            <Users className="text-blue-500" size={24} />
          </div>
        </div>
        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-dark-muted">Active Users</p>
              <p className="text-2xl font-bold text-dark-text">{stats.activeUsers}</p>
            </div>
            <CheckCircle className="text-green-500" size={24} />
          </div>
        </div>
        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-dark-muted">Pending Approval</p>
              <p className="text-2xl font-bold text-dark-text">{stats.pendingApproval}</p>
            </div>
            <Clock className="text-yellow-500" size={24} />
          </div>
        </div>
        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-dark-muted">Total Events</p>
              <p className="text-2xl font-bold text-dark-text">{stats.totalEvents}</p>
            </div>
            <Calendar className="text-purple-500" size={24} />
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex space-x-1 border-b border-dark-border">
        <button
          onClick={() => setActiveTab('users')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'users'
              ? 'text-primary-500 border-b-2 border-primary-500'
              : 'text-dark-muted hover:text-dark-text'
          }`}
        >
          <Users size={20} className="inline mr-2" />
          User Management
        </button>
        <button
          onClick={() => setActiveTab('events')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'events'
              ? 'text-primary-500 border-b-2 border-primary-500'
              : 'text-dark-muted hover:text-dark-text'
          }`}
        >
          <Calendar size={20} className="inline mr-2" />
          Event Management
        </button>
        <button
          onClick={() => setActiveTab('stats')}
          className={`px-4 py-2 font-medium transition-colors ${
            activeTab === 'stats'
              ? 'text-primary-500 border-b-2 border-primary-500'
              : 'text-dark-muted hover:text-dark-text'
          }`}
        >
          <MessageSquare size={20} className="inline mr-2" />
          Statistics
        </button>
      </div>

      {/* Tab Content */}
      {activeTab === 'users' && (
        <div className="space-y-4">
          <div className="card">
            <h2 className="text-lg font-semibold text-dark-text mb-4">Pending User Approvals</h2>
            {usersLoading ? (
              <div className="flex items-center justify-center h-32">
                <LoadingSpinner size="lg" />
              </div>
            ) : pendingUsers && pendingUsers.length > 0 ? (
              <div className="space-y-4">
                {pendingUsers.map((user) => (
                  <div key={user.id} className="flex items-center justify-between p-4 border border-dark-border rounded-lg">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-full bg-primary-500 flex items-center justify-center text-white font-semibold">
                        {user.username[0]?.toUpperCase()}
                      </div>
                      <div>
                        <p className="font-medium text-dark-text">
                          {user.username}
                        </p>
                        <p className="text-sm text-dark-muted">@{user.username}</p>
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        onClick={() => handleApproveUser(user.id, true)}
                        className="bg-green-500 hover:bg-green-600"
                      >
                        <CheckCircle size={16} className="mr-1" />
                        Approve
                      </Button>
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => handleApproveUser(user.id, false)}
                        className="text-red-500 hover:text-red-600"
                      >
                        <XCircle size={16} className="mr-1" />
                        Reject
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8">
                <Clock size={48} className="mx-auto text-dark-muted mb-4" />
                <h3 className="text-lg font-medium text-dark-text mb-2">No pending approvals</h3>
                <p className="text-dark-muted">All users have been reviewed</p>
              </div>
            )}
          </div>
        </div>
      )}

      {activeTab === 'events' && (
        <div className="space-y-4">
          <div className="card">
            <h2 className="text-lg font-semibold text-dark-text mb-4">Event Management</h2>
            <div className="text-center py-8">
              <Calendar size={48} className="mx-auto text-dark-muted mb-4" />
              <h3 className="text-lg font-medium text-dark-text mb-2">Event management features</h3>
              <p className="text-dark-muted">Monitor and manage all platform events</p>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'stats' && (
        <div className="space-y-4">
          <div className="card">
            <h2 className="text-lg font-semibold text-dark-text mb-4">Platform Statistics</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h3 className="text-medium text-dark-text mb-3">User Activity</h3>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="text-dark-muted">Daily Active Users</span>
                    <span className="text-dark-text">45</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-dark-muted">Weekly Active Users</span>
                    <span className="text-dark-text">89</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-dark-muted">Monthly Active Users</span>
                    <span className="text-dark-text">124</span>
                  </div>
                </div>
              </div>
              <div>
                <h3 className="text-medium text-dark-text mb-3">Content Statistics</h3>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="text-dark-muted">Total Messages</span>
                    <span className="text-dark-text">{stats.totalMessages}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-dark-muted">Total Events</span>
                    <span className="text-dark-text">{stats.totalEvents}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-dark-muted">Upcoming Events</span>
                    <span className="text-dark-text">{stats.upcomingEvents}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminPage;
