import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { FiHome, FiSearch, FiPlusSquare, FiHeart, FiUser, FiLogOut, FiMenu } from 'react-icons/fi';

const Header: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [showProfileMenu, setShowProfileMenu] = useState(false);
  const [showMobileMenu, setShowMobileMenu] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="bg-white border-b border-gray-300 sticky top-0 z-50">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link to="/" className="text-2xl font-bold">
            Jerrygram
          </Link>

          {/* Search Bar - Hidden on mobile */}
          <div className="hidden md:block flex-1 max-w-xs mx-8">
            <div className="relative">
              <input
                type="text"
                placeholder="Search..."
                className="w-full bg-gray-100 border border-gray-300 rounded-lg px-4 py-2 pl-10 focus:outline-none focus:border-gray-400"
                onClick={() => navigate('/search')}
                readOnly
              />
              <FiSearch className="absolute left-3 top-3 text-gray-400" />
            </div>
          </div>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center space-x-6">
            <Link to="/" className="text-gray-700 hover:text-black">
              <FiHome size={24} />
            </Link>
            <Link to="/search" className="text-gray-700 hover:text-black md:hidden">
              <FiSearch size={24} />
            </Link>
            <Link to="/create" className="text-gray-700 hover:text-black">
              <FiPlusSquare size={24} />
            </Link>
            <Link to="/explore" className="text-gray-700 hover:text-black">
              <FiSearch size={24} />
            </Link>
            <Link to="/notifications" className="text-gray-700 hover:text-black">
              <FiHeart size={24} />
            </Link>

            {/* Profile Menu */}
            <div className="relative">
              <button
                onClick={() => setShowProfileMenu(!showProfileMenu)}
                className="flex items-center space-x-2 focus:outline-none"
              >
                {user?.profileImageUrl ? (
                  <img
                    src={user.profileImageUrl}
                    alt={user.username}
                    className="w-8 h-8 rounded-full object-cover border-2 border-gray-300"
                  />
                ) : (
                  <div className="w-8 h-8 rounded-full bg-gray-300 flex items-center justify-center">
                    <FiUser size={18} />
                  </div>
                )}
              </button>

              {showProfileMenu && (
                <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 border border-gray-200">
                  <Link
                    to={`/${user?.username}`}
                    className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                    onClick={() => setShowProfileMenu(false)}
                  >
                    <FiUser className="inline mr-2" />
                    Profile
                  </Link>
                  <button
                    onClick={handleLogout}
                    className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                  >
                    <FiLogOut className="inline mr-2" />
                    Logout
                  </button>
                </div>
              )}
            </div>
          </nav>

          {/* Mobile Menu Button */}
          <button
            className="md:hidden text-gray-700"
            onClick={() => setShowMobileMenu(!showMobileMenu)}
          >
            <FiMenu size={24} />
          </button>
        </div>

        {/* Mobile Menu */}
        {showMobileMenu && (
          <div className="md:hidden py-4 border-t border-gray-200">
            <nav className="flex flex-col space-y-4">
              <Link to="/" className="flex items-center text-gray-700 hover:text-black">
                <FiHome className="mr-3" size={20} />
                Home
              </Link>
              <Link to="/search" className="flex items-center text-gray-700 hover:text-black">
                <FiSearch className="mr-3" size={20} />
                Search
              </Link>
              <Link to="/create" className="flex items-center text-gray-700 hover:text-black">
                <FiPlusSquare className="mr-3" size={20} />
                Create
              </Link>
              <Link to="/explore" className="flex items-center text-gray-700 hover:text-black">
                <FiSearch className="mr-3" size={20} />
                Explore
              </Link>
              <Link to="/notifications" className="flex items-center text-gray-700 hover:text-black">
                <FiHeart className="mr-3" size={20} />
                Notifications
              </Link>
              <Link to={`/${user?.username}`} className="flex items-center text-gray-700 hover:text-black">
                <FiUser className="mr-3" size={20} />
                Profile
              </Link>
              <button
                onClick={handleLogout}
                className="flex items-center text-gray-700 hover:text-black text-left"
              >
                <FiLogOut className="mr-3" size={20} />
                Logout
              </button>
            </nav>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;
