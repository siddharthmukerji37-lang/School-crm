import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import homeworkService from '../../services/homeworkService';

export const fetchHomework = createAsyncThunk(
  'homework/fetchHomework',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await homeworkService.getAll(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch homework'
      );
    }
  }
);

export const fetchHomeworkById = createAsyncThunk(
  'homework/fetchHomeworkById',
  async (id, { rejectWithValue }) => {
    try {
      const response = await homeworkService.getById(id);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch homework'
      );
    }
  }
);

export const createHomework = createAsyncThunk(
  'homework/createHomework',
  async (homeworkData, { rejectWithValue }) => {
    try {
      const response = await homeworkService.create(homeworkData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create homework'
      );
    }
  }
);

export const updateHomework = createAsyncThunk(
  'homework/updateHomework',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await homeworkService.update(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update homework'
      );
    }
  }
);

export const deleteHomework = createAsyncThunk(
  'homework/deleteHomework',
  async (id, { rejectWithValue }) => {
    try {
      await homeworkService.delete(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete homework'
      );
    }
  }
);

const homeworkSlice = createSlice({
  name: 'homework',
  initialState: {
    homework: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    selectedHomework: null,
    loading: false,
    error: null,
  },
  reducers: {
    clearSelectedHomework: (state) => {
      state.selectedHomework = null;
    },
    clearHomeworkError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchHomework.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchHomework.fulfilled, (state, action) => {
        state.loading = false;
        state.homework = action.payload;
      })
      .addCase(fetchHomework.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchHomeworkById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchHomeworkById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedHomework = action.payload;
      })
      .addCase(fetchHomeworkById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createHomework.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createHomework.fulfilled, (state, action) => {
        state.loading = false;
        state.homework.items.push(action.payload);
        state.homework.totalCount += 1;
      })
      .addCase(createHomework.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateHomework.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateHomework.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.homework.items.findIndex(
          (h) => h.id === action.payload.id
        );
        if (index !== -1) {
          state.homework.items[index] = action.payload;
        }
        if (state.selectedHomework?.id === action.payload.id) {
          state.selectedHomework = action.payload;
        }
      })
      .addCase(updateHomework.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteHomework.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteHomework.fulfilled, (state, action) => {
        state.loading = false;
        state.homework.items = state.homework.items.filter(
          (h) => h.id !== action.payload
        );
        state.homework.totalCount -= 1;
      })
      .addCase(deleteHomework.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearSelectedHomework, clearHomeworkError } = homeworkSlice.actions;
export default homeworkSlice.reducer;
