import React, { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import { postService } from '../services/postService';
import { getApiErrorMessage } from '../utils/apiData';
import { FiGlobe, FiImage, FiUsers, FiX } from 'react-icons/fi';

const CreatePostPage: React.FC = () => {
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>('');
  const [caption, setCaption] = useState('');
  const [visibility, setVisibility] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [dragging, setDragging] = useState(false);

  const setImageFile = (file: File) => {
    if (file.size > 10 * 1024 * 1024) {
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
  };

  const handleImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) setImageFile(file);
  };

  const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragging(false);
    const file = event.dataTransfer.files?.[0];
    if (file) setImageFile(file);
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
      formData.append('caption', caption.trim());
      formData.append('visibility', visibility.toString());

      await postService.createPost(formData);
      navigate('/');
    } catch (err: any) {
      setError(getApiErrorMessage(err, 'Failed to create post'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="mx-auto max-w-4xl">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-950">Create</h1>
        </div>

        <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
          <div className="border-b border-gray-200 px-5 py-4">
            <h2 className="text-lg font-semibold text-gray-950">New post</h2>
          </div>

          <form onSubmit={handleSubmit}>
            <div className="grid gap-6 p-5 lg:grid-cols-[minmax(0,1fr)_320px] lg:p-6">
              {!imagePreview ? (
                <div
                  onClick={() => fileInputRef.current?.click()}
                  onDragOver={(event) => {
                    event.preventDefault();
                    setDragging(true);
                  }}
                  onDragLeave={() => setDragging(false)}
                  onDrop={handleDrop}
                  className={`flex min-h-[360px] cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 text-center transition-colors ${
                    dragging ? 'border-primary bg-blue-50' : 'border-gray-300 hover:border-gray-400'
                  }`}
                >
                  <FiImage className="mx-auto mb-4 text-gray-400" size={48} />
                  <p className="mb-2 font-semibold text-gray-800">Select an image</p>
                  <p className="text-sm text-gray-400">PNG, JPG, GIF up to 10MB</p>
                  <input ref={fileInputRef} type="file" accept="image/*" onChange={handleImageSelect} className="hidden" />
                </div>
              ) : (
                <div className="relative rounded-lg bg-gray-100">
                  <img src={imagePreview} alt="Preview" className="h-full max-h-[560px] min-h-[360px] w-full rounded-lg object-contain" />
                  <button
                    type="button"
                    onClick={handleRemoveImage}
                    className="absolute right-3 top-3 rounded-md bg-white p-2 text-gray-700 shadow-lg hover:bg-gray-100"
                    aria-label="Remove image"
                  >
                    <FiX size={20} />
                  </button>
                </div>
              )}

              <div className="flex flex-col">
                <div>
                  <label className="mb-2 block text-sm font-semibold text-gray-800">Caption</label>
                  <textarea
                    value={caption}
                    onChange={(e) => setCaption(e.target.value)}
                    placeholder="Write a caption..."
                    rows={8}
                    className="w-full resize-none rounded-md border border-gray-300 px-3 py-3 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary"
                    maxLength={2200}
                  />
                  <div className="mt-1 text-right text-sm text-gray-400">{caption.length}/2200</div>
                </div>

                <div className="mt-5">
                  <label className="mb-2 block text-sm font-semibold text-gray-800">Visibility</label>
                  <div className="grid grid-cols-2 gap-2 rounded-lg border border-gray-200 bg-gray-50 p-1">
                    <button
                      type="button"
                      onClick={() => setVisibility(0)}
                      className={`inline-flex items-center justify-center gap-2 rounded-md px-3 py-2 text-sm font-semibold ${
                        visibility === 0 ? 'bg-white text-gray-950 shadow-sm' : 'text-gray-600 hover:text-gray-950'
                      }`}
                    >
                      <FiGlobe size={16} />
                      Public
                    </button>
                    <button
                      type="button"
                      onClick={() => setVisibility(1)}
                      className={`inline-flex items-center justify-center gap-2 rounded-md px-3 py-2 text-sm font-semibold ${
                        visibility === 1 ? 'bg-white text-gray-950 shadow-sm' : 'text-gray-600 hover:text-gray-950'
                      }`}
                    >
                      <FiUsers size={16} />
                      Followers
                    </button>
                  </div>
                </div>

                {error && <div className="mt-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">{error}</div>}

                <div className="mt-auto flex gap-3 pt-6">
                  <button
                    type="button"
                    onClick={() => navigate('/')}
                    className="flex-1 rounded-md border border-gray-300 px-4 py-2 font-semibold text-gray-800 hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={loading || !selectedImage || !caption.trim()}
                    className="flex-1 rounded-md bg-primary px-4 py-2 font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {loading ? 'Posting...' : 'Share'}
                  </button>
                </div>
              </div>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  );
};

export default CreatePostPage;
