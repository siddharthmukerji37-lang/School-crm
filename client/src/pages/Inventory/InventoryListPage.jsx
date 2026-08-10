import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, Tab, Tabs } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchItems, deleteItem, fetchVendors, deleteVendor } from '../../store/slices/inventorySlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

export default function InventoryListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { items, vendors, loading } = useSelector((state) => state.inventory);

  const [tab, setTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleteType, setDeleteType] = useState(null);
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    if (tab === 0) {
      dispatch(fetchItems({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      dispatch(fetchVendors({ page: page + 1, pageSize: rowsPerPage }));
    }
  }, [dispatch, tab, page, rowsPerPage]);

  const getStatusColor = (quantity, reorderLevel) => {
    if (quantity <= 0) return 'error';
    if (quantity <= reorderLevel) return 'warning';
    return 'success';
  };

  const getStatusLabel = (quantity, reorderLevel) => {
    if (quantity <= 0) return 'Out of Stock';
    if (quantity <= reorderLevel) return 'Low Stock';
    return 'In Stock';
  };

  const itemColumns = [
    { id: 'name', header: 'Name', accessor: 'name', minWidth: 150 },
    { id: 'category', header: 'Category', accessor: 'category', minWidth: 120 },
    {
      id: 'quantity',
      header: 'Quantity',
      accessor: 'quantity',
      minWidth: 90,
      render: (value) => String(value ?? 0),
    },
    { id: 'unit', header: 'Unit', accessor: 'unit', minWidth: 80 },
    {
      id: 'reorderLevel',
      header: 'Reorder Level',
      accessor: 'reorderLevel',
      minWidth: 110,
      render: (value) => String(value ?? 0),
    },
    {
      id: 'status',
      header: 'Status',
      accessor: 'quantity',
      minWidth: 110,
      render: (value, row) => (
        <Chip
          label={getStatusLabel(value, row.reorderLevel)}
          color={getStatusColor(value, row.reorderLevel)}
          size="small"
          variant="outlined"
        />
      ),
    },
  ];

  const vendorColumns = [
    { id: 'name', header: 'Name', accessor: 'name', minWidth: 150 },
    { id: 'contactPerson', header: 'Contact Person', accessor: 'contactPerson', minWidth: 140 },
    { id: 'email', header: 'Email', accessor: 'email', minWidth: 180 },
    { id: 'phone', header: 'Phone', accessor: 'phone', minWidth: 120 },
    { id: 'address', header: 'Address', accessor: 'address', minWidth: 180 },
  ];

  const currentData = tab === 0 ? items : vendors;

  const handlePageChange = (_, newPage) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleTabChange = (_, newValue) => {
    setTab(newValue);
    setPage(0);
  };

  const handleEdit = (row) => {
    if (tab === 0) {
      navigate(`/inventory/${row.id}/edit`);
    } else {
      navigate(`/vendors/${row.id}/edit`);
    }
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
    setDeleteType(tab === 0 ? 'item' : 'vendor');
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleteLoading(true);
    try {
      let result;
      if (deleteType === 'item') {
        result = await dispatch(deleteItem(deleteTarget.id));
      } else {
        result = await dispatch(deleteVendor(deleteTarget.id));
      }
      const thunk = deleteType === 'item' ? deleteItem : deleteVendor;
      if (thunk.fulfilled.match(result)) {
        toast.success(
          deleteType === 'item'
            ? 'Item deleted successfully'
            : 'Vendor deleted successfully'
        );
        setDeleteTarget(null);
        setDeleteType(null);
      } else {
        toast.error(result.payload || 'Failed to delete');
      }
    } finally {
      setDeleteLoading(false);
    }
  };

  return (
    <Box>
      <PageHeader
        title="Inventory"
        subtitle={`Total ${currentData.totalCount || 0} ${tab === 0 ? 'items' : 'vendors'}`}
        actions={
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => {
              if (tab === 0) {
                navigate('/inventory/create');
              } else {
                navigate('/vendors/create');
              }
            }}
          >
            {tab === 0 ? 'Add Item' : 'Add Vendor'}
          </Button>
        }
      />

      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
        <Tabs value={tab} onChange={handleTabChange}>
          <Tab label="Items" />
          <Tab label="Vendors" />
        </Tabs>
      </Box>

      <DataTable
        key={tab}
        columns={tab === 0 ? itemColumns : vendorColumns}
        rows={currentData.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={currentData.totalCount || 0}
        searchPlaceholder={tab === 0 ? 'Search items...' : 'Search vendors...'}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onEdit={handleEdit}
        onDelete={handleDelete}
        emptyMessage={tab === 0 ? 'No items found' : 'No vendors found'}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title={deleteType === 'item' ? 'Delete Item' : 'Delete Vendor'}
        message={`Are you sure you want to delete "${deleteTarget?.name}"? This action cannot be undone.`}
        confirmText="Delete"
        loading={deleteLoading}
        onConfirm={confirmDelete}
        onCancel={() => {
          setDeleteTarget(null);
          setDeleteType(null);
        }}
      />
    </Box>
  );
}
