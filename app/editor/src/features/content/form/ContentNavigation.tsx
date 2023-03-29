import { faTableColumns, faUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { FaChevronLeft, FaChevronRight, FaSpinner } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { Button, ButtonVariant, IconButton, Row, Show, useCombinedView } from 'tno-core';

import { IContentForm } from './interfaces/IContentForm';
import { getContentPath } from './utils';

export interface IContentNavigationProps {
  /** The current content id. */
  values: IContentForm;
  /** Function to fetch content. */
  fetchContent: (id: number) => void;
  /** Root path for combined view. */
  combinedPath?: string;
}

/**
 * Provides a component to navigate between content items and the list view page.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentNavigation: React.FC<IContentNavigationProps> = ({
  values,
  fetchContent,
  combinedPath,
}) => {
  const navigate = useNavigate();
  const [{ content: page }] = useContent();
  const { combined } = useCombinedView(values.contentType);

  const indexPosition = !!values.id ? page?.items.findIndex((c) => c.id === +values.id) ?? -1 : -1;
  const enablePrev = indexPosition > 0;
  const enableNext = indexPosition < (page?.items.length ?? 0) - 1;

  return (
    <Row>
      <Show visible={!combined}>
        <IconButton label="List View" onClick={() => navigate('/contents')} iconType="back" />
      </Show>
      <Show visible={!combined}>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Combined View"
          onClick={() => {
            navigate(
              `${getContentPath(true, values.id, values.contentType, combinedPath)}?form=${
                values.contentType
              }`,
            );
          }}
        >
          <FontAwesomeIcon icon={faTableColumns} />
        </Button>
      </Show>
      <Show visible={combined}>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Full Page View"
          onClick={() => {
            navigate(getContentPath(false, values.id, values.contentType, combinedPath));
          }}
        >
          <FontAwesomeIcon icon={faUpRightFromSquare} />
        </Button>
      </Show>
      <Show visible={!!values.id}>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Previous"
          onClick={() => {
            const id = page?.items[indexPosition - 1]?.id;
            if (!!id) {
              navigate(getContentPath(combined, id, values.contentType, combinedPath));
            }
          }}
          disabled={!enablePrev}
        >
          <FaChevronLeft />
        </Button>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Next"
          onClick={() => {
            const id = page?.items[indexPosition + 1]?.id;
            navigate(getContentPath(combined, id, values.contentType, combinedPath));
          }}
          disabled={!enableNext}
        >
          <FaChevronRight />
        </Button>
        <Button
          variant={ButtonVariant.secondary}
          tooltip="Reload"
          onClick={() => {
            fetchContent(values.id);
          }}
        >
          <FaSpinner />
        </Button>
      </Show>
    </Row>
  );
};
