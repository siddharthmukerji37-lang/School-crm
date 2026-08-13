import test from 'node:test';
import assert from 'node:assert/strict';
import { findCurrentStudent, filterStudentAllocations } from './studentAllocationUtils.js';

test('findCurrentStudent matches by email when a student is logged in', () => {
  const students = {
    items: [
      { id: 'student-1', firstName: 'Riya', lastName: 'Sen', email: 'riya@example.com' },
      { id: 'student-2', firstName: 'Amit', lastName: 'Roy', email: 'amit@example.com' },
    ],
  };

  const user = { email: 'riya@example.com' };

  assert.deepEqual(findCurrentStudent(students, user), students.items[0]);
});

test('filterStudentAllocations hides other students allocations', () => {
  const currentStudent = { id: 'student-1', firstName: 'Riya', lastName: 'Sen' };
  const allocations = [
    { id: 'a1', studentId: 'student-1', studentName: 'Riya Sen' },
    { id: 'a2', studentId: 'student-2', studentName: 'Amit Roy' },
  ];

  assert.deepEqual(filterStudentAllocations(allocations, currentStudent), [allocations[0]]);
});
