/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class', // enables toggling dark mode via a 'dark' class
  content: [
    './src/app/**/*.{html,ts}',   // all Angular app components
    './src/web-client-angular/**/*.{html,ts}', // optional, include other source dirs
  ],
  theme: {
    extend: {
      colors: {
        teal: {
          500: '#14b8a6', // example custom teal
        },
      },
      fontFamily: {
        sans: ['Roboto', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
