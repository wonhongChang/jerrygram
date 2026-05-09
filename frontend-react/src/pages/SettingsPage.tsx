import React, { useRef, useState } from 'react';
import Layout from '../components/layout/Layout';
import Avatar from '../components/ui/Avatar';
import { useAuth } from '../contexts/AuthContext';
import { userService } from '../services/userService';
import { getApiErrorMessage } from '../utils/apiData';

const SettingsPage: React.FC = () => {
  const { user, refreshUser } = useAuth();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [previewUrl, setPreviewUrl] = useState('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      setError('Please select an image file.');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      setError('Image size must be less than 5MB.');
      return;
    }

    setSelectedFile(file);
    setPreviewUrl(URL.createObjectURL(file));
    setError('');
    setMessage('');
  };

  const handleSave = async () => {
    if (!selectedFile) return;

    setSaving(true);
    setError('');
    setMessage('');
    try {
      await userService.uploadAvatar(selectedFile);
      await refreshUser();
      setSelectedFile(null);
      setPreviewUrl('');
      setMessage('Profile photo updated.');
      if (fileInputRef.current) fileInputRef.current.value = '';
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to update profile photo.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Layout>
      <div className="mx-auto max-w-2xl">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-950">Settings</h1>
        </div>

        <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
          <div className="border-b border-gray-200 px-6 py-4">
            <h2 className="text-sm font-semibold text-gray-950">Profile</h2>
          </div>

          <div className="p-6 space-y-6">
            <div className="flex items-center gap-5">
              <Avatar src={previewUrl || user?.profileImageUrl} username={user?.username} size="lg" />
              <div className="min-w-0">
                <p className="font-semibold text-gray-900">{user?.username}</p>
                <p className="text-sm text-gray-500 truncate">{user?.email}</p>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2" htmlFor="avatar">
                Profile photo
              </label>
              <input
                ref={fileInputRef}
                id="avatar"
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                className="block w-full text-sm text-gray-700 file:mr-4 file:rounded-md file:border-0 file:bg-gray-100 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-gray-700 hover:file:bg-gray-200"
              />
            </div>

            {error && <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>}
            {message && <p className="rounded-md bg-green-50 px-4 py-3 text-sm text-green-700">{message}</p>}

            <div className="flex justify-end">
              <button
                type="button"
                onClick={handleSave}
                disabled={!selectedFile || saving}
                className="rounded-md bg-primary px-5 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {saving ? 'Saving...' : 'Save changes'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default SettingsPage;
