export interface IToneValueProps {
  tone?: number;
}

/**
 * add classname for colouring as well as formatting the tone value (+ sign for positive)
 * @param param0 Component properties.
 * @returns Component
 */
export const ToneValue: React.FC<IToneValueProps> = ({ tone }) => {
  if (tone && tone > 0) return <span className="pos">+{tone}</span>;
  if (tone && tone < 0) return <span className="neg">{tone}</span>;
  return <span className="neut">{tone}</span>;
};
