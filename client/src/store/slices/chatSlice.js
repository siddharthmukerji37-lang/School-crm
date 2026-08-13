import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import chatService from '../../services/chatService';

export const fetchConversations = createAsyncThunk(
  'chat/fetchConversations',
  async (_, { rejectWithValue }) => {
    try {
      const response = await chatService.getConversations();
      return response.data.data;
    } catch (error) {
      return rejectWithValue(error.message || 'Failed to load conversations');
    }
  }
);

export const fetchMessages = createAsyncThunk(
  'chat/fetchMessages',
  async ({ peerUserId, sectionId } = {}, { rejectWithValue }) => {
    try {
      const response = await chatService.getMessages({ peerUserId, sectionId, pageSize: 100 });
      return response.data.data;
    } catch (error) {
      return rejectWithValue(error.message || 'Failed to load messages');
    }
  }
);

export const sendMessage = createAsyncThunk(
  'chat/sendMessage',
  async (payload, { rejectWithValue }) => {
    try {
      const response = await chatService.sendMessage(payload);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(error.message || 'Failed to send message');
    }
  }
);

export const markChatRead = createAsyncThunk(
  'chat/markChatRead',
  async (peerUserId, { rejectWithValue }) => {
    try {
      await chatService.markRead(peerUserId);
      return peerUserId;
    } catch (error) {
      return rejectWithValue(error.message || 'Failed to mark chat as read');
    }
  }
);

export const fetchAvailableUsers = createAsyncThunk(
  'chat/fetchAvailableUsers',
  async ({ role, search } = {}, { rejectWithValue }) => {
    try {
      const response = await chatService.getUsers({ role, search });
      return response.data.data;
    } catch (error) {
      return rejectWithValue(error.message || 'Failed to load users');
    }
  }
);

const recomputeUnreadTotal = (state) => {
  state.unreadTotal = state.conversations.reduce(
    (sum, conv) => sum + (conv.unreadCount || 0),
    0
  );
};

const chatSlice = createSlice({
  name: 'chat',
  initialState: {
    conversations: [],
    messages: [],
    activeConversation: null,
    availableUsers: [],
    unreadTotal: 0,
    loading: false,
    sending: false,
    error: null,
  },
  reducers: {
    setActiveConversation: (state, action) => {
      state.activeConversation = action.payload;
      state.messages = [];
      if (action.payload?.type === 'Direct') {
        const conv = state.conversations.find(
          (c) => c.id === action.payload.id
        );
        if (conv) {
          conv.unreadCount = 0;
          recomputeUnreadTotal(state);
        }
      }
    },
    receiveMessage: (state, action) => {
      const { message: msg, currentUserId } = action.payload;
      if (!msg || !msg.id) return;

      const active = state.activeConversation;
      let belongsToActive = false;
      if (active) {
        if (active.type === 'Class') {
          belongsToActive =
            active.sectionId === msg.sectionId && msg.messageType === 'Class';
        } else {
          belongsToActive =
            active.peerUserId === msg.senderId ||
            active.peerUserId === msg.receiverId;
        }
      }

      if (belongsToActive && !state.messages.some((m) => m.id === msg.id)) {
        state.messages.push(msg);
      }

      const isMine = msg.senderId === currentUserId;
      const convId =
        msg.messageType === 'Class'
          ? `class:${msg.sectionId}`
          : isMine
            ? `direct:${msg.receiverId}`
            : `direct:${msg.senderId}`;

      const conv = state.conversations.find((c) => c.id === convId);
      if (conv) {
        conv.lastMessage = msg.message;
        conv.lastMessageAt = msg.createdAt;
        conv.lastSenderId = msg.senderId;
        if (belongsToActive) {
          conv.unreadCount = 0;
        } else if (!isMine) {
          conv.unreadCount = (conv.unreadCount || 0) + 1;
        }
        recomputeUnreadTotal(state);
      }
    },
    clearChatError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchConversations.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchConversations.fulfilled, (state, action) => {
        state.loading = false;
        state.conversations = action.payload || [];
        recomputeUnreadTotal(state);
      })
      .addCase(fetchConversations.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchMessages.pending, (state) => {
        state.error = null;
      })
      .addCase(fetchMessages.fulfilled, (state, action) => {
        state.messages = action.payload || [];
        const active = state.activeConversation;
        if (active?.type === 'Direct') {
          const conv = state.conversations.find((c) => c.id === active.id);
          if (conv) {
            conv.unreadCount = 0;
            recomputeUnreadTotal(state);
          }
        }
      })
      .addCase(fetchMessages.rejected, (state, action) => {
        state.error = action.payload;
      })
      .addCase(sendMessage.pending, (state) => {
        state.sending = true;
        state.error = null;
      })
      .addCase(sendMessage.fulfilled, (state, action) => {
        state.sending = false;
        const msg = action.payload;
        const active = state.activeConversation;
        if (!msg || !active) return;

        let matches = false;
        if (active.type === 'Class') {
          matches = active.sectionId === msg.sectionId && msg.messageType === 'Class';
        } else {
          matches = active.peerUserId === msg.receiverId || active.peerUserId === msg.senderId;
        }
        if (matches && !state.messages.some((m) => m.id === msg.id)) {
          state.messages.push(msg);
        }

        const convId =
          msg.messageType === 'Class'
            ? `class:${msg.sectionId}`
            : `direct:${msg.receiverId}`;
        const conv = state.conversations.find((c) => c.id === convId);
        if (conv) {
          conv.lastMessage = msg.message;
          conv.lastMessageAt = msg.createdAt;
          conv.lastSenderId = msg.senderId;
          conv.unreadCount = 0;
        }
      })
      .addCase(sendMessage.rejected, (state, action) => {
        state.sending = false;
        state.error = action.payload;
      })
      .addCase(markChatRead.fulfilled, (state, action) => {
        const peerId = action.payload;
        const conv = state.conversations.find(
          (c) => c.type === 'Direct' && c.peerUserId === peerId
        );
        if (conv) {
          conv.unreadCount = 0;
          recomputeUnreadTotal(state);
        }
      })
      .addCase(fetchAvailableUsers.pending, (state) => {
        state.error = null;
      })
      .addCase(fetchAvailableUsers.fulfilled, (state, action) => {
        state.availableUsers = action.payload || [];
      })
      .addCase(fetchAvailableUsers.rejected, (state, action) => {
        state.error = action.payload;
      });
  },
});

export const { setActiveConversation, receiveMessage, clearChatError } =
  chatSlice.actions;
export default chatSlice.reducer;
