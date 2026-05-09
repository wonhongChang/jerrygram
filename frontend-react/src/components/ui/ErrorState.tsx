import React from 'react';
import { FiRefreshCw } from 'react-icons/fi';

interface ErrorStateProps {
  message: string;
  onRetry?: () => void;
}

const ErrorState: React.FC<ErrorStateProps> = ({ message, onRetry }) => (
  <div className="rounded-md border border-red-200 bg-red-50 px-4 py-4 text-red-700">
    <div className="flex items-center justify-between gap-4">
      <p className="text-sm font-medium">{message}</p>
      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          className="inline-flex items-center gap-2 rounded-md border border-red-200 bg-white px-3 py-2 text-sm font-semibold text-red-700 hover:bg-red-100"
        >
          <FiRefreshCw size={16} />
          Retry
        </button>
      )}
    </div>
  </div>
);

export default ErrorState;
