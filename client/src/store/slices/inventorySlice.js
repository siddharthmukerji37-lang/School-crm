import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import inventoryService from '../../services/inventoryService';

export const fetchItems = createAsyncThunk(
  'inventory/fetchItems',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await inventoryService.getAllItems(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch items'
      );
    }
  }
);

export const createItem = createAsyncThunk(
  'inventory/createItem',
  async (itemData, { rejectWithValue }) => {
    try {
      const response = await inventoryService.createItem(itemData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create item'
      );
    }
  }
);

export const updateItem = createAsyncThunk(
  'inventory/updateItem',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await inventoryService.updateItem(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update item'
      );
    }
  }
);

export const deleteItem = createAsyncThunk(
  'inventory/deleteItem',
  async (id, { rejectWithValue }) => {
    try {
      await inventoryService.deleteItem(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete item'
      );
    }
  }
);

export const fetchVendors = createAsyncThunk(
  'inventory/fetchVendors',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await inventoryService.getVendors(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch vendors'
      );
    }
  }
);

export const createVendor = createAsyncThunk(
  'inventory/createVendor',
  async (vendorData, { rejectWithValue }) => {
    try {
      const response = await inventoryService.createVendor(vendorData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create vendor'
      );
    }
  }
);

export const updateVendor = createAsyncThunk(
  'inventory/updateVendor',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await inventoryService.updateVendor(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update vendor'
      );
    }
  }
);

export const deleteVendor = createAsyncThunk(
  'inventory/deleteVendor',
  async (id, { rejectWithValue }) => {
    try {
      await inventoryService.deleteVendor(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete vendor'
      );
    }
  }
);

const inventorySlice = createSlice({
  name: 'inventory',
  initialState: {
    items: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    vendors: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    loading: false,
    error: null,
  },
  reducers: {
    clearInventoryError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchItems.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchItems.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchItems.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createItem.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createItem.fulfilled, (state, action) => {
        state.loading = false;
        state.items.items.push(action.payload);
        state.items.totalCount += 1;
      })
      .addCase(createItem.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateItem.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateItem.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.items.items.findIndex(
          (i) => i.id === action.payload.id
        );
        if (index !== -1) {
          state.items.items[index] = action.payload;
        }
      })
      .addCase(updateItem.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteItem.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteItem.fulfilled, (state, action) => {
        state.loading = false;
        state.items.items = state.items.items.filter(
          (i) => i.id !== action.payload
        );
        state.items.totalCount -= 1;
      })
      .addCase(deleteItem.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchVendors.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchVendors.fulfilled, (state, action) => {
        state.loading = false;
        state.vendors = action.payload;
      })
      .addCase(fetchVendors.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createVendor.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createVendor.fulfilled, (state, action) => {
        state.loading = false;
        state.vendors.items.push(action.payload);
        state.vendors.totalCount += 1;
      })
      .addCase(createVendor.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateVendor.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateVendor.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.vendors.items.findIndex(
          (v) => v.id === action.payload.id
        );
        if (index !== -1) {
          state.vendors.items[index] = action.payload;
        }
      })
      .addCase(updateVendor.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteVendor.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteVendor.fulfilled, (state, action) => {
        state.loading = false;
        state.vendors.items = state.vendors.items.filter(
          (v) => v.id !== action.payload
        );
        state.vendors.totalCount -= 1;
      })
      .addCase(deleteVendor.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearInventoryError } = inventorySlice.actions;
export default inventorySlice.reducer;
