import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import reportService from '../../services/reportService';

export const fetchReportTemplates = createAsyncThunk(
  'reports/fetchReportTemplates',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await reportService.getTemplates(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch report templates'
      );
    }
  }
);

const reportSlice = createSlice({
  name: 'reports',
  initialState: {
    templates: [],
    loading: false,
    error: null,
  },
  reducers: {
    clearReportError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchReportTemplates.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchReportTemplates.fulfilled, (state, action) => {
        state.loading = false;
        state.templates = action.payload;
      })
      .addCase(fetchReportTemplates.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearReportError } = reportSlice.actions;
export default reportSlice.reducer;
