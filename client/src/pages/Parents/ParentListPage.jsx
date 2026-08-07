import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchParents, deleteParent } from '../../store/slices/parentSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import { hasAdminRole } from '../../utils/roles';
import toast from 'react-hot-toast';

export default function ParentListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { parents, loading } = useSelector((state) => state.parents);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = hasAdminRole(user?.roles);

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    dispatch(
      fetchParents({
        page: page + 1,
        pageSize: rowsPerPage,
      })
    );
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    {
      id: 'name',
      header: 'Name',
      accessor: (row) => `${row.firstName || ''} ${row.lastName || ''}`.trim(),
      minWidth: 180,
    },
    { id: 'email', header: 'Email', accessor: 'email', minWidth: 200 },
    { id: 'phone', header: 'Phone', accessor: 'phone', minWidth: 130 },
    { id: 'occupation', header: 'Occupation', accessor: 'occupation', minWidth: 140 },
    { id: 'relationship', header: 'Relationship', accessor: 'relationship', minWidth: 120 },
    {
      id: 'children',
      header: 'Children',
      accessor: (row) =>
        (row.children || []).map((c) => c.studentName).join(', ') || '-',
      minWidth: 200,
    },
  ];

  const handlePageChange = (_, newPage) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleEdit = (row) => {
    navigate(`/parents/${row.id}/edit`);
  };

  const handleView = (row) => {
    navigate(`/parents/${row.id}`);
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteParent(deleteTarget.id));
    if (deleteParent.fulfilled.match(result)) {
      toast.success('Parent deleted successfully');
      setDeleteTarget(null);
      dispatch(fetchParents({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete parent');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Parents"
        subtitle={`Total ${parents.totalCount || 0} parents`}
        actions={
          isAdmin ? (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => navigate('/parents/create')}
            >
              Add Parent
            </Button>
          ) : null
        }
      />

      <DataTable
        columns={columns}
        rows={parents.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={parents.totalCount || 0}
        searchPlaceholder="Search parents..."
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onEdit={isAdmin ? handleEdit : undefined}
        onDelete={isAdmin ? handleDelete : undefined}
        onView={handleView}
        emptyMessage="No parents found"
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Parent"
        message={`Are you sure you want to delete ${deleteTarget?.firstName || 'this parent'}? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
