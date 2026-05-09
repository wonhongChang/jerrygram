import React from 'react';
import Header from './Header';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
    <div className="min-h-screen">
      <Header />
      <main className="mx-auto max-w-6xl px-4 pb-24 pt-5 sm:px-6 md:ml-64 md:px-8 md:pb-10 md:pt-8 lg:px-10">
        <div className="mx-auto w-full max-w-5xl">{children}</div>
      </main>
    </div>
  );
};

export default Layout;
