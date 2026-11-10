import 'styled-components';

declare module 'styled-components' {
  /** Each design provides its SCSS `:export` variables (css/_variables.module.scss) as `css`. */
  export interface DefaultTheme {
    css: Record<string, string>;
  }
}
