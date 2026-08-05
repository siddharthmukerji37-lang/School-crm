import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axiosInstance from '../../services/axiosInstance';

export const fetchFees = createAsyncThunk(
  'fees/fetchFees',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.get('/fees', { params });
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch fees'
      );
    }
  }
);

export const collectFee = createAsyncThunk(
  'fees/collectFee',
  async (feeData, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.post('/fees/collect', feeData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to collect fee'
      );
    }
  }
);

export const fetchReceipts = createAsyncThunk(
  'fees/fetchReceipts',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.get('/fees/receipts', { params });
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch receipts'
      );
    }
  }
);

export const fetchFeeReport = createAsyncThunk(
  'fees/fetchFeeReport',
  async (params, { rejectWithValue }) => {
    try {
      const response = await axiosInstance.get('/fees/report', { params });
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch fee report'
      );
    }
  }
);

const feeSlice = createSlice({
  name: 'fees',
  initialState: {
    fees: [],
    receipts: [],
    report: null,
    loading: false,
    error: null,
  },
  reducers: {
    clearFeeError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchFees.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchFees.fulfilled, (state, action) => {
        state.loading = false;
        state.fees = action.payload;
      })
      .addCase(fetchFees.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(collectFee.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(collectFee.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(collectFee.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchReceipts.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchReceipts.fulfilled, (state, action) => {
        state.loading = false;
        state.receipts = action.payload;
      })
      .addCase(fetchReceipts.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchFeeReport.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchFeeReport.fulfilled, (state, action) => {
        state.loading = false;
        state.report = action.payload;
      })
      .addCase(fetchFeeReport.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearFeeError } = feeSlice.actions;
export default feeSlice.reducer;
