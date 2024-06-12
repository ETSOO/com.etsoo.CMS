/// <reference types="vite/client" />

// TypeScript Augmentation

interface ImportMetaEnv {
  /**
   * APP version
   */
  readonly VITE_APP_VERSION: string;

  /**
   * APP base path
   */
  readonly VITE_APP_BASE: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
