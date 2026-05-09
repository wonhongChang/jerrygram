import React from 'react';
import { render, screen } from '@testing-library/react';
import App from './App';

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

test('renders the login screen for anonymous users', async () => {
  window.localStorage.clear();

  render(<App />);

  expect(await screen.findByRole('heading', { name: /sign in/i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
});
