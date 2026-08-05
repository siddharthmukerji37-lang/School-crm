import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import accountService from '../../services/accountService';

export const fetchIncome = createAsyncThunk(
  'accounts/fetchIncome',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await accountService.getIncome(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch income'
      );
    }
  }
);

export const createIncome = createAsyncThunk(
  'accounts/createIncome',
  async (incomeData, { rejectWithValue }) => {
    try {
      const response = await accountService.createIncome(incomeData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create income'
      );
    }
  }
);

export const updateIncome = createAsyncThunk(
  'accounts/updateIncome',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await accountService.updateIncome(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update income'
      );
    }
  }
);

export const deleteIncome = createAsyncThunk(
  'accounts/deleteIncome',
  async (id, { rejectWithValue }) => {
    try {
      await accountService.deleteIncome(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete income'
      );
    }
  }
);

export const fetchExpense = createAsyncThunk(
  'accounts/fetchExpense',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await accountService.getExpense(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch expense'
      );
    }
  }
);

export const createExpense = createAsyncThunk(
  'accounts/createExpense',
  async (expenseData, { rejectWithValue }) => {
    try {
      const response = await accountService.createExpense(expenseData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create expense'
      );
    }
  }
);

export const updateExpense = createAsyncThunk(
  'accounts/updateExpense',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await accountService.updateExpense(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update expense'
      );
    }
  }
);

export const deleteExpense = createAsyncThunk(
  'accounts/deleteExpense',
  async (id, { rejectWithValue }) => {
    try {
      await accountService.deleteExpense(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete expense'
      );
    }
  }
);

export const fetchLedger = createAsyncThunk(
  'accounts/fetchLedger',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await accountService.getLedger(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch ledger'
      );
    }
  }
);

const accountSlice = createSlice({
  name: 'accounts',
  initialState: {
    income: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    expense: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    ledger: [],
    loading: false,
    error: null,
  },
  reducers: {
    clearAccountError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchIncome.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchIncome.fulfilled, (state, action) => {
        state.loading = false;
        state.income = action.payload;
      })
      .addCase(fetchIncome.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createIncome.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createIncome.fulfilled, (state, action) => {
        state.loading = false;
        state.income.items.push(action.payload);
        state.income.totalCount += 1;
      })
      .addCase(createIncome.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateIncome.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateIncome.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.income.items.findIndex(
          (i) => i.id === action.payload.id
        );
        if (index !== -1) {
          state.income.items[index] = action.payload;
        }
      })
      .addCase(updateIncome.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteIncome.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteIncome.fulfilled, (state, action) => {
        state.loading = false;
        state.income.items = state.income.items.filter(
          (i) => i.id !== action.payload
        );
        state.income.totalCount -= 1;
      })
      .addCase(deleteIncome.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchExpense.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchExpense.fulfilled, (state, action) => {
        state.loading = false;
        state.expense = action.payload;
      })
      .addCase(fetchExpense.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createExpense.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createExpense.fulfilled, (state, action) => {
        state.loading = false;
        state.expense.items.push(action.payload);
        state.expense.totalCount += 1;
      })
      .addCase(createExpense.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateExpense.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateExpense.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.expense.items.findIndex(
          (e) => e.id === action.payload.id
        );
        if (index !== -1) {
          state.expense.items[index] = action.payload;
        }
      })
      .addCase(updateExpense.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteExpense.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteExpense.fulfilled, (state, action) => {
        state.loading = false;
        state.expense.items = state.expense.items.filter(
          (e) => e.id !== action.payload
        );
        state.expense.totalCount -= 1;
      })
      .addCase(deleteExpense.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchLedger.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchLedger.fulfilled, (state, action) => {
        state.loading = false;
        state.ledger = action.payload;
      })
      .addCase(fetchLedger.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearAccountError } = accountSlice.actions;
export default accountSlice.reducer;
