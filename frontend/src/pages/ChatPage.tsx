import { useState, useEffect, useRef } from 'react';
import { Send, Search, Phone, Video, MoreVertical, MessageSquare } from 'lucide-react';
import { useGetUserChatsQuery } from '@/app/api/chatApi';
import { useSignalR } from '@/hooks/useSignalR';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '@/app/store';
import { setCurrentChat, setMessages, addMessage } from '@/app/slices/chatSlice';
import { Button } from '@/components/ui/button';
import { GlassInput } from '@/components/ui/GlassInput';
import LoadingSpinner from '@/components/ui/LoadingSpinner';

const ChatPage = () => {
  const dispatch = useDispatch();
  const { currentChat, messages, typingUsers } = useSelector((state: RootState) => state.chat);
  const [selectedChatId, setSelectedChatId] = useState<string | null>(null);
  const [message, setMessage] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  
  const { data: chats, isLoading } = useGetUserChatsQuery();
  const { sendMessage, joinChat, leaveChat, typing } = useSignalR();

  const filteredChats = chats?.filter(chat =>
    chat.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    chat.participants.some(p => p.username.toLowerCase().includes(searchQuery.toLowerCase()))
  ) || [];

  useEffect(() => {
    if (selectedChatId && chats) {
      const chat = chats.find(c => c.id === selectedChatId);
      if (chat) {
        dispatch(setCurrentChat(chat));
        joinChat(selectedChatId);
      }
    }

    return () => {
      if (selectedChatId) {
        leaveChat(selectedChatId);
      }
    };
  }, [selectedChatId, chats, dispatch, joinChat, leaveChat]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!message.trim() || !currentChat) return;

    const messageData = {
      chatId: currentChat.id,
      content: message.trim(),
      type: 'Text' as const,
    };

    try {
      await sendMessage(messageData);
      setMessage('');
    } catch (error) {
      console.error('Failed to send message:', error);
    }
  };

  const handleTyping = (isTyping: boolean) => {
    if (currentChat) {
      typing(currentChat.id, isTyping);
    }
  };

  const formatTime = (dateString: string) => {
    return new Date(dateString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="h-full flex">
      {/* Chat List Sidebar */}
      <div className="w-80 border-r border-dark-border flex flex-col">
        <div className="p-4 border-b border-dark-border">
          <h2 className="text-lg font-semibold text-dark-text mb-4">Messages</h2>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-dark-muted" size={20} />
            <Input
              placeholder="Search conversations..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        <div className="flex-1 overflow-y-auto">
          {filteredChats.length > 0 ? (
            filteredChats.map((chat) => (
              <div
                key={chat.id}
                onClick={() => setSelectedChatId(chat.id)}
                className={`p-4 border-b border-dark-border cursor-pointer transition-colors ${
                  selectedChatId === chat.id ? 'bg-dark-border' : 'hover:bg-dark-border'
                }`}
              >
                <div className="flex items-center gap-3">
                  <div className="w-12 h-12 rounded-full bg-primary-500 flex items-center justify-center text-white font-semibold">
                    {chat.type === 'Private' 
                      ? chat.participants[0]?.username?.[0]?.toUpperCase()
                      : chat.name[0]?.toUpperCase()
                    }
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <p className="font-medium text-dark-text truncate">
                        {chat.type === 'Private' 
                          ? chat.participants[0]?.username
                          : chat.name
                        }
                      </p>
                      <span className="text-xs text-dark-muted">
                        {formatTime(chat.lastMessageAt)}
                      </span>
                    </div>
                    <div className="flex items-center justify-between mt-1">
                      <p className="text-sm text-dark-muted truncate">
                        {chat.lastMessage?.content || 'No messages yet'}
                      </p>
                      {chat.unreadCount > 0 && (
                        <span className="bg-primary-500 text-white text-xs px-2 py-1 rounded-full">
                          {chat.unreadCount}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            ))
          ) : (
            <div className="p-8 text-center text-dark-muted">
              <p>No conversations found</p>
            </div>
          )}
        </div>
      </div>

      {/* Chat Window */}
      <div className="flex-1 flex flex-col">
        {currentChat ? (
          <>
            {/* Chat Header */}
            <div className="p-4 border-b border-dark-border">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-primary-500 flex items-center justify-center text-white font-semibold">
                    {currentChat.type === 'Private' 
                      ? currentChat.participants[0]?.username?.[0]?.toUpperCase()
                      : currentChat.name[0]?.toUpperCase()
                    }
                  </div>
                  <div>
                    <p className="font-medium text-dark-text">
                      {currentChat.type === 'Private' 
                        ? currentChat.participants[0]?.username
                        : currentChat.name
                      }
                    </p>
                    <p className="text-sm text-dark-muted">
                      {currentChat.participants.filter(p => p.isOnline).length} online
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Button variant="ghost" size="sm">
                    <Phone size={20} />
                  </Button>
                  <Button variant="ghost" size="sm">
                    <Video size={20} />
                  </Button>
                  <Button variant="ghost" size="sm">
                    <MoreVertical size={20} />
                  </Button>
                </div>
              </div>
            </div>

            {/* Messages Area */}
            <div className="flex-1 overflow-y-auto p-4">
              <div className="space-y-4">
                {messages.map((msg) => (
                  <div
                    key={msg.id}
                    className={`flex ${msg.sender.id === 'current-user' ? 'justify-end' : 'justify-start'}`}
                  >
                    <div className={`max-w-xs lg:max-w-md message-bubble ${
                      msg.sender.id === 'current-user' 
                        ? 'message-bubble-sent' 
                        : 'message-bubble-received'
                    }`}>
                      <p>{msg.content}</p>
                      <p className={`text-xs mt-1 ${
                        msg.sender.id === 'current-user' ? 'text-white/70' : 'text-dark-muted'
                      }`}>
                        {formatTime(msg.createdAt)}
                      </p>
                    </div>
                  </div>
                ))}
                
                {typingUsers.length > 0 && (
                  <div className="flex justify-start">
                    <div className="message-bubble-received">
                      <div className="flex gap-1">
                        <div className="w-2 h-2 bg-dark-muted rounded-full animate-bounce" />
                        <div className="w-2 h-2 bg-dark-muted rounded-full animate-bounce" style={{ animationDelay: '0.1s' }} />
                        <div className="w-2 h-2 bg-dark-muted rounded-full animate-bounce" style={{ animationDelay: '0.2s' }} />
                      </div>
                    </div>
                  </div>
                )}
                <div ref={messagesEndRef} />
              </div>
            </div>

            {/* Message Input */}
            <div className="p-4 border-t border-dark-border">
              <form onSubmit={handleSendMessage} className="flex gap-2">
                <Input
                  placeholder="Type a message..."
                  value={message}
                  onChange={(e) => setMessage(e.target.value)}
                  onFocus={() => handleTyping(true)}
                  onBlur={() => handleTyping(false)}
                  className="flex-1"
                />
                <Button type="submit" disabled={!message.trim()}>
                  <Send size={20} />
                </Button>
              </form>
            </div>
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center">
            <div className="text-center">
              <div className="w-20 h-20 rounded-full bg-dark-border flex items-center justify-center mx-auto mb-4">
                <MessageSquare size={40} className="text-dark-muted" />
              </div>
              <h3 className="text-lg font-medium text-dark-text mb-2">Select a conversation</h3>
              <p className="text-dark-muted">Choose a chat from the sidebar to start messaging</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ChatPage;
