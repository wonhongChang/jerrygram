/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#0095f6',
        secondary: '#262626',
        border: '#dbdbdb',
        hover: '#f5f5f5',
      },
    },
  },
  plugins: [],
}
