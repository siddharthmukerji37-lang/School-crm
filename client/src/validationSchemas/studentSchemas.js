import * as Yup from 'yup';

export const studentSchema = Yup.object({
  firstName: Yup.string()
    .trim()
    .required('First name is required')
    .min(2, 'First name must be at least 2 characters'),
  lastName: Yup.string()
    .trim()
    .required('Last name is required')
    .min(2, 'Last name must be at least 2 characters'),
  email: Yup.string()
    .email('Please enter a valid email address')
    .required('Email is required'),
  password: Yup.string()
    .min(6, 'Password must be at least 6 characters')
    .required('Password is required'),
  dateOfBirth: Yup.date()
    .nullable()
    .required('Date of birth is required')
    .max(new Date(), 'Date of birth cannot be in the future'),
  gender: Yup.string()
    .oneOf(['Male', 'Female', 'Other'], 'Please select a valid gender')
    .required('Gender is required'),
  classRoomId: Yup.string()
    .required('Class is required'),
  sectionId: Yup.string()
    .required('Section is required'),
  admissionDate: Yup.date()
    .nullable()
    .required('Admission date is required'),
  phone: Yup.string()
    .matches(/^[0-9+\-\s()]*$/, 'Please enter a valid phone number'),
  address: Yup.string()
    .trim(),
  parentName: Yup.string()
    .trim(),
  parentPhone: Yup.string()
    .matches(/^[0-9+\-\s()]*$/, 'Please enter a valid phone number'),
  parentEmail: Yup.string()
    .email('Please enter a valid email address'),
  bloodGroup: Yup.string()
    .oneOf(
      ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-', ''],
      'Please select a valid blood group'
    ),
  transportRequired: Yup.boolean(),
  hostelRequired: Yup.boolean(),
  notes: Yup.string()
    .trim(),
});

export const studentEditSchema = studentSchema.omit(['password']);

export const studentFilterSchema = Yup.object({
  search: Yup.string().trim(),
  classId: Yup.string(),
  sectionId: Yup.string(),
});
