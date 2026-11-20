import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Link } from 'react-router';
import { registerValidation } from '../utils/registerValidation';
import Card from '../components/Card';
import Button from '../components/Button';
import { API_BASE_URL } from '../config';
import { useState } from 'react';

export default function Register() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    defaultValue: {
      email: '',
      password: '',
      displayName: '',
    },
    resolver: yupResolver(registerValidation),
  });

  const [resSuccessMsg, setResSuccessMsg] = useState('');
  const [resErrorMsg, setResErrorMsg] = useState('');

  const onSubmit = async ({ email, password }) => {
    try {
      const res = await fetch(`${API_BASE_URL}/api/v1/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });
      const result = await res.json();

      console.log(result);

      if (result?.user) {
        setResSuccessMsg(
          result?.message ||
            result?.detail ||
            JSON.stringfy(result) ||
            'Register berhasil!',
        );
      } else {
        setResErrorMsg(
          result?.message ||
            result?.detail ||
            JSON.stringfy(result) ||
            'Register gagal!',
        );
      }
    } catch (error) {
      console.log(error);
      setResErrorMsg(
        error?.message ||
          error?.detail ||
          JSON.stringify(error) ||
          'Gagal. Ada kesalahan!',
      );
    }
  };

  return (
    <div className="flex justify-center">
      <Card className="sm:w-140">
        <h1 className="text-3xl font-bold text-center mb-4">Register</h1>
        {resSuccessMsg && (
          <div className="border p-3 w-full my-4 bg-green-500 text-white rounded-2xl">
            {resSuccessMsg}
          </div>
        )}
        {resErrorMsg && (
          <div className="border p-3 w-full my-4 bg-red-500 text-white rounded-2xl">
            {resErrorMsg}
          </div>
        )}
        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="grid sm:grid-cols-2 gap-5">
            <div className="flex flex-col mb-3">
              <label htmlFor="fullname" className="mb-3">
                Fullname
              </label>
              <input
                id="fullname"
                type="text"
                className="form-input"
                placeholder="Masukkan Nama Lengkap"
                {...register('fullname')}
              />
              {errors.fullname?.message && (
                <p role="alert" className="mt-1 text-sm text-red-600">
                  {errors.fullname.message}
                </p>
              )}
            </div>
            <div className="flex flex-col mb-3">
              <label htmlFor="email" className="mb-3">
                Email
              </label>
              <input
                id="email"
                type="email"
                className="form-input"
                placeholder="Masukkan Email"
                {...register('email')}
              />
              {errors.email?.message && (
                <p role="alert" className="mt-1 text-sm text-red-600">
                  {errors.email.message}
                </p>
              )}
            </div>
            <div className="flex flex-col mb-3">
              <label htmlFor="username" className="mb-3">
                Username
              </label>
              <input
                id="username"
                type="text"
                className="form-input"
                placeholder="Masukkan Username"
                {...register('username')}
              />
              {errors.username?.message && (
                <p role="alert" className="mt-1 text-sm text-red-600">
                  {errors.username.message}
                </p>
              )}
            </div>
            <div className="flex flex-col mb-3">
              <label htmlFor="password" className="mb-3">
                Password
              </label>
              <input
                id="password"
                type="password"
                className="form-input"
                placeholder="Masukkan Password"
                {...register('password')}
              />
              {errors.password?.message && (
                <p role="alert" className="mt-1 text-sm text-red-600">
                  {errors.password.message}
                </p>
              )}
            </div>
          </div>
          <p className="text-sm text-gray-700 mt-4 text-center">
            Sudah punya akun?{' '}
            <Link to="/" className="underline hover:text-blue-600">
              Masuk disini!
            </Link>
          </p>
          <div className="flex justify-center">
            <Button type="submit">Daftar</Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
