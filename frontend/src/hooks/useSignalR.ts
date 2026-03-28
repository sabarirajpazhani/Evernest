import { useEffect, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../app/store';
import { addMessage, setConnected, addTypingUser, removeTypingUser } from '../app/slices/chatSlice';

export const useSignalR = () => {
  const dispatch = useDispatch();
  const { token } = useSelector((state: RootState) => state.auth);
  const connectionRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    if (!token) return;

    const connection = new HubConnectionBuilder()
      .withUrl('/chathub', {
        accessTokenFactory: () => token,
        transport: HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    connection.on('ReceiveMessage', (message) => {
      dispatch(addMessage(message));
    });

    connection.on('UserJoined', ({ userId, chatId }) => {
      console.log(`User ${userId} joined chat ${chatId}`);
    });

    connection.on('UserLeft', ({ userId, chatId }) => {
      console.log(`User ${userId} left chat ${chatId}`);
    });

    connection.on('UserTyping', ({ userId, chatId, isTyping }) => {
      if (isTyping) {
        dispatch(addTypingUser(userId));
      } else {
        dispatch(removeTypingUser(userId));
      }
    });

    connection.on('MessagesRead', ({ userId, chatId }) => {
      console.log(`Messages read by ${userId} in chat ${chatId}`);
    });

    connection.on('OnlineUsers', ({ chatId, users }) => {
      console.log('Online users updated:', users);
    });

    connection.on('Error', (error) => {
      console.error('SignalR error:', error);
    });

    connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      dispatch(setConnected(false));
    });

    connection.onreconnected(() => {
      console.log('SignalR reconnected');
      dispatch(setConnected(true));
    });

    connection.onclose(() => {
      console.log('SignalR connection closed');
      dispatch(setConnected(false));
    });

    const startConnection = async () => {
      try {
        await connection.start();
        console.log('SignalR connected');
        dispatch(setConnected(true));
      } catch (error) {
        console.error('SignalR connection error:', error);
        dispatch(setConnected(false));
      }
    };

    startConnection();

    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, [token, dispatch]);

  const joinChat = async (chatId: string) => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.invoke('JoinChat', chatId);
      } catch (error) {
        console.error('Error joining chat:', error);
      }
    }
  };

  const leaveChat = async (chatId: string) => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.invoke('LeaveChat', chatId);
      } catch (error) {
        console.error('Error leaving chat:', error);
      }
    }
  };

  const sendMessage = async (messageData: any) => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.invoke('SendMessage', messageData);
      } catch (error) {
        console.error('Error sending message:', error);
      }
    }
  };

  const markAsRead = async (chatId: string) => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.invoke('MarkAsRead', chatId);
      } catch (error) {
        console.error('Error marking as read:', error);
      }
    }
  };

  const typing = async (chatId: string, isTyping: boolean) => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.invoke('Typing', chatId, isTyping);
      } catch (error) {
        console.error('Error updating typing status:', error);
      }
    }
  };

  const getOnlineUsers = async (chatId: string) => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.invoke('GetOnlineUsers', chatId);
      } catch (error) {
        console.error('Error getting online users:', error);
      }
    }
  };

  return {
    joinChat,
    leaveChat,
    sendMessage,
    markAsRead,
    typing,
    getOnlineUsers,
  };
};
