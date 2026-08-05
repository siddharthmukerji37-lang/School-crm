import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axiosInstance from '../../services/axiosInstance';

export const fetchExams = createAsyncThunk(
  'exams/fetchExams',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.get('/exams', { params });
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch exams'
      );
    }
  }
);

export const fetchExamById = createAsyncThunk(
  'exams/fetchExamById',
  async (id, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.get(`/exams/${id}`);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch exam'
      );
    }
  }
);

export const createExam = createAsyncThunk(
  'exams/createExam',
  async (examData, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.post('/exams', examData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create exam'
      );
    }
  }
);

export const updateExam = createAsyncThunk(
  'exams/updateExam',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.put(`/exams/${id}`, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update exam'
      );
    }
  }
);

export const deleteExam = createAsyncThunk(
  'exams/deleteExam',
  async (id, { rejectWithValue }) => {
    try {
      await axiosInstance.delete(`/exams/${id}`);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete exam'
      );
    }
  }
);

const examSlice = createSlice({
  name: 'exams',
  initialState: {
    exams: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    selectedExam: null,
    loading: false,
    error: null,
  },
  reducers: {
    clearSelectedExam: (state) => {
      state.selectedExam = null;
    },
    clearExamError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchExams.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchExams.fulfilled, (state, action) => {
        state.loading = false;
        state.exams = action.payload;
      })
      .addCase(fetchExams.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchExamById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchExamById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedExam = action.payload;
      })
      .addCase(fetchExamById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createExam.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createExam.fulfilled, (state, action) => {
        state.loading = false;
        state.exams.items.push(action.payload);
        state.exams.totalCount += 1;
      })
      .addCase(createExam.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateExam.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateExam.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.exams.items.findIndex(
          (e) => e.id === action.payload.id
        );
        if (index !== -1) {
          state.exams.items[index] = action.payload;
        }
        if (state.selectedExam?.id === action.payload.id) {
          state.selectedExam = action.payload;
        }
      })
      .addCase(updateExam.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteExam.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteExam.fulfilled, (state, action) => {
        state.loading = false;
        state.exams.items = state.exams.items.filter(
          (e) => e.id !== action.payload
        );
        state.exams.totalCount -= 1;
      })
      .addCase(deleteExam.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearSelectedExam, clearExamError } = examSlice.actions;
export default examSlice.reducer;
