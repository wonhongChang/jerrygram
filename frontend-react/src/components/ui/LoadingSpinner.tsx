import React from 'react';

interface LoadingSpinnerProps {
  label?: string;
  className?: string;
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({ label = 'Loading', className = '' }) => (
  <div className={`flex flex-col items-center justify-center gap-3 text-gray-500 ${className}`}>
    <div className="h-10 w-10 animate-spin rounded-full border-2 border-gray-200 border-t-gray-950" />
    <span className="text-sm font-medium">{label}</span>
  </div>
);

export default LoadingSpinner;
