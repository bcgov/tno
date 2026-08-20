import React from 'react';
import { FaCircleInfo } from 'react-icons/fa6';

import { sectionDocs } from './constants';

export interface ISectionInfoButtonProps {
  doc: keyof typeof sectionDocs;
}

/**
 * An info icon that opens its section documentation in a light-weight popover anchored at the
 * icon - not a modal: the page stays interactive, and a click outside or Escape dismisses it.
 */
export const SectionInfoButton: React.FC<ISectionInfoButtonProps> = ({ doc }) => {
  const [open, setOpen] = React.useState(false);
  const anchorRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    if (!open) return;
    const onMouseDown = (e: MouseEvent) => {
      if (!anchorRef.current?.contains(e.target as Node)) setOpen(false);
    };
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false);
    };
    document.addEventListener('mousedown', onMouseDown);
    document.addEventListener('keydown', onKeyDown);
    return () => {
      document.removeEventListener('mousedown', onMouseDown);
      document.removeEventListener('keydown', onKeyDown);
    };
  }, [open]);

  const { title, content } = sectionDocs[doc];
  return (
    <div className="section-doc-anchor" ref={anchorRef}>
      <button
        type="button"
        className="section-doc-button"
        aria-label={`${title} help`}
        aria-expanded={open}
        title={`${title} help`}
        onClick={() => setOpen((current) => !current)}
      >
        <FaCircleInfo />
      </button>
      {open && (
        <div className="section-doc-popover" role="note" aria-label={title}>
          <div className="section-doc-popover-head">
            <strong>{title}</strong>
            <button
              type="button"
              className="rule-icon-button"
              aria-label="Close help"
              title="Close"
              onClick={() => setOpen(false)}
            >
              ×
            </button>
          </div>
          <div className="section-doc-content">{content}</div>
        </div>
      )}
    </div>
  );
};
