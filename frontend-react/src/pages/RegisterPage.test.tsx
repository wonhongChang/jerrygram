import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import RegisterPage from './RegisterPage';

const mockRegister = jest.fn();

jest.mock('axios', () => ({
  __esModule: true,
  default: {
    create: () => ({
      interceptors: {
        request: { use: jest.fn() },
        response: { use: jest.fn() },
      },
      get: jest.fn(),
      post: jest.fn(),
      put: jest.fn(),
      delete: jest.fn(),
    }),
  },
}));

jest.mock('../contexts/AuthContext', () => ({
  useAuth: () => ({
    register: mockRegister,
  }),
}));

const renderRegisterPage = () =>
  render(
    <MemoryRouter>
      <RegisterPage />
    </MemoryRouter>
  );

describe('RegisterPage', () => {
  beforeEach(() => {
    mockRegister.mockReset();
  });

  it('shows field-level messages before submitting invalid data', async () => {
    renderRegisterPage();

    await userEvent.click(screen.getByRole('button', { name: /sign up/i }));

    expect(screen.getByText('Email is required.')).toBeInTheDocument();
    expect(screen.getByText('Username is required.')).toBeInTheDocument();
    expect(screen.getByText('Password is required.')).toBeInTheDocument();
    expect(mockRegister).not.toHaveBeenCalled();
  });

  it('maps duplicate username responses to the username field', async () => {
    mockRegister.mockRejectedValueOnce({
      response: {
        data: {
          error: 'This username is already taken.',
        },
      },
    });

    renderRegisterPage();

    await userEvent.type(screen.getByLabelText(/email/i), 'jerry@example.com');
    await userEvent.type(screen.getByLabelText(/username/i), 'jerry');
    await userEvent.type(screen.getByLabelText(/password/i), 'Strong1!');
    await userEvent.click(screen.getByRole('button', { name: /sign up/i }));

    expect(await screen.findByText('This username is already taken.')).toBeInTheDocument();
  });
});
