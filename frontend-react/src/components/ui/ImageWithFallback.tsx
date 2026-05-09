import React, { useState } from 'react';
import { FiImage } from 'react-icons/fi';
import { resolveAssetUrl } from '../../utils/apiData';

interface ImageWithFallbackProps {
  src?: string | null;
  alt: string;
  className?: string;
  fallbackClassName?: string;
}

const ImageWithFallback: React.FC<ImageWithFallbackProps> = ({
  src,
  alt,
  className = '',
  fallbackClassName = '',
}) => {
  const [failed, setFailed] = useState(false);
  const imageUrl = resolveAssetUrl(src);

  if (!imageUrl || failed) {
    return (
      <div
        className={`flex h-full w-full items-center justify-center bg-gray-100 text-gray-400 ${fallbackClassName}`}
        aria-label={alt}
      >
        <FiImage size={32} />
      </div>
    );
  }

  return <img src={imageUrl} alt={alt} className={className} onError={() => setFailed(true)} />;
};

export default ImageWithFallback;
