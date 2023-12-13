import React from 'react';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { useFolders } from 'store/hooks/admin';
import { FlexboxTable, FormPage, IFolderContentModel, Settings } from 'tno-core';

import { useColumns } from './hooks';
import * as styled from './styled';

/**
 * Provides a list of all topics.
 * Provides CRUD form for topics.
 * @returns Component
 */
const EventOfTheDayList: React.FC = () => {
  const [{ settings }] = useLookup();

  const [, { getContentInFolder }] = useFolders();
  const [, { updateContentTopics }] = useContent();

  const [loading, setLoading] = React.useState(false);
  const [items, setItems] = React.useState<IFolderContentModel[]>([]);

  React.useEffect(() => {
    if (!items.length && !loading) {
      const eventOfTheDayFolderId = settings.find(
        (s) => s.name === Settings.EventOfTheDayFolder,
      )?.value;
      if (eventOfTheDayFolderId) {
        setLoading(true);
        getContentInFolder(+eventOfTheDayFolderId)
          .then((data) => {
            setItems(data);
          })
          .catch(() => {})
          .finally(() => {
            setLoading(false);
          });
      } else {
        toast.error(`${Settings.EventOfTheDayFolder} setting needs to be configured.`);
      }
    }
  }, [settings, getContentInFolder, items.length, loading]);

  const handleSubmit = async (values: IFolderContentModel) => {
    try {
      setLoading(true);
      const result = await updateContentTopics(values.contentId, values.content!.topics);
      let index = items.findIndex((el) => el.contentId === values.contentId);
      let results = [...items];
      results[index].content!.topics = result;
      setItems(results);
    } catch {
      // Ignore error as it's handled globally.
    } finally {
      setLoading(false);
    }
  };

  return (
    <styled.EventOfTheDayList>
      <FormPage>
        <p className="list-title">Update Content for Event of the Day</p>
        <FlexboxTable
          rowId="contentId"
          data={items}
          columns={useColumns(handleSubmit, loading)}
          groupBy={(item) => {
            if (item.original.content?.series?.name) return item.original.content?.series?.name;
            else if (item.original.content?.source?.name)
              return item.original.content?.source?.name;
            else return ' ';
          }}
          showActive={false}
          showSort={false}
          pagingEnabled={false}
          isLoading={loading}
        />
      </FormPage>
    </styled.EventOfTheDayList>
  );
};

export default EventOfTheDayList;
