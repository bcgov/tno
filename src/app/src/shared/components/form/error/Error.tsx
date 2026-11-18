import { Show } from '../../show';

export interface IErrorProps {
  /** The error message that needs to be displayed */
  error?: string;
}

/**
 * Simple component to conditionally display an error message
 * @param param0 Component properties.
 * @returns Component.
 */
export const Error: React.FC<IErrorProps> = ({ error }) => {
  return (
    <Show visible={!!error}>
      <p role="alert">{error}</p>
    </Show>
  );
};
