import * as yup from 'yup';

export const registerValidation = yup.object({
  displayName: yup.string().required('Nama lengkap wajib diisi'),
  email: yup.string().required('Email wajib diisi').email('Email harus valid'),
  password: yup
    .string()
    .required('Password wajib diisi')
    .min(3, 'Password minimal 3 karakter'),
});
