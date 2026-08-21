import React from 'react';
import { FaClone } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { Button, ButtonVariant, Modal, Row, useApi, useModal } from 'tno-core';

interface ILinkedContent {
  id: number;
  headline: string;
  source?: string;
  publishedOn?: string;
  status?: string;
}

export interface IContentDuplicatesButtonProps {
  /** The content item whose duplicate links are listed. */
  contentId: number;
}

/**
 * An icon button that opens a popup listing the content linked to this item as duplicates
 * (content_link records the automation dedupe writes). Clicking a list item opens that content
 * in a new tab.
 */
export const ContentDuplicatesButton: React.FC<IContentDuplicatesButtonProps> = ({ contentId }) => {
  const api = useApi();
  const { toggle, isShowing } = useModal();
  const [items, setItems] = React.useState<ILinkedContent[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);

  const open = async () => {
    toggle();
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

  return (
    <>
      <Button variant={ButtonVariant.secondary} tooltip="Duplicates" onClick={open}>
        <FaClone />
      </Button>
      <Modal
        headerText={`Duplicates of content ${contentId}`}
        isShowing={isShowing}
        hide={toggle}
        type="custom"
        component={
          <div>
            {isLoading ? (
              <p>Loading…</p>
            ) : items.length === 0 ? (
              <p>No duplicate links have been recorded for this item.</p>
            ) : (
              <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
                {items.map((item) => (
                  <li key={item.id} style={{ borderBottom: '1px solid #e4e7ec' }}>
                    <button
                      type="button"
                      title={`Open content ${item.id} in a new tab`}
                      onClick={() => window.open(`/contents/${item.id}`, '_blank', 'noreferrer')}
                      style={{
                        display: 'block',
                        width: '100%',
                        textAlign: 'left',
                        background: 'none',
                        border: 'none',
                        padding: '0.5rem 0.25rem',
                        cursor: 'pointer',
                      }}
                    >
                      <strong>#{item.id}</strong> {item.headline}
                      <span style={{ color: '#667085' }}>
                        {item.source ? ` — ${item.source}` : ''}
                        {item.publishedOn ? ` (${item.publishedOn.substring(0, 10)})` : ''}
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        }
        customButtons={
          <Row justifyContent="flex-end" width="100%">
            <Button variant={ButtonVariant.secondary} onClick={toggle}>
              Close
            </Button>
          </Row>
        }
      />
    </>
  );
};
