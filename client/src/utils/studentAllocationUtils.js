export function findCurrentStudent(students = {}, user = null) {
  if (!user) return null;

  const allStudents = Array.isArray(students?.items)
    ? students.items
    : Array.isArray(students)
      ? students
      : [];

  if (!allStudents.length) return null;

  const normalizedUserEmail = String(user.email ?? '').trim().toLowerCase();
  const normalizedUserId = String(user.id ?? user.userId ?? '').trim();
  const normalizedStudentId = String(user.studentId ?? '').trim();

  return allStudents.find((student) => {
    const studentEmail = String(student.email ?? student.userEmail ?? '').trim().toLowerCase();
    const studentUserId = String(student.userId ?? student.user_id ?? '').trim();
    const currentStudentId = String(student.id ?? '').trim();

    return (
      (!!normalizedUserEmail && !!studentEmail && studentEmail === normalizedUserEmail) ||
      (!!normalizedUserId && !!studentUserId && studentUserId === normalizedUserId) ||
      (!!normalizedStudentId && !!currentStudentId && currentStudentId === normalizedStudentId)
    );
  }) || null;
}

export function filterStudentAllocations(allocations = [], currentStudent = null) {
  if (!currentStudent) return allocations;

  const currentStudentId = String(currentStudent.id ?? currentStudent.studentId ?? '').trim();
  const studentName = `${currentStudent.firstName || ''} ${currentStudent.lastName || ''}`.trim().toLowerCase();

  return allocations.filter((allocation) => {
    const allocationStudentId = String(allocation.studentId ?? allocation.student_id ?? '').trim();
    const allocationStudentName = String(allocation.studentName ?? '').trim().toLowerCase();

    return (
      (!currentStudentId || allocationStudentId === currentStudentId) ||
      (!!studentName && allocationStudentName === studentName)
    );
  });
}
