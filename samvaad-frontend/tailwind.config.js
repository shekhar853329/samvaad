/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        'background-primary': 'var(--color-background-primary, #ffffff)',
        'background-secondary': 'var(--color-background-secondary, #f0f2f5)',
        'border-secondary': 'var(--color-border-secondary, #ccced2)',
        'border-tertiary': 'var(--color-border-tertiary, #e4e6eb)',
        'text-primary': 'var(--color-text-primary, #050505)',
        'text-secondary': 'var(--color-text-secondary, #65676b)',
        'text-tertiary': 'var(--color-text-tertiary, #8c939d)',
      },
      borderRadius: {
        'md': 'var(--border-radius-md, 6px)',
        'lg': 'var(--border-radius-lg, 12px)',
      },
      keyframes: {
        blink: {
          '0%, 100%': { opacity: '0.3' },
          '50%': { opacity: '1' },
        },
      },
      animation: {
        blink: 'blink 1s ease-in-out infinite',
      },
    },
  },
  plugins: [],
}
