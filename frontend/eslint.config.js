import js from '@eslint/js';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import tseslint from 'typescript-eslint';

const browserGlobals = { AbortController: 'readonly', AbortSignal: 'readonly', Blob: 'readonly', document: 'readonly', fetch: 'readonly', File: 'readonly', HTMLAnchorElement: 'readonly', location: 'readonly', URL: 'readonly', window: 'readonly' };

export default tseslint.config(
  { ignores: ['dist', '.tmp-dist-prompt041', '.tmp-dist-prompt042'] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ['**/*.{ts,tsx}'],
    languageOptions: { ecmaVersion: 2020, globals: browserGlobals },
    plugins: { 'react-hooks': reactHooks, 'react-refresh': reactRefresh },
    rules: { ...reactHooks.configs.recommended.rules, 'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
      'react-hooks/set-state-in-effect': 'off' }
  }
);
