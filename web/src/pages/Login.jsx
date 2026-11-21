import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { loginValidation } from '../utils/loginValidation';
import { Link } from 'react-router';
import Card from '../components/Card';
import Button from '../components/Button';
import { useState } from 'react';
import { API_BASE_URL } from '../config';

export default function Login() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm({
    defaultValue: {
      email: '',
      password: '',
    },
    resolver: yupResolver(loginValidation),
  });

  const [resSuccessMsg, setResSuccessMsg] = useState('');
  const [resErrorMsg, setResErrorMsg] = useState('');

  const onSubmit = async ({ email, password }) => {
    try {
      const res = await fetch(`${API_BASE_URL}/api/v1/auth/login`, {
        method: 'POST',
        credentials: 'include',
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
            'Login berhasil!',
        );
      } else {
        setResErrorMsg(
          result?.message ||
            result?.detail ||
            JSON.stringfy(result) ||
            'Login gagal!',
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
      <Card className="w-75 position-relative">
        <h1 className="text-3xl font-bold text-center mb-4">Login</h1>
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
          <div className="flex flex-col mb-5">
            <label htmlFor="email" className="mb-3">
              Email
            </label>
            <input
              id="email"
              type="text"
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
          <div className="flex flex-col mb-5">
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
          <p className="text-sm text-gray-700 mt-5">
            Belum punya akun?{' '}
            <Link to="/register" className="underline hover:text-blue-600">
              Daftar disini!
            </Link>
          </p>
          <div className="flex justify-center">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Loading...' : 'Masuk'}
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
