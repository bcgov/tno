import { FaFileCode, FaFilm, FaImage, FaNewspaper, FaRadio } from 'react-icons/fa6';
import { useIngestTypes } from 'store/hooks/admin';
import { IIngestModel } from 'tno-core';

export const useIngestIcon = () => {
  const [{ ingestTypes }] = useIngestTypes();

  return (ingest: IIngestModel) => {
    const type = ingestTypes.find((t) => t.id === ingest.ingestTypeId)?.name;

    switch (type) {
      case 'Paper':
        return <FaNewspaper size={30} className="icon" title={type} />;
      case 'Syndication':
        return <FaFileCode size={30} className="icon" title={type} />;
      case 'Audio':
        return <FaRadio size={30} className="icon" title={type} />;
      case 'Video':
        return <FaFilm size={30} className="icon" title={type} />;
      case 'Front Page':
        return <FaImage size={30} className="icon" title={type} />;
      default:
        return null;
    }
  };
};
