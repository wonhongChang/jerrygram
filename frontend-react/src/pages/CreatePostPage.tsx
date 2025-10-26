import React, { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import { postService } from '../services/postService';
import { FiImage, FiX } from 'react-icons/fi';

const CreatePostPage: React.FC = () => {
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>('');
  const [caption, setCaption] = useState('');
  const [visibility, setVisibility] = useState(0); // 0 = Public, 1 = Private
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      if (file.size > 10 * 1024 * 1024) { // 10MB limit
        setError('Image size must be less than 10MB');
        return;
      }

      if (!file.type.startsWith('image/')) {
        setError('Please select an image file');
        return;
      }

      setSelectedImage(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
      setError('');
    }
  };

  const handleRemoveImage = () => {
    setSelectedImage(null);
    setImagePreview('');
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedImage) {
      setError('Please select an image');
      return;
    }

    if (!caption.trim()) {
      setError('Please add a caption');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', selectedImage);
      formData.append('caption', caption);
      formData.append('visibility', visibility.toString());

      await postService.createPost(formData);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create post');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="max-w-2xl mx-auto">
        <div className="bg-white border border-gray-300 rounded-lg overflow-hidden">
          {/* Header */}
          <div className="border-b border-gray-300 px-4 py-3">
            <h2 className="text-xl font-semibold text-center">Create New Post</h2>
          </div>

          <form onSubmit={handleSubmit}>
            {/* Image Upload Section */}
            <div className="p-6">
              {!imagePreview ? (
                <div
                  onClick={() => fileInputRef.current?.click()}
                  className="border-2 border-dashed border-gray-300 rounded-lg p-12 text-center cursor-pointer hover:border-gray-400 transition-colors"
                >
                  <FiImage className="mx-auto mb-4 text-gray-400" size={48} />
                  <p className="text-gray-600 mb-2">Click to select an image</p>
                  <p className="text-gray-400 text-sm">or drag and drop</p>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    onChange={handleImageSelect}
                    className="hidden"
                  />
                </div>
              ) : (
                <div className="relative">
                  <img
                    src={imagePreview}
                    alt="Preview"
                    className="w-full rounded-lg max-h-96 object-contain bg-gray-100"
                  />
                  <button
                    type="button"
                    onClick={handleRemoveImage}
                    className="absolute top-2 right-2 bg-white rounded-full p-2 shadow-lg hover:bg-gray-100"
                  >
                    <FiX size={20} />
                  </button>
                </div>
              )}

              {/* Caption */}
              <div className="mt-6">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Caption
                </label>
                <textarea
                  value={caption}
                  onChange={(e) => setCaption(e.target.value)}
                  placeholder="Write a caption..."
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent resize-none"
                  maxLength={2200}
                />
                <div className="text-right text-sm text-gray-400 mt-1">
                  {caption.length}/2200
                </div>
              </div>

              {/* Visibility */}
              <div className="mt-6">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Visibility
                </label>
                <div className="flex gap-4">
                  <label className="flex items-center cursor-pointer">
                    <input
                      type="radio"
                      name="visibility"
                      value={0}
                      checked={visibility === 0}
                      onChange={() => setVisibility(0)}
                      className="mr-2"
                    />
                    <span className="text-sm">Public</span>
                  </label>
                  <label className="flex items-center cursor-pointer">
                    <input
                      type="radio"
                      name="visibility"
                      value={1}
                      checked={visibility === 1}
                      onChange={() => setVisibility(1)}
                      className="mr-2"
                    />
                    <span className="text-sm">Private (Followers only)</span>
                  </label>
                </div>
              </div>

              {/* Error Message */}
              {error && (
                <div className="mt-4 bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-md text-sm">
                  {error}
                </div>
              )}

              {/* Submit Button */}
              <div className="mt-6 flex gap-3">
                <button
                  type="button"
                  onClick={() => navigate('/')}
                  className="flex-1 py-2 px-4 border border-gray-300 rounded-lg font-semibold hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={loading || !selectedImage || !caption.trim()}
                  className="flex-1 py-2 px-4 bg-primary text-white rounded-lg font-semibold hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {loading ? 'Posting...' : 'Share'}
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  );
};

export default CreatePostPage;
