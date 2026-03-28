import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { RootState } from '../store';
import { User } from './authApi';

export interface Event {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  location: string;
  createdBy: User;
  attendees: User[];
  invitedUsers: User[];
  status: 'Upcoming' | 'Ongoing' | 'Completed' | 'Cancelled';
  createdAt: string;
  eventPictureUrl?: string;
  isPrivate: boolean;
  userRSVP: 'Pending' | 'Attending' | 'NotAttending' | 'Maybe';
  attendingCount: number;
  invitedCount: number;
}

export interface CreateEventRequest {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  location: string;
  inviteUserIds?: string[];
  isPrivate?: boolean;
}

export interface UpdateEventRequest {
  title?: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  location?: string;
  status?: 'Upcoming' | 'Ongoing' | 'Completed' | 'Cancelled';
}

export interface RSVPRequest {
  status: 'Attending' | 'NotAttending' | 'Maybe';
}

export const eventApi = createApi({
  reducerPath: 'eventApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/event',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  endpoints: (builder) => ({
    createEvent: builder.mutation<Event, CreateEventRequest>({
      query: (eventData) => ({
        url: '',
        method: 'POST',
        body: eventData,
      }),
    }),
    getEventById: builder.query<Event, string>({
      query: (id) => `/${id}`,
    }),
    getUserEvents: builder.query<Event[], void>({
      query: () => '',
    }),
    getCreatedEvents: builder.query<Event[], void>({
      query: () => '/created',
    }),
    getUpcomingEvents: builder.query<Event[], void>({
      query: () => '/upcoming',
    }),
    getInvitedEvents: builder.query<Event[], void>({
      query: () => '/invited',
    }),
    updateEvent: builder.mutation<Event, { eventId: string; updateData: UpdateEventRequest }>({
      query: ({ eventId, updateData }) => ({
        url: `/${eventId}`,
        method: 'PUT',
        body: updateData,
      }),
    }),
    deleteEvent: builder.mutation<boolean, string>({
      query: (eventId) => ({
        url: `/${eventId}`,
        method: 'DELETE',
      }),
    }),
    inviteUsers: builder.mutation<boolean, { eventId: string; userIds: string[] }>({
      query: ({ eventId, userIds }) => ({
        url: `/${eventId}/invite`,
        method: 'POST',
        body: userIds,
      }),
    }),
    rsvpEvent: builder.mutation<boolean, { eventId: string; rsvpData: RSVPRequest }>({
      query: ({ eventId, rsvpData }) => ({
        url: `/${eventId}/rsvp`,
        method: 'POST',
        body: rsvpData,
      }),
    }),
    cancelRSVP: builder.mutation<boolean, string>({
      query: (eventId) => ({
        url: `/${eventId}/cancel-rsvp`,
        method: 'POST',
      }),
    }),
  }),
});

export const {
  useCreateEventMutation,
  useGetEventByIdQuery,
  useGetUserEventsQuery,
  useGetCreatedEventsQuery,
  useGetUpcomingEventsQuery,
  useGetInvitedEventsQuery,
  useUpdateEventMutation,
  useDeleteEventMutation,
  useInviteUsersMutation,
  useRsvpEventMutation,
  useCancelRSVPMutation,
} = eventApi;
