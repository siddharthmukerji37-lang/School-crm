import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import teacherService from '../../services/teacherService';

export const fetchTeachers = createAsyncThunk(
  'teachers/fetchTeachers',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await teacherService.getAll(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch teachers'
      );
    }
  }
);

export const fetchTeacherById = createAsyncThunk(
  'teachers/fetchTeacherById',
  async (id, { rejectWithValue }) => {
    try {
      const response = await teacherService.getById(id);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch teacher'
      );
    }
  }
);

export const createTeacher = createAsyncThunk(
  'teachers/createTeacher',
  async (teacherData, { rejectWithValue }) => {
    try {
      const response = await teacherService.create(teacherData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create teacher'
      );
    }
  }
);

export const updateTeacher = createAsyncThunk(
  'teachers/updateTeacher',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await teacherService.update(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update teacher'
      );
    }
  }
);

export const deleteTeacher = createAsyncThunk(
  'teachers/deleteTeacher',
  async (id, { rejectWithValue }) => {
    try {
      await teacherService.delete(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete teacher'
      );
    }
  }
);

const teacherSlice = createSlice({
  name: 'teachers',
  initialState: {
    teachers: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    selectedTeacher: null,
    loading: false,
    error: null,
  },
  reducers: {
    clearSelectedTeacher: (state) => {
      state.selectedTeacher = null;
    },
    clearTeacherError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchTeachers.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTeachers.fulfilled, (state, action) => {
        state.loading = false;
        state.teachers = action.payload;
      })
      .addCase(fetchTeachers.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchTeacherById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchTeacherById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedTeacher = action.payload;
      })
      .addCase(fetchTeacherById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createTeacher.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createTeacher.fulfilled, (state, action) => {
        state.loading = false;
        state.teachers.items.push(action.payload);
        state.teachers.totalCount += 1;
      })
      .addCase(createTeacher.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateTeacher.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateTeacher.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.teachers.items.findIndex(
          (t) => t.id === action.payload.id
        );
        if (index !== -1) {
          state.teachers.items[index] = action.payload;
        }
        if (state.selectedTeacher?.id === action.payload.id) {
          state.selectedTeacher = action.payload;
        }
      })
      .addCase(updateTeacher.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteTeacher.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteTeacher.fulfilled, (state, action) => {
        state.loading = false;
        state.teachers.items = state.teachers.items.filter(
          (t) => t.id !== action.payload
        );
        state.teachers.totalCount -= 1;
      })
      .addCase(deleteTeacher.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearSelectedTeacher, clearTeacherError } = teacherSlice.actions;
export default teacherSlice.reducer;
