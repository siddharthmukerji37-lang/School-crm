import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import transportService from '../../services/transportService';

export const fetchRoutes = createAsyncThunk(
  'transport/fetchRoutes',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await transportService.getRoutes(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch routes'
      );
    }
  }
);

export const createRoute = createAsyncThunk(
  'transport/createRoute',
  async (routeData, { rejectWithValue }) => {
    try {
      const response = await transportService.createRoute(routeData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create route'
      );
    }
  }
);

export const updateRoute = createAsyncThunk(
  'transport/updateRoute',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await transportService.updateRoute(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update route'
      );
    }
  }
);

export const deleteRoute = createAsyncThunk(
  'transport/deleteRoute',
  async (id, { rejectWithValue }) => {
    try {
      await transportService.deleteRoute(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete route'
      );
    }
  }
);

export const fetchVehicles = createAsyncThunk(
  'transport/fetchVehicles',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await transportService.getVehicles(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch vehicles'
      );
    }
  }
);

export const createVehicle = createAsyncThunk(
  'transport/createVehicle',
  async (vehicleData, { rejectWithValue }) => {
    try {
      const response = await transportService.createVehicle(vehicleData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create vehicle'
      );
    }
  }
);

export const updateVehicle = createAsyncThunk(
  'transport/updateVehicle',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await transportService.updateVehicle(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update vehicle'
      );
    }
  }
);

export const deleteVehicle = createAsyncThunk(
  'transport/deleteVehicle',
  async (id, { rejectWithValue }) => {
    try {
      await transportService.deleteVehicle(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete vehicle'
      );
    }
  }
);

export const fetchAllocations = createAsyncThunk(
  'transport/fetchAllocations',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await transportService.getAllocations(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch allocations'
      );
    }
  }
);

export const allocateTransport = createAsyncThunk(
  'transport/allocateTransport',
  async (allocationData, { rejectWithValue }) => {
    try {
      const response = await transportService.allocate(allocationData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to allocate transport'
      );
    }
  }
);

export const deallocateTransport = createAsyncThunk(
  'transport/deallocateTransport',
  async (allocationId, { rejectWithValue }) => {
    try {
      const response = await transportService.deallocate(allocationId);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to deallocate transport'
      );
    }
  }
);

const transportSlice = createSlice({
  name: 'transport',
  initialState: {
    routes: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    vehicles: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    allocations: {
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
    clearTransportError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchRoutes.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchRoutes.fulfilled, (state, action) => {
        state.loading = false;
        state.routes = action.payload;
      })
      .addCase(fetchRoutes.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createRoute.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createRoute.fulfilled, (state, action) => {
        state.loading = false;
        state.routes.items.push(action.payload);
        state.routes.totalCount += 1;
      })
      .addCase(createRoute.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateRoute.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateRoute.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.routes.items.findIndex(
          (r) => r.id === action.payload.id
        );
        if (index !== -1) {
          state.routes.items[index] = action.payload;
        }
      })
      .addCase(updateRoute.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteRoute.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteRoute.fulfilled, (state, action) => {
        state.loading = false;
        state.routes.items = state.routes.items.filter(
          (r) => r.id !== action.payload
        );
        state.routes.totalCount -= 1;
      })
      .addCase(deleteRoute.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchVehicles.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchVehicles.fulfilled, (state, action) => {
        state.loading = false;
        state.vehicles = action.payload;
      })
      .addCase(fetchVehicles.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createVehicle.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createVehicle.fulfilled, (state, action) => {
        state.loading = false;
        state.vehicles.items.push(action.payload);
        state.vehicles.totalCount += 1;
      })
      .addCase(createVehicle.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateVehicle.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateVehicle.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.vehicles.items.findIndex(
          (v) => v.id === action.payload.id
        );
        if (index !== -1) {
          state.vehicles.items[index] = action.payload;
        }
      })
      .addCase(updateVehicle.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteVehicle.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteVehicle.fulfilled, (state, action) => {
        state.loading = false;
        state.vehicles.items = state.vehicles.items.filter(
          (v) => v.id !== action.payload
        );
        state.vehicles.totalCount -= 1;
      })
      .addCase(deleteVehicle.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchAllocations.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAllocations.fulfilled, (state, action) => {
        state.loading = false;
        state.allocations = action.payload;
      })
      .addCase(fetchAllocations.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(allocateTransport.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(allocateTransport.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(allocateTransport.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deallocateTransport.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deallocateTransport.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(deallocateTransport.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearTransportError } = transportSlice.actions;
export default transportSlice.reducer;
