import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import libraryService from '../../services/libraryService';

export const fetchBooks = createAsyncThunk(
  'library/fetchBooks',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await libraryService.getAllBooks(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch books'
      );
    }
  }
);

export const fetchBookById = createAsyncThunk(
  'library/fetchBookById',
  async (id, { rejectWithValue }) => {
    try {
      const response = await libraryService.getBookById(id);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch book'
      );
    }
  }
);

export const createBook = createAsyncThunk(
  'library/createBook',
  async (bookData, { rejectWithValue }) => {
    try {
      const response = await libraryService.createBook(bookData);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to create book'
      );
    }
  }
);

export const updateBook = createAsyncThunk(
  'library/updateBook',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const response = await libraryService.updateBook(id, data);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to update book'
      );
    }
  }
);

export const deleteBook = createAsyncThunk(
  'library/deleteBook',
  async (id, { rejectWithValue }) => {
    try {
      await libraryService.deleteBook(id);
      return id;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to delete book'
      );
    }
  }
);

export const fetchIssuedBooks = createAsyncThunk(
  'library/fetchIssuedBooks',
  async (params = {}, { rejectWithValue }) => {
    try {
      const response = await libraryService.getIssuedBooks(params);
      return response.data.data;
    } catch (error) {
      return rejectWithValue(
        error.response?.data?.message || 'Failed to fetch issued books'
      );
    }
  }
);

const librarySlice = createSlice({
  name: 'library',
  initialState: {
    books: {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 10,
      totalPages: 0,
    },
    selectedBook: null,
    issuedBooks: [],
    loading: false,
    error: null,
  },
  reducers: {
    clearSelectedBook: (state) => {
      state.selectedBook = null;
    },
    clearLibraryError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchBooks.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchBooks.fulfilled, (state, action) => {
        state.loading = false;
        state.books = action.payload;
      })
      .addCase(fetchBooks.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchBookById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchBookById.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedBook = action.payload;
      })
      .addCase(fetchBookById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(createBook.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createBook.fulfilled, (state, action) => {
        state.loading = false;
        state.books.items.push(action.payload);
        state.books.totalCount += 1;
      })
      .addCase(createBook.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(updateBook.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateBook.fulfilled, (state, action) => {
        state.loading = false;
        const index = state.books.items.findIndex(
          (b) => b.id === action.payload.id
        );
        if (index !== -1) {
          state.books.items[index] = action.payload;
        }
        if (state.selectedBook?.id === action.payload.id) {
          state.selectedBook = action.payload;
        }
      })
      .addCase(updateBook.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(deleteBook.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteBook.fulfilled, (state, action) => {
        state.loading = false;
        state.books.items = state.books.items.filter(
          (b) => b.id !== action.payload
        );
        state.books.totalCount -= 1;
      })
      .addCase(deleteBook.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchIssuedBooks.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchIssuedBooks.fulfilled, (state, action) => {
        state.loading = false;
        state.issuedBooks = action.payload;
      })
      .addCase(fetchIssuedBooks.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearSelectedBook, clearLibraryError } = librarySlice.actions;
export default librarySlice.reducer;
