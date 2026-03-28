import { useState } from 'react';
import { User, Edit2, Camera, Settings, LogOut } from 'lucide-react';
import { useGetProfileQuery, useUpdateProfileMutation } from '@/app/api/userApi';
import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/button';
import Input from '@/components/ui/Input';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

const ProfilePage = () => {
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    username: '',
    bio: '',
  });

  const { user, logout } = useAuth();
  const { data: profile, isLoading } = useGetProfileQuery();
  const [updateProfile] = useUpdateProfileMutation();

  useState(() => {
    if (profile) {
      setFormData({
        username: profile.username,
        bio: profile.bio || '',
      });
    }
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await updateProfile(formData).unwrap();
      setIsEditing(false);
    } catch (error) {
      console.error('Failed to update profile:', error);
    }
  };

  const handleLogout = () => {
    logout();
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-dark-text mb-2">Profile</h1>
        <p className="text-dark-muted">Manage your personal information</p>
      </div>

      <div className="card">
        <div className="flex items-start justify-between mb-6">
          <div className="flex items-center gap-4">
            <div className="relative">
              <div className="w-24 h-24 rounded-full bg-primary-500 flex items-center justify-center text-white text-2xl font-semibold">
                {profile?.username[0]?.toUpperCase()}
              </div>
              <button className="absolute bottom-0 right-0 bg-primary-500 text-white p-2 rounded-full hover:bg-primary-600 transition-colors">
                <Camera size={16} />
              </button>
            </div>
            <div>
              <h2 className="text-xl font-semibold text-dark-text">
                {profile?.username}
              </h2>
              <p className="text-dark-muted">@{profile?.username}</p>
              <p className="text-sm text-dark-muted mt-1">
                Status: <span className="text-green-500">{profile?.status}</span>
              </p>
            </div>
          </div>
          <div className="flex gap-2">
            <Button
              variant={isEditing ? "primary" : "ghost"}
              onClick={() => setIsEditing(!isEditing)}
            >
              <Edit2 size={20} className="mr-2" />
              {isEditing ? 'Cancel' : 'Edit Profile'}
            </Button>
            <Button variant="ghost" onClick={handleLogout}>
              <LogOut size={20} className="mr-2" />
              Logout
            </Button>
          </div>
        </div>

        {isEditing ? (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <Input
                label="Username"
                value={formData.username}
                onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                required
              />
            </div>
            <div>
              <Input
                label="Bio"
                value={formData.bio}
                onChange={(e) => setFormData({ ...formData, bio: e.target.value })}
                placeholder="Tell us about yourself"
                multiline
                rows={4}
              />
            </div>
            <div className="flex gap-2">
              <Button type="submit">Save Changes</Button>
              <Button type="button" variant="ghost" onClick={() => setIsEditing(false)}>
                Cancel
              </Button>
            </div>
          </form>
        ) : (
          <div className="space-y-6">
            <div>
              <h3 className="text-lg font-semibold text-dark-text mb-4">About</h3>
              <p className="text-dark-muted">
                {profile?.bio || 'No bio added yet'}
              </p>
            </div>

            <div>
              <h3 className="text-lg font-semibold text-dark-text mb-4">Account Information</h3>
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-dark-muted">Email</span>
                  <span className="text-dark-text">{profile?.email}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-dark-muted">Member Since</span>
                  <span className="text-dark-text">
                    {profile?.createdAt ? new Date(profile.createdAt).toLocaleDateString() : 'N/A'}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-dark-muted">Account Status</span>
                  <span className={`px-2 py-1 rounded text-xs ${
                    profile?.status === 'Approved' 
                      ? 'bg-green-500/20 text-green-500'
                      : 'bg-yellow-500/20 text-yellow-500'
                  }`}>
                    {profile?.status}
                  </span>
                </div>
              </div>
            </div>

            <div>
              <h3 className="text-lg font-semibold text-dark-text mb-4">Quick Actions</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Button variant="ghost" className="justify-start">
                  <Settings size={20} className="mr-2" />
                  Account Settings
                </Button>
                <Button variant="ghost" className="justify-start">
                  <User size={20} className="mr-2" />
                  Privacy Settings
                </Button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProfilePage;
