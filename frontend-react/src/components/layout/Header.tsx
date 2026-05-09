import React, { useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import {
  FiBell,
  FiCompass,
  FiHome,
  FiLogOut,
  FiPlusSquare,
  FiSearch,
  FiSettings,
  FiUser,
} from 'react-icons/fi';
import Avatar from '../ui/Avatar';

const navItems = [
  { to: '/', label: 'Home', icon: FiHome },
  { to: '/search', label: 'Search', icon: FiSearch },
  { to: '/create', label: 'Create', icon: FiPlusSquare },
  { to: '/explore', label: 'Explore', icon: FiCompass },
  { to: '/notifications', label: 'Alerts', icon: FiBell },
];

const Header: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [showProfileMenu, setShowProfileMenu] = useState(false);

  const handleLogout = () => {
    logout();
    setShowProfileMenu(false);
    navigate('/login');
  };

  const desktopNavClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-semibold transition-colors ${
      isActive ? 'bg-gray-900 text-white' : 'text-gray-600 hover:bg-gray-100 hover:text-gray-950'
    }`;

  const mobileNavClass = ({ isActive }: { isActive: boolean }) =>
    `flex h-12 w-12 items-center justify-center rounded-md transition-colors ${
      isActive ? 'bg-gray-900 text-white' : 'text-gray-500 hover:bg-gray-100 hover:text-gray-950'
    }`;

  return (
    <>
      <header className="md:hidden sticky top-0 z-50 border-b border-gray-200 bg-white/95 backdrop-blur">
        <div className="flex h-14 items-center justify-between px-4">
          <Link to="/" className="text-xl font-black">
            Jerrygram
          </Link>
          <button
            type="button"
            onClick={() => navigate('/search')}
            className="rounded-md border border-gray-200 bg-gray-50 p-2 text-gray-600"
            aria-label="Search"
          >
            <FiSearch size={20} />
          </button>
        </div>
      </header>

      <aside className="hidden md:flex fixed inset-y-0 left-0 z-50 w-64 flex-col border-r border-gray-200 bg-white/95 px-4 py-5 backdrop-blur">
        <Link to="/" className="mb-8 flex items-center gap-3 px-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-md bg-gray-950 text-sm font-black text-white">
            J
          </span>
          <span className="text-xl font-black">Jerrygram</span>
        </Link>

        <button
          type="button"
          onClick={() => navigate('/search')}
          className="mb-5 flex items-center gap-3 rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-left text-sm text-gray-500 hover:border-gray-300"
        >
          <FiSearch size={18} />
          Search Jerrygram
        </button>

        <nav className="space-y-1">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} end={to === '/'} className={desktopNavClass}>
              <Icon size={20} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="mt-auto">
          <div className="relative">
            <button
              type="button"
              onClick={() => setShowProfileMenu((current) => !current)}
              className="flex w-full items-center gap-3 rounded-md border border-gray-200 bg-white px-3 py-3 text-left hover:bg-gray-50"
              aria-label="Open profile menu"
            >
              <Avatar src={user?.profileImageUrl} username={user?.username} size="sm" />
              <span className="min-w-0 flex-1">
                <span className="block truncate text-sm font-semibold text-gray-950">{user?.username || 'Profile'}</span>
                <span className="block truncate text-xs text-gray-500">{user?.email || 'Account'}</span>
              </span>
            </button>

            {showProfileMenu && (
              <div className="absolute bottom-full left-0 mb-2 w-full rounded-md border border-gray-200 bg-white p-1 shadow-lg">
                <Link
                  to={`/${user?.username || ''}`}
                  className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-gray-700 hover:bg-gray-50"
                  onClick={() => setShowProfileMenu(false)}
                >
                  <FiUser size={16} />
                  Profile
                </Link>
                <Link
                  to="/settings"
                  className="flex items-center gap-2 rounded-md px-3 py-2 text-sm text-gray-700 hover:bg-gray-50"
                  onClick={() => setShowProfileMenu(false)}
                >
                  <FiSettings size={16} />
                  Settings
                </Link>
                <button
                  onClick={handleLogout}
                  className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-left text-sm text-gray-700 hover:bg-gray-50"
                >
                  <FiLogOut size={16} />
                  Logout
                </button>
              </div>
            )}
          </div>
        </div>
      </aside>

      <nav className="md:hidden fixed inset-x-0 bottom-0 z-50 border-t border-gray-200 bg-white/95 px-3 py-2 backdrop-blur">
        <div className="mx-auto flex max-w-md items-center justify-between">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink key={to} to={to} end={to === '/'} className={mobileNavClass} aria-label={label}>
              <Icon size={21} />
            </NavLink>
          ))}
          <NavLink to={`/${user?.username || ''}`} className={mobileNavClass} aria-label="Profile">
            <Avatar src={user?.profileImageUrl} username={user?.username} size="sm" />
          </NavLink>
        </div>
      </nav>
    </>
  );
};

export default Header;
