import React from 'react';
import { createPortal } from 'react-dom';
import { FaX } from 'react-icons/fa6';
import styled from 'styled-components';

const Dialog = styled.dialog<{ $width?: string }>`
  width: ${(props) => props.$width ?? 'min(80rem, 95vw)'};
  max-width: 95vw;
  max-height: 92vh;
  padding: 0;
  border: 1px solid #ccc;
  border-radius: 0.5rem;
  overflow: hidden;

  &::backdrop {
    background: #0006;
  }

  .ai-dialog {
    display: flex;
    flex-direction: column;
    max-height: 92vh;
  }

  .ai-dialog-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 0.75rem 1.25rem;
    border-bottom: 1px solid #ddd;

    h2 {
      margin: 0;
      font-size: 1.15rem;
      flex: 1;
    }

    button {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      padding: 0.3rem 0.6rem;
      background: transparent;
      border: 1px solid #bbb;
      border-radius: 0.25rem;
      cursor: pointer;
    }
  }

  .ai-dialog-body {
    flex: 1 1 auto;
    min-height: 0;
    overflow: auto;
    padding: 1rem 1.25rem;
  }

  .ai-dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
    padding: 0.75rem 1.25rem;
    border-top: 1px solid #ddd;
  }
`;

export interface IAIPromptDialogProps {
  /** Whether the dialog is shown. */
  open: boolean;
  /** The heading. */
  title: string;
  /** Called when the dialog closes (close button, Escape). */
  onClose: () => void;
  /** CSS width; defaults to a wide dialog. */
  width?: string;
  /** Controls after the heading. */
  header?: React.ReactNode;
  /** Buttons at the bottom; none hides the footer. */
  footer?: React.ReactNode;
  className?: string;
  children?: React.ReactNode;
}

/**
 * A modal dialog for the AI prompt tools, rendered at the document body so it stacks above the
 * report form and nests inside the expanded prompt editor.
 */
export const AIPromptDialog: React.FC<IAIPromptDialogProps> = ({
  open,
  title,
  onClose,
  width,
  header,
  footer,
  className,
  children,
}) => {
  const ref = React.useRef<HTMLDialogElement>(null);

  React.useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    else if (!open && dialog.open) dialog.close();
  }, [open]);

  return createPortal(
    <Dialog
      ref={ref}
      aria-label={title}
      className={className}
      $width={width}
      onClose={onClose}
      onCancel={(e) => {
        e.preventDefault();
        onClose();
      }}
    >
      {open && (
        <div className="ai-dialog">
          <div className="ai-dialog-header">
            <h2>{title}</h2>
            {header}
            <button type="button" title="Close" aria-label="Close" onClick={onClose}>
              <FaX />
            </button>
          </div>
          <div className="ai-dialog-body">{children}</div>
          {footer && <div className="ai-dialog-footer">{footer}</div>}
        </div>
      )}
    </Dialog>,
    document.body,
  );
};
