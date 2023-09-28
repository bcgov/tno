import { getStatusText } from 'features/content/list-view/utils';
import { FaBan, FaCheckCircle, FaRegCircle, FaSpinner } from 'react-icons/fa';
import { FaRegCircleQuestion } from 'react-icons/fa6';
import { ContentStatusName } from 'tno-core';

export interface IStatusProps {
  value: ContentStatusName;
  onClick?: (value: ContentStatusName) => void;
}

export const Status: React.FC<IStatusProps> = ({ value, onClick }) => {
  const title = getStatusText(value);
  if (value === ContentStatusName.Draft)
    return (
      <FaRegCircle
        title={title}
        className="status-draft"
        onClick={() => onClick?.(ContentStatusName.Publish)}
      />
    );
  if (value === ContentStatusName.Publish)
    return <FaSpinner title={title} className="status-publish" />;
  if (value === ContentStatusName.Published)
    return (
      <FaCheckCircle
        title={title}
        className="status-published"
        onClick={() => onClick?.(ContentStatusName.Unpublish)}
      />
    );
  if (value === ContentStatusName.Unpublish)
    return <FaSpinner title={title} className="status-unpublish" />;
  if (value === ContentStatusName.Unpublished)
    return (
      <FaBan
        title={title}
        className="status-unpublished"
        onClick={() => onClick?.(ContentStatusName.Publish)}
      />
    );
  return <FaRegCircleQuestion title={title} />;
};
