import React from 'react';
import { Droppable, type DroppableProps } from 'react-beautiful-dnd';

/**
 * StrictModeDroppable wraps react-beautiful-dnd's Droppable so it works under React 18's
 * `StrictMode`. StrictMode double-invokes effects on mount, which breaks the library's
 * synchronous droppable registration and causes "Cannot find droppable entry with id" errors
 * (especially for nested droppables). Deferring the render by one animation frame lets the
 * registration settle before the droppable is enabled.
 */
export const StrictModeDroppable: React.FC<DroppableProps> = ({ children, ...props }) => {
  const [enabled, setEnabled] = React.useState(false);

  React.useEffect(() => {
    const animation = requestAnimationFrame(() => setEnabled(true));
    return () => {
      cancelAnimationFrame(animation);
      setEnabled(false);
    };
  }, []);

  if (!enabled) return null;

  return <Droppable {...props}>{children}</Droppable>;
};
