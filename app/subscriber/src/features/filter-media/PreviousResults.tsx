import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent } from 'store/hooks';
import { generateQuery, Row } from 'tno-core';

import { IPreviousDate } from './interfaces';
import * as styled from './styled';

export interface IPreviousResultsProps {
  results: IContentSearchResult[];
  setResults: React.Dispatch<React.SetStateAction<IContentSearchResult[]>>;
}
export const PreviousResults: React.FC<IPreviousResultsProps> = ({ results }) => {
  const [
    {
      mediaType: { filter },
    },
    { findContentWithElasticsearch, storeMediaTypeFilter: storeFilter },
  ] = useContent();

  const [prevResults, setPrevResults] = React.useState<IPreviousDate[]>([]);

  React.useEffect(() => {
    // wait for startDate, and also do not want to fetch if results are returned
    if (!filter.startDate || !!results.length) return;
    createDateRanges(filter.startDate);
    // only want to run when start date or source ids change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.startDate, filter.sourceIds]);

  const createDateRanges = async (startDateStr: string) => {
    // Parse the input strings into Date objects
    const startDate = new Date(startDateStr);

    const dayInMillis = 24 * 60 * 60 * 1000; // Hours*Minutes*Seconds*Milliseconds

    // Previous 5 days that will be used to fetch in a filter
    let dateRanges: IPreviousDate[] = [];

    // Generate the previous 5 days from the read date
    for (let i = 1; i <= 5; i++) {
      const currentStartDate = new Date(startDate.getTime() - i * dayInMillis);
      const currentEndDate = new Date(currentStartDate.getTime() + dayInMillis - 1);

      const result = await findContentWithElasticsearch(
        generateQuery({
          ...filter,
          startDate: currentStartDate.toISOString(),
          endDate: currentEndDate.toISOString(),
        }),
        false,
      );

      dateRanges.push({
        startDate: currentStartDate.toISOString(),
        endDate: currentEndDate.toISOString(),
        hits: result.hits.hits.length,
      });
    }
    setPrevResults(dateRanges);
  };

  return (
    <styled.PreviousResults>
      <p>
        There are no results for your specified filter. If there are results for your filter in the
        past 5 days, they will be listed below.
      </p>
      {prevResults
        .filter((x) => x.hits > 0)
        .map((x) => {
          return (
            <Row
              key={x.startDate}
              className="prev-result-row"
              onClick={() => storeFilter({ ...filter, startDate: x.startDate, endDate: x.endDate })}
            >
              <div className="hits">{x.hits}</div>
              <div className="date">{moment(x.startDate).format('dddd, MMMM DD, YYYY')}</div>
            </Row>
          );
        })}
    </styled.PreviousResults>
  );
};
