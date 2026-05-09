import React from 'react';

interface EmptyStateProps {
  title: string;
  description?: string;
  action?: React.ReactNode;
}

const EmptyState: React.FC<EmptyStateProps> = ({ title, description, action }) => (
  <div className="text-center py-12 px-4">
    <p className="text-gray-700 text-lg font-semibold">{title}</p>
    {description && <p className="text-gray-500 text-sm mt-2">{description}</p>}
    {action && <div className="mt-5">{action}</div>}
  </div>
);

export default EmptyState;
