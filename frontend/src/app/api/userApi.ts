import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { RootState } from '../store';
import { User } from './authApi';

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  username?: string;
  bio?: string;
  profilePictureUrl?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ApproveUserRequest {
  status: 'Pending' | 'Approved' | 'Rejected' | 'Suspended';
}

export const userApi = createApi({
  reducerPath: 'userApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/user',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  endpoints: (builder) => ({
    getProfile: builder.query<User, void>({
      query: () => '/profile',
    }),
    getUserById: builder.query<User, string>({
      query: (id) => `/${id}`,
    }),
    getAllUsers: builder.query<User[], void>({
      query: () => '',
    }),
    getPendingUsers: builder.query<User[], void>({
      query: () => '/pending',
    }),
    getApprovedUsers: builder.query<User[], void>({
      query: () => '/approved',
    }),
    searchUsers: builder.query<User[], string>({
      query: (query) => `/search?query=${query}`,
    }),
    updateProfile: builder.mutation<User, UpdateUserRequest>({
      query: (userData) => ({
        url: '/profile',
        method: 'PUT',
        body: userData,
      }),
    }),
    changePassword: builder.mutation<boolean, ChangePasswordRequest>({
      query: (passwordData) => ({
        url: '/change-password',
        method: 'POST',
        body: passwordData,
      }),
    }),
    approveUser: builder.mutation<boolean, { id: string; status: { status: 'Approved' | 'Rejected' } }>({
      query: ({ id, status }) => ({
        url: `/approve/${id}`,
        method: 'POST',
        body: status,
      }),
    }),
    deleteUser: builder.mutation<boolean, string>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
    }),
    updateProfilePicture: builder.mutation<User, string>({
      query: (pictureUrl) => ({
        url: '/update-profile-picture',
        method: 'POST',
        body: pictureUrl,
      }),
    }),
    updateLastActive: builder.mutation<boolean, void>({
      query: () => ({
        url: '/update-last-active',
        method: 'POST',
      }),
    }),
    updateOnlineStatus: builder.mutation<boolean, boolean>({
      query: (isOnline) => ({
        url: '/update-online-status',
        method: 'POST',
        body: isOnline,
      }),
    }),
  }),
});

export const {
  useGetProfileQuery,
  useGetUserByIdQuery,
  useGetAllUsersQuery,
  useGetPendingUsersQuery,
  useGetApprovedUsersQuery,
  useSearchUsersQuery,
  useUpdateProfileMutation,
  useChangePasswordMutation,
  useApproveUserMutation,
} = userApi;
