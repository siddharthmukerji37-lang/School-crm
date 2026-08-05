import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import dashboardService from '../../services/dashboardService';

export const fetchDashboardStats = createAsyncThunk(
  'dashboard/fetchStats',
  async (_, { rejectWithValue }) => {
    try {
      const response = await dashboardService.getStats();
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch dashboard stats'
      );
    }
  }
);

export const fetchAttendanceChart = createAsyncThunk(
  'dashboard/fetchAttendanceChart',
  async (params, { rejectWithValue }) => {
    try {
      const response = await dashboardService.getAttendanceChart(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch attendance chart'
      );
    }
  }
);

export const fetchFeeChart = createAsyncThunk(
  'dashboard/fetchFeeChart',
  async (params, { rejectWithValue }) => {
    try {
      const response = await dashboardService.getFeeChart(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch fee chart'
      );
    }
  }
);

const dashboardSlice = createSlice({
  name: 'dashboard',
  initialState: {
    stats: null,
    attendanceChart: null,
    feeChart: null,
    loading: false,
    error: null,
  },
  reducers: {
    clearDashboardError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchDashboardStats.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchDashboardStats.fulfilled, (state, action) => {
        state.loading = false;
        state.stats = action.payload;
      })
      .addCase(fetchDashboardStats.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchAttendanceChart.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchAttendanceChart.fulfilled, (state, action) => {
        state.loading = false;
        state.attendanceChart = action.payload;
      })
      .addCase(fetchAttendanceChart.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchFeeChart.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchFeeChart.fulfilled, (state, action) => {
        state.loading = false;
        state.feeChart = action.payload;
      })
      .addCase(fetchFeeChart.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearDashboardError } = dashboardSlice.actions;
export default dashboardSlice.reducer;
