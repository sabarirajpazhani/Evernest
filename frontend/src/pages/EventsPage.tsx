import { useState } from 'react';
import { CalendarPlus, Calendar, MapPin, Users, Clock } from 'lucide-react';
import { useGetUpcomingEventsQuery, useCreateEventMutation } from '@/app/api/eventApi';
import { Button } from '@/components/ui/button';
import Input from '@/components/ui/Input';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

const EventsPage = () => {
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    location: '',
    isPrivate: true,
  });

  const { data: events, isLoading } = useGetUpcomingEventsQuery();
  const [createEvent] = useCreateEventMutation();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createEvent({
        ...formData,
        startDate: formData.startDate,
        endDate: formData.endDate,
      }).unwrap();
      setShowCreateForm(false);
      setFormData({
        title: '',
        description: '',
        startDate: '',
        endDate: '',
        location: '',
        isPrivate: true,
      });
    } catch (error) {
      console.error('Failed to create event:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-dark-text mb-2">Events</h1>
          <p className="text-dark-muted">Discover and join events</p>
        </div>
        <Button onClick={() => setShowCreateForm(!showCreateForm)}>
          <CalendarPlus size={20} className="mr-2" />
          Create Event
        </Button>
      </div>

      {showCreateForm && (
        <div className="card">
          <h2 className="text-lg font-semibold text-dark-text mb-4">Create New Event</h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <Input
                label="Event Title"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                required
              />
            </div>
            <div>
              <Input
                label="Description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                multiline
                rows={3}
                required
              />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Input
                  label="Start Date & Time"
                  type="datetime-local"
                  value={formData.startDate}
                  onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                  required
                />
              </div>
              <div>
                <Input
                  label="End Date & Time"
                  type="datetime-local"
                  value={formData.endDate}
                  onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                  required
                />
              </div>
            </div>
            <div>
              <Input
                label="Location"
                value={formData.location}
                onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                required
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isPrivate"
                checked={formData.isPrivate}
                onChange={(e) => setFormData({ ...formData, isPrivate: e.target.checked })}
                className="rounded"
              />
              <label htmlFor="isPrivate" className="text-dark-text">
                Private Event (invite only)
              </label>
            </div>
            <div className="flex gap-2">
              <Button type="submit">Create Event</Button>
              <Button type="button" variant="ghost" onClick={() => setShowCreateForm(false)}>
                Cancel
              </Button>
            </div>
          </form>
        </div>
      )}

      <div className="space-y-4">
        {events && events.length > 0 ? (
          events.map((event) => (
            <div key={event.id} className="card">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <h3 className="text-lg font-semibold text-dark-text mb-2">
                    {event.title}
                  </h3>
                  <p className="text-dark-muted mb-4">{event.description}</p>
                  <div className="flex flex-wrap gap-4 text-sm text-dark-muted">
                    <div className="flex items-center gap-1">
                      <Calendar size={16} />
                      {new Date(event.startDate).toLocaleDateString()} at{' '}
                      {new Date(event.startDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </div>
                    <div className="flex items-center gap-1">
                      <MapPin size={16} />
                      {event.location}
                    </div>
                    <div className="flex items-center gap-1">
                      <Users size={16} />
                      {event.attendingCount || 0} attending
                    </div>
                    {event.isPrivate && (
                      <div className="flex items-center gap-1">
                        <Clock size={16} />
                        Private
                      </div>
                    )}
                  </div>
                </div>
                <div className="flex gap-2 ml-4">
                  <Button variant="ghost" size="sm">
                    Attend
                  </Button>
                  <Button variant="ghost" size="sm">
                    Maybe
                  </Button>
                </div>
              </div>
            </div>
          ))
        ) : (
          <div className="text-center py-12">
            <Calendar size={48} className="mx-auto text-dark-muted mb-4" />
            <h3 className="text-lg font-medium text-dark-text mb-2">No upcoming events</h3>
            <p className="text-dark-muted mb-4">Create an event or wait for invitations</p>
            <Button onClick={() => setShowCreateForm(true)}>
              <CalendarPlus size={20} className="mr-2" />
              Create Your First Event
            </Button>
          </div>
        )}
      </div>
    </div>
  );
};

export default EventsPage;
