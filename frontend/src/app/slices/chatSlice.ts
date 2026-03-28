import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface Message {
  id: string;
  chatId: string;
  sender: {
    id: string;
    username: string;
    profilePictureUrl?: string;
  };
  content: string;
  type: 'Text' | 'Image' | 'File' | 'Voice' | 'System';
  createdAt: string;
  status: 'Sent' | 'Delivered' | 'Read' | 'Failed';
  replyToMessage?: Message;
  isDeleted: boolean;
}

interface Chat {
  id: string;
  type: 'Private' | 'Group';
  name: string;
  participants: Array<{
    id: string;
    username: string;
    profilePictureUrl?: string;
    isOnline: boolean;
  }>;
  lastMessage?: Message;
  lastMessageAt: string;
  createdAt: string;
  groupPictureUrl?: string;
  createdBy: {
    id: string;
    username: string;
  };
  unreadCount: number;
  isTyping: boolean;
}

interface ChatState {
  chats: Chat[];
  currentChat: Chat | null;
  messages: Message[];
  isLoading: boolean;
  isConnected: boolean;
  typingUsers: string[];
}

const initialState: ChatState = {
  chats: [],
  currentChat: null,
  messages: [],
  isLoading: false,
  isConnected: false,
  typingUsers: [],
};

const chatSlice = createSlice({
  name: 'chat',
  initialState,
  reducers: {
    setChats: (state, action: PayloadAction<Chat[]>) => {
      state.chats = action.payload;
    },
    setCurrentChat: (state, action: PayloadAction<Chat | null>) => {
      state.currentChat = action.payload;
      state.messages = [];
    },
    setMessages: (state, action: PayloadAction<Message[]>) => {
      state.messages = action.payload;
    },
    addMessage: (state, action: PayloadAction<Message>) => {
      state.messages.push(action.payload);
      
      // Update last message in chat
      const chat = state.chats.find(c => c.id === action.payload.chatId);
      if (chat) {
        chat.lastMessage = action.payload;
        chat.lastMessageAt = action.payload.createdAt;
      }
      
      // Update current chat if it's the active one
      if (state.currentChat?.id === action.payload.chatId) {
        state.currentChat.lastMessage = action.payload;
        state.currentChat.lastMessageAt = action.payload.createdAt;
      }
    },
    updateMessage: (state, action: PayloadAction<Message>) => {
      const index = state.messages.findIndex(m => m.id === action.payload.id);
      if (index !== -1) {
        state.messages[index] = action.payload;
      }
    },
    removeMessage: (state, action: PayloadAction<string>) => {
      const index = state.messages.findIndex(m => m.id === action.payload);
      if (index !== -1) {
        state.messages[index].isDeleted = true;
      }
    },
    setTypingUsers: (state, action: PayloadAction<string[]>) => {
      state.typingUsers = action.payload;
    },
    addTypingUser: (state, action: PayloadAction<string>) => {
      if (!state.typingUsers.includes(action.payload)) {
        state.typingUsers.push(action.payload);
      }
    },
    removeTypingUser: (state, action: PayloadAction<string>) => {
      state.typingUsers = state.typingUsers.filter(id => id !== action.payload);
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
    },
    setConnected: (state, action: PayloadAction<boolean>) => {
      state.isConnected = action.payload;
    },
    updateChatUnreadCount: (state, action: PayloadAction<{ chatId: string; count: number }>) => {
      const chat = state.chats.find(c => c.id === action.payload.chatId);
      if (chat) {
        chat.unreadCount = action.payload.count;
      }
    },
  },
});

export const {
  setChats,
  setCurrentChat,
  setMessages,
  addMessage,
  updateMessage,
  removeMessage,
  setTypingUsers,
  addTypingUser,
  removeTypingUser,
  setLoading,
  setConnected,
  updateChatUnreadCount,
} = chatSlice.actions;

export default chatSlice.reducer;
