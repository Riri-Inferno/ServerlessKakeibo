/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
  theme: {
    extend: {
      fontFamily: {
        sans: ["Noto Sans JP", "sans-serif"],
      },
      // TODO: カラーパレット未定
      // colors: {
      //   primary: '#3B82F6',    // 青系
      //   secondary: '#10B981',  // 緑系（収入）
      //   danger: '#EF4444',     // 赤系（支出）
      // },
    },
  },
  plugins: [],
};
