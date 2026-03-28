import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { RootState } from '../store';
import { User } from './authApi';

export interface Message {
  id: string;
  chatId: string;
  sender: User;
  content: string;
  type: 'Text' | 'Image' | 'File' | 'Voice' | 'System';
  createdAt: string;
  status: 'Sent' | 'Delivered' | 'Read' | 'Failed';
  replyToMessage?: Message;
  isDeleted: boolean;
}

export interface Chat {
  id: string;
  type: 'Private' | 'Group';
  name: string;
  participants: User[];
  lastMessage?: Message;
  lastMessageAt: string;
  createdAt: string;
  groupPictureUrl?: string;
  createdBy: User;
  unreadCount: number;
  isTyping: boolean;
}

export interface CreateChatRequest {
  type: 'Private' | 'Group';
  name?: string;
  participantIds: string[];
}

export interface SendMessageRequest {
  chatId: string;
  content: string;
  type?: 'Text' | 'Image' | 'File' | 'Voice' | 'System';
  replyToMessageId?: string;
}

export const chatApi = createApi({
  reducerPath: 'chatApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/chat',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  endpoints: (builder) => ({
    createChat: builder.mutation<Chat, CreateChatRequest>({
      query: (chatData) => ({
        url: '',
        method: 'POST',
        body: chatData,
      }),
    }),
    getUserChats: builder.query<Chat[], void>({
      query: () => '',
    }),
    getChatById: builder.query<Chat, string>({
      query: (id) => `/${id}`,
    }),
    getChatMessages: builder.query<Message[], { chatId: string; limit?: number; lastMessageId?: string }>({
      query: ({ chatId, limit = 50, lastMessageId }) => ({
        url: `/${chatId}/messages`,
        params: { limit, lastMessageId },
      }),
    }),
    sendMessage: builder.mutation<Message, SendMessageRequest>({
      query: (messageData) => ({
        url: '/send-message',
        method: 'POST',
        body: messageData,
      }),
    }),
    deleteMessage: builder.mutation<boolean, string>({
      query: (messageId) => ({
        url: `/messages/${messageId}`,
        method: 'DELETE',
      }),
    }),
    markMessagesAsRead: builder.mutation<boolean, string>({
      query: (chatId) => ({
        url: `/${chatId}/mark-read`,
        method: 'POST',
      }),
    }),
    leaveChat: builder.mutation<boolean, string>({
      query: (chatId) => ({
        url: `/${chatId}/leave`,
        method: 'POST',
      }),
    }),
    addParticipant: builder.mutation<boolean, { chatId: string; participantId: string }>({
      query: ({ chatId, participantId }) => ({
        url: `/${chatId}/add-participant`,
        method: 'POST',
        body: participantId,
      }),
    }),
    removeParticipant: builder.mutation<boolean, { chatId: string; participantId: string }>({
      query: ({ chatId, participantId }) => ({
        url: `/${chatId}/remove-participant/${participantId}`,
        method: 'DELETE',
      }),
    }),
    updateTypingStatus: builder.mutation<boolean, { chatId: string; isTyping: boolean }>({
      query: ({ chatId, isTyping }) => ({
        url: `/${chatId}/typing`,
        method: 'POST',
        body: isTyping,
      }),
    }),
  }),
});

export const {
  useCreateChatMutation,
  useGetUserChatsQuery,
  useGetChatByIdQuery,
  useGetChatMessagesQuery,
  useSendMessageMutation,
  useDeleteMessageMutation,
  useMarkMessagesAsReadMutation,
  useLeaveChatMutation,
  useAddParticipantMutation,
  useRemoveParticipantMutation,
  useUpdateTypingStatusMutation,
} = chatApi;
