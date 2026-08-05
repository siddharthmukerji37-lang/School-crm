import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import noticeService from '../../services/noticeService';

export const fetchNotices = createAsyncThunk(
  'notices/fetchNotices',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await noticeService.getAll(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch notices'
      );
    }
  }
);

export const fetchPublishedNotices = createAsyncThunk(
  'notices/fetchPublishedNotices',
  async (_, { rejectWithValue }) => {
    try {
      const response = await noticeService.getPublished();
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch notices'
      );
    }
  }
);

export const fetchNoticeById = createAsyncThunk(
  'notices/fetchNoticeById',
  async (id, { rejectWithValue }) => {
    try {
      const response = await noticeService.getById(id);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch notice'
      );
    }
  }
);

export const createNotice = createAsyncThunk(
  'notices/createNotice',
  async (noticeData, { rejectWithValue }) => {
    try {
      const response = await noticeService.create(noticeData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create notice'
      );
    }
  }
);

export const updateNotice = createAsyncThunk(
  'notices/updateNotice',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await noticeService.update(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update notice'
      );
    }
  }
);

export const deleteNotice = createAsyncThunk(
  'notices/deleteNotice',
  async (id, { rejectWithValue }) => {
    try {
      await noticeService.delete(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete notice'
      );
    }
  }
);

const noticeSlice = createSlice({
  name: 'notices',
  initialState: {
    notices: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    publishedNotices: [],
    selectedNotice: null,
    loading: false,
    error: null,
  },
  reducers: {
    clearSelectedNotice: (state) => {
      state.selectedNotice = null;
    },
    clearNoticeError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchNotices.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchNotices.fulfilled, (state, action) => {
        state.loading = false;
        state.notices = action.payload;
      })
      .addCase(fetchNotices.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchPublishedNotices.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchPublishedNotices.fulfilled, (state, action) => {
        state.loading = false;
        state.publishedNotices = action.payload;
      })
      .addCase(fetchPublishedNotices.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchNoticeById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchNoticeById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedNotice = action.payload;
      })
      .addCase(fetchNoticeById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createNotice.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createNotice.fulfilled, (state, action) => {
        state.loading = false;
        state.notices.items.unshift(action.payload);
        state.notices.totalCount += 1;
      })
      .addCase(createNotice.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateNotice.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateNotice.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.notices.items.findIndex(
          (n) => n.id === action.payload.id
        );
        if (index !== -1) {
          state.notices.items[index] = action.payload;
        }
        if (state.selectedNotice?.id === action.payload.id) {
          state.selectedNotice = action.payload;
        }
      })
      .addCase(updateNotice.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteNotice.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteNotice.fulfilled, (state, action) => {
        state.loading = false;
        state.notices.items = state.notices.items.filter(
          (n) => n.id !== action.payload
        );
        state.notices.totalCount -= 1;
      })
      .addCase(deleteNotice.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearSelectedNotice, clearNoticeError } = noticeSlice.actions;
export default noticeSlice.reducer;
