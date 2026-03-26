/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['var(--font-sans)', 'sans-serif'],
      },
      colors: {
        'background-page': 'var(--color-background-page, #F7F7FB)',
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
        slideDown: {
          from: { transform: 'translateY(-100%)', opacity: '0' },
          to: { transform: 'translateY(0)', opacity: '1' },
        },
        pop: {
          from: { transform: 'scale(0)' },
          to: { transform: 'scale(1)' },
        },
        dropIn: {
          from: { opacity: '0', transform: 'translateY(-6px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        'draw-check': {
          to: { strokeDashoffset: '0' },
        },
      },
      animation: {
        blink: 'blink 1s ease-in-out infinite',
        slideDown: 'slideDown .38s cubic-bezier(.22,1,.36,1) both',
        pop: 'pop .25s .7s cubic-bezier(.34,1.56,.64,1) both',
        dropIn: 'dropIn .17s ease both',
        'draw-check': 'draw-check 0.35s ease-out forwards',
      },
    },
  },
  plugins: [],
}
