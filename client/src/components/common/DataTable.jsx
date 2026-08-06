import React, { useState, useMemo } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  TablePagination,
  Paper,
  TextField,
  InputAdornment,
  Box,
  Typography,
  CircularProgress,
  IconButton,
  Tooltip,
  Stack,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import InboxIcon from '@mui/icons-material/Inbox';

function resolveAccessor(row, accessor) {
  if (typeof accessor === 'function') return accessor(row);
  return row[accessor];
}

function descendingComparator(a, b, orderBy) {
  if (resolveAccessor(b, orderBy) < resolveAccessor(a, orderBy)) return -1;
  if (resolveAccessor(b, orderBy) > resolveAccessor(a, orderBy)) return 1;
  return 0;
}

function getComparator(order, orderBy) {
  return order === 'desc'
    ? (a, b) => descendingComparator(a, b, orderBy)
    : (a, b) => -descendingComparator(a, b, orderBy);
}

function stableSort(array, comparator) {
  const stabilized = array.map((el, index) => [el, index]);
  stabilized.sort((a, b) => {
    const order = comparator(a[0], b[0]);
    if (order !== 0) return order;
    return a[1] - b[1];
  });
  return stabilized.map((el) => el[0]);
}

export default function DataTable({
  columns,
  rows = [],
  loading = false,
  page = 0,
  rowsPerPage = 10,
  totalCount = 0,
  searchPlaceholder = 'Search...',
  enableSearch = true,
  onPageChange,
  onRowsPerPageChange,
  onView,
  onEdit,
  onDelete,
  emptyMessage = 'No records found',
  showActions = true,
  defaultSortBy = '',
  defaultOrder = 'asc',
  onRowClick,
}) {
  const [order, setOrder] = useState(defaultOrder);
  const [orderBy, setOrderBy] = useState(defaultSortBy);
  const [searchTerm, setSearchTerm] = useState('');

  const handleSort = (property) => {
    const isAsc = orderBy === property && order === 'asc';
    setOrder(isAsc ? 'desc' : 'asc');
    setOrderBy(property);
  };

  const handleSearchChange = (event) => {
    setSearchTerm(event.target.value);
  };

  const filteredRows = useMemo(() => {
    if (!searchTerm || !enableSearch) return rows;
    const lower = searchTerm.toLowerCase();
    return rows.filter((row) =>
      columns.some((col) => {
        const value = resolveAccessor(row, col.accessor);
        return String(value ?? '').toLowerCase().includes(lower);
      })
    );
  }, [rows, searchTerm, columns, enableSearch]);

  const sortedRows = useMemo(
    () => (orderBy ? stableSort(filteredRows, getComparator(order, orderBy)) : filteredRows),
    [filteredRows, order, orderBy]
  );

  const hasActions = showActions && (onView || onEdit || onDelete);

  if (loading) {
    return (
      <Paper sx={{ p: 4, display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 200 }}>
        <CircularProgress />
      </Paper>
    );
  }

  return (
    <Paper>
      {enableSearch && (
        <Box sx={{ p: 2, pb: 0 }}>
          <TextField
            size="small"
            placeholder={searchPlaceholder}
            value={searchTerm}
            onChange={handleSearchChange}
            sx={{ minWidth: 280 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon color="action" />
                </InputAdornment>
              ),
            }}
          />
        </Box>
      )}

      <TableContainer>
        <Table size="medium">
          <TableHead>
            <TableRow>
              {columns.map((col) => (
                <TableCell
                  key={col.id || col.accessor}
                  align={col.align || 'left'}
                  style={{ minWidth: col.minWidth }}
                  sortDirection={orderBy === col.accessor ? order : false}
                >
                  {col.accessor && col.sortable !== false ? (
                    <TableSortLabel
                      active={orderBy === col.accessor}
                      direction={orderBy === col.accessor ? order : 'asc'}
                      onClick={() => handleSort(col.accessor)}
                    >
                      {col.header}
                    </TableSortLabel>
                  ) : (
                    col.header
                  )}
                </TableCell>
              ))}
              {hasActions && (
                <TableCell align="right" sx={{ minWidth: 120 }}>
                  Actions
                </TableCell>
              )}
            </TableRow>
          </TableHead>
          <TableBody>
            {sortedRows.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={columns.length + (hasActions ? 1 : 0)}
                  align="center"
                >
                  <Box sx={{ py: 6, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                    <InboxIcon sx={{ fontSize: 48, color: 'text.disabled', mb: 1 }} />
                    <Typography variant="body1" color="text.secondary">
                      {emptyMessage}
                    </Typography>
                  </Box>
                </TableCell>
              </TableRow>
            ) : (
              sortedRows.map((row, rowIndex) => (
                <TableRow
                  key={row.id || rowIndex}
                  hover
                  sx={{
                    cursor: onRowClick ? 'pointer' : 'default',
                    '&:last-child td, &:last-child th': { border: 0 },
                  }}
                  onClick={onRowClick ? () => onRowClick(row) : undefined}
                >
                  {columns.map((col) => (
                    <TableCell key={col.id || col.accessor} align={col.align || 'left'}>
                      {col.render
                        ? col.render(resolveAccessor(row, col.accessor), row)
                        : resolveAccessor(row, col.accessor)}
                    </TableCell>
                  ))}
                  {hasActions && (
                    <TableCell align="right">
                      <Stack direction="row" spacing={0} justifyContent="flex-end">
                        {onView && (
                          <Tooltip title="View">
                            <IconButton
                              size="small"
                              color="info"
                              onClick={(e) => {
                                e.stopPropagation();
                                onView(row);
                              }}
                            >
                              <VisibilityIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                        {onEdit && (
                          <Tooltip title="Edit">
                            <IconButton
                              size="small"
                              color="primary"
                              onClick={(e) => {
                                e.stopPropagation();
                                onEdit(row);
                              }}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                        {onDelete && (
                          <Tooltip title="Delete">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={(e) => {
                                e.stopPropagation();
                                onDelete(row);
                              }}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                      </Stack>
                    </TableCell>
                  )}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        component="div"
        count={totalCount || filteredRows.length}
        page={page}
        onPageChange={onPageChange || (() => {})}
        rowsPerPage={rowsPerPage}
        onRowsPerPageChange={onRowsPerPageChange || (() => {})}
        rowsPerPageOptions={[5, 10, 25, 50]}
      />
    </Paper>
  );
}
