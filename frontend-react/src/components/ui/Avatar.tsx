import React from 'react';
import { FiUser } from 'react-icons/fi';
import { resolveAssetUrl } from '../../utils/apiData';

interface AvatarProps {
  src?: string | null;
  username?: string;
  size?: 'sm' | 'md' | 'lg' | 'xl';
  className?: string;
}

const sizeClass = {
  sm: 'w-8 h-8 text-xs',
  md: 'w-10 h-10 text-sm',
  lg: 'w-12 h-12 text-base',
  xl: 'w-32 h-32 md:w-40 md:h-40 text-5xl',
};

const Avatar: React.FC<AvatarProps> = ({ src, username = 'user', size = 'md', className = '' }) => {
  const imageUrl = resolveAssetUrl(src);
  const initial = username.trim().charAt(0).toUpperCase();

  if (imageUrl) {
    return (
      <img
        src={imageUrl}
        alt={username}
        className={`${sizeClass[size]} rounded-full object-cover border border-gray-200 bg-gray-100 ${className}`}
      />
    );
  }

  return (
    <div
      className={`${sizeClass[size]} rounded-full bg-gray-200 text-gray-600 flex items-center justify-center border border-gray-200 ${className}`}
      aria-label={username}
    >
      {initial || <FiUser size={18} />}
    </div>
  );
};

export default Avatar;
