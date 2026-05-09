/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#2563eb',
        secondary: '#171923',
        border: '#e4e7ec',
        hover: '#f5f7fa',
      },
    },
  },
  plugins: [],
}
