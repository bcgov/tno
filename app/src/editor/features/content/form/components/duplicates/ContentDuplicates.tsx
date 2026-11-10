import React from 'react';
import { FaClone } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useApi } from 'tno-core';

interface ILinkedContent {
  id: number;
  headline: string;
  source?: string;
  publishedOn?: string;
  status?: string;
}

export interface IContentDuplicatesProps {
  /** The content item whose duplicate links are listed; hidden until the item is saved. */
  contentId: number;
}

/**
 * A plain clone icon beside the headline that opens a light-weight popover anchored at the icon -
 * not a modal: the page stays interactive, and a click outside or Escape dismisses it. The popover
 * lists the content linked to this item as duplicates (the content_link records the automation
 * dedupe writes); clicking a row opens that content in a new tab.
 */
export const ContentDuplicates: React.FC<IContentDuplicatesProps> = ({ contentId }) => {
  const api = useApi();
  const [open, setOpen] = React.useState(false);
  const [items, setItems] = React.useState<ILinkedContent[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);
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

  const toggle = async () => {
    const next = !open;
    setOpen(next);
    if (!next) return;
    setIsLoading(true);
    try {
      const response = await api.get<never, { data: ILinkedContent[] }, any>(
        `/editor/contents/${contentId}/links?value=duplicate`,
      );
      setItems(response.data ?? []);
    } catch {
      toast.error('Failed to load the duplicate content list.');
      setItems([]);
    } finally {
      setIsLoading(false);
    }
  };

  if (!contentId) return null;

  return (
    <div className="duplicates-anchor" ref={anchorRef}>
      <FaClone
        className="icon-button"
        title="Duplicate content"
        aria-label="Duplicate content"
        onClick={toggle}
      />
      {open && (
        <div className="duplicates-popover" role="note" aria-label="Duplicate content">
          <div className="duplicates-head">Duplicates</div>
          {isLoading ? (
            <p className="duplicates-empty">Loading…</p>
          ) : items.length === 0 ? (
            <p className="duplicates-empty">No duplicate links have been recorded for this item.</p>
          ) : (
            <ul className="duplicates-list">
              {items.map((item) => (
                <li key={item.id}>
                  <button
                    type="button"
                    className="duplicates-row"
                    title={`Open content ${item.id} in a new tab`}
                    onClick={() => window.open(`/contents/${item.id}`, '_blank', 'noreferrer')}
                  >
                    <strong>#{item.id}</strong> {item.headline}
                    <span className="duplicates-meta">
                      {item.source ? ` — ${item.source}` : ''}
                      {item.publishedOn ? ` (${item.publishedOn.substring(0, 10)})` : ''}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
};
