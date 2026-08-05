import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axiosInstance from '../../services/axiosInstance';

export const fetchParents = createAsyncThunk(
  'parents/fetchParents',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.get('/parents', { params });
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch parents'
      );
    }
  }
);

export const fetchParentById = createAsyncThunk(
  'parents/fetchParentById',
  async (id, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.get(`/parents/${id}`);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch parent'
      );
    }
  }
);

export const createParent = createAsyncThunk(
  'parents/createParent',
  async (parentData, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.post('/parents', parentData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create parent'
      );
    }
  }
);

export const updateParent = createAsyncThunk(
  'parents/updateParent',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.put(`/parents/${id}`, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update parent'
      );
    }
  }
);

export const deleteParent = createAsyncThunk(
  'parents/deleteParent',
  async (id, { rejectWithValue }) => {
    try {
      await axiosInstance.delete(`/parents/${id}`);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete parent'
      );
    }
  }
);

const parentSlice = createSlice({
  name: 'parents',
  initialState: {
    parents: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    selectedParent: null,
    loading: false,
    error: null,
  },
  reducers: {
    clearSelectedParent: (state) => {
      state.selectedParent = null;
    },
    clearParentError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchParents.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchParents.fulfilled, (state, action) => {
        state.loading = false;
        state.parents = action.payload;
      })
      .addCase(fetchParents.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchParentById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchParentById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedParent = action.payload;
      })
      .addCase(fetchParentById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createParent.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createParent.fulfilled, (state, action) => {
        state.loading = false;
        state.parents.items.push(action.payload);
        state.parents.totalCount += 1;
      })
      .addCase(createParent.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateParent.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateParent.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.parents.items.findIndex(
          (p) => p.id === action.payload.id
        );
        if (index !== -1) {
          state.parents.items[index] = action.payload;
        }
        if (state.selectedParent?.id === action.payload.id) {
          state.selectedParent = action.payload;
        }
      })
      .addCase(updateParent.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteParent.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteParent.fulfilled, (state, action) => {
        state.loading = false;
        state.parents.items = state.parents.items.filter(
          (p) => p.id !== action.payload
        );
        state.parents.totalCount -= 1;
      })
      .addCase(deleteParent.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearSelectedParent, clearParentError } = parentSlice.actions;
export default parentSlice.reducer;
