import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { RootState } from '../store';
import { User } from './authApi';

export interface FriendRequest {
  id: string;
  sender: User;
  receiver: User;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled';
  createdAt: string;
  respondedAt?: string;
  message?: string;
}

export interface SendFriendRequestRequest {
  receiverId: string;
  message?: string;
}

export interface RespondFriendRequestRequest {
  status: 'Accepted' | 'Rejected';
}

export const friendApi = createApi({
  reducerPath: 'friendApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/friend',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  endpoints: (builder) => ({
    sendFriendRequest: builder.mutation<FriendRequest, SendFriendRequestRequest>({
      query: (requestData) => ({
        url: '/send-request',
        method: 'POST',
        body: requestData,
      }),
    }),
    getSentRequests: builder.query<FriendRequest[], void>({
      query: () => '/sent-requests',
    }),
    getReceivedRequests: builder.query<FriendRequest[], void>({
      query: () => '/received-requests',
    }),
    respondToFriendRequest: builder.mutation<FriendRequest, { requestId: string; response: RespondFriendRequestRequest }>({
      query: ({ requestId, response }) => ({
        url: `/respond/${requestId}`,
        method: 'POST',
        body: response,
      }),
    }),
    cancelFriendRequest: builder.mutation<boolean, string>({
      query: (requestId) => ({
        url: `/cancel/${requestId}`,
        method: 'POST',
      }),
    }),
    getFriends: builder.query<User[], void>({
      query: () => '/list',
    }),
    removeFriend: builder.mutation<boolean, string>({
      query: (friendId) => ({
        url: `/remove/${friendId}`,
        method: 'DELETE',
      }),
    }),
    isFriend: builder.query<boolean, string>({
      query: (friendId) => `/is-friend/${friendId}`,
    }),
  }),
});

export const {
  useSendFriendRequestMutation,
  useGetSentRequestsQuery,
  useGetReceivedRequestsQuery,
  useRespondToFriendRequestMutation,
  useCancelFriendRequestMutation,
  useGetFriendsQuery,
  useRemoveFriendMutation,
  useIsFriendQuery,
} = friendApi;
