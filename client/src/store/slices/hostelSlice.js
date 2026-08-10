import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import hostelService from '../../services/hostelService';

export const fetchHostels = createAsyncThunk(
  'hostel/fetchHostels',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await hostelService.getAll(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch hostels'
      );
    }
  }
);

export const createHostel = createAsyncThunk(
  'hostel/createHostel',
  async (hostelData, { rejectWithValue }) => {
    try {
      const response = await hostelService.create(hostelData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create hostel'
      );
    }
  }
);

export const updateHostel = createAsyncThunk(
  'hostel/updateHostel',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await hostelService.update(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update hostel'
      );
    }
  }
);

export const deleteHostel = createAsyncThunk(
  'hostel/deleteHostel',
  async (id, { rejectWithValue }) => {
    try {
      await hostelService.delete(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete hostel'
      );
    }
  }
);

export const fetchRooms = createAsyncThunk(
  'hostel/fetchRooms',
  async (hostelId, { rejectWithValue }) => {
    try {
      const response = await hostelService.getRooms(hostelId);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch rooms'
      );
    }
  }
);

export const fetchAllRooms = createAsyncThunk(
  'hostel/fetchAllRooms',
  async (_, { rejectWithValue }) => {
    try {
      const response = await hostelService.getAllRooms();
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch rooms'
      );
    }
  }
);

export const createRoom = createAsyncThunk(
  'hostel/createRoom',
  async (roomData, { rejectWithValue }) => {
    try {
      const response = await hostelService.createRoom(roomData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create room'
      );
    }
  }
);

export const updateRoom = createAsyncThunk(
  'hostel/updateRoom',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await hostelService.updateRoom(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update room'
      );
    }
  }
);

export const deleteRoom = createAsyncThunk(
  'hostel/deleteRoom',
  async (id, { rejectWithValue }) => {
    try {
      await hostelService.deleteRoom(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete room'
      );
    }
  }
);

export const fetchAllocations = createAsyncThunk(
  'hostel/fetchAllocations',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await hostelService.getAllocations(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch allocations'
      );
    }
  }
);

export const allocateBed = createAsyncThunk(
  'hostel/allocateBed',
  async (data, { rejectWithValue }) => {
    try {
      const response = await hostelService.allocate(data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to allocate bed'
      );
    }
  }
);

export const checkout = createAsyncThunk(
  'hostel/checkout',
  async (allocationId, { rejectWithValue }) => {
    try {
      const response = await hostelService.checkout(allocationId);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to checkout'
      );
    }
  }
);

const hostelSlice = createSlice({
  name: 'hostel',
  initialState: {
    hostels: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    rooms: [],
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
    clearHostelError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchHostels.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchHostels.fulfilled, (state, action) => {
        state.loading = false;
        state.hostels = action.payload;
      })
      .addCase(fetchHostels.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createHostel.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createHostel.fulfilled, (state, action) => {
        state.loading = false;
        state.hostels.items.push(action.payload);
        state.hostels.totalCount += 1;
      })
      .addCase(createHostel.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateHostel.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateHostel.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.hostels.items.findIndex(
          (h) => h.id === action.payload.id
        );
        if (index !== -1) {
          state.hostels.items[index] = action.payload;
        }
      })
      .addCase(updateHostel.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteHostel.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteHostel.fulfilled, (state, action) => {
        state.loading = false;
        state.hostels.items = state.hostels.items.filter(
          (h) => h.id !== action.payload
        );
        state.hostels.totalCount -= 1;
      })
      .addCase(deleteHostel.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchRooms.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchRooms.fulfilled, (state, action) => {
        state.loading = false;
        state.rooms = action.payload;
      })
      .addCase(fetchRooms.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchAllRooms.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchAllRooms.fulfilled, (state, action) => {
        state.loading = false;
        state.rooms = action.payload;
      })
      .addCase(fetchAllRooms.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createRoom.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createRoom.fulfilled, (state, action) => {
        state.loading = false;
        if (Array.isArray(state.rooms)) {
          state.rooms.push(action.payload);
        }
      })
      .addCase(createRoom.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateRoom.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateRoom.fulfilled, (state, action) => {
        state.loading = false;
        if (Array.isArray(state.rooms)) {
          const index = state.rooms.findIndex((r) => r.id === action.payload.id);
          if (index !== -1) {
            state.rooms[index] = action.payload;
          }
        }
      })
      .addCase(updateRoom.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteRoom.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteRoom.fulfilled, (state, action) => {
        state.loading = false;
        if (Array.isArray(state.rooms)) {
          state.rooms = state.rooms.filter((r) => r.id !== action.payload);
        }
      })
      .addCase(deleteRoom.rejected, (state, action) => {
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
      .addCase(allocateBed.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(allocateBed.fulfilled, (state) => {
        state.loading = false;
        state.error = null;
      })
      .addCase(allocateBed.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(checkout.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(checkout.fulfilled, (state) => {
        state.loading = false;
        state.error = null;
      })
      .addCase(checkout.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearHostelError } = hostelSlice.actions;
export default hostelSlice.reducer;
