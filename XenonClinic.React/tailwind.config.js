/** @type {import('tailwindcss').Config} */
import sharedPreset from '../Shared.UI/src/theme/tailwind-preset.js';

export default {
  presets: [sharedPreset],
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
    "../Shared.UI/src/**/*.{js,ts,jsx,tsx}",
  ],
  plugins: [],
}
