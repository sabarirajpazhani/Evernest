import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { RootState } from '../store';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
  confirmPassword: string;
}

export interface User {
  id: string;
  email: string;
  username: string;
  bio?: string;
  profilePictureUrl?: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Suspended';
  role: 'User' | 'Admin';
  createdAt: string;
  lastActiveAt?: string;
  isOnline: boolean;
  friendCount: number;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string;
}

export const authApi = createApi({
  reducerPath: 'authApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/auth',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/login',
        method: 'POST',
        body: credentials,
      }),
    }),
    register: builder.mutation<AuthResponse, RegisterRequest>({
      query: (userData) => ({
        url: '/register',
        method: 'POST',
        body: userData,
      }),
    }),
    validateToken: builder.mutation<boolean, string>({
      query: (token) => ({
        url: '/validate',
        method: 'POST',
        body: token,
      }),
    }),
  }),
});

export const { useLoginMutation, useRegisterMutation, useValidateTokenMutation } = authApi;
