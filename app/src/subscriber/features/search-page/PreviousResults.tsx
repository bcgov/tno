import React from 'react';

import { Row } from '@/shared';
import { dayjs, type Dayjs } from '@/shared/utils/date';
import { IContentSearchResult } from '@/subscriber/features/utils/interfaces';
import { useContent } from '@/subscriber/store/hooks';

import { IPreviousDate } from './interfaces';
import * as styled from './styled';

export interface IPreviousResultsProps {
  currDateResults: IContentSearchResult[];
  prevDateResults: IContentSearchResult[];
  setResults: React.Dispatch<React.SetStateAction<IContentSearchResult[]>>;
  startDate: Dayjs;
  executeSearch: Function;
}
export const PreviousResults: React.FC<IPreviousResultsProps> = ({
  currDateResults,
  prevDateResults,
  startDate,
  executeSearch,
}) => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

  const [prevResults, setPrevResults] = React.useState<IPreviousDate[]>([]);

  const createDateRanges = React.useCallback(
    (startDate: Dayjs) => {
      if (!prevDateResults.length) return setPrevResults([]);
      let date = dayjs(startDate).startOf('day');
      const dateRanges: IPreviousDate[] = [];
      // One range per previous day. (With moment, every range shared one mutated start date, so
      // all hits were counted in a single row dated eight days back.)
      for (let i = 1; i < 9; i++) {
        dateRanges.push({
          startDate: date,
          endDate: date.endOf('day'),
          hits: 0,
        });
        date = date.add(-1, 'day');
      }

      for (let i = 0; i < prevDateResults.length; i++) {
        const content = prevDateResults[i];
        const range = dateRanges.find((r) => {
          const publishedOn = dayjs(content.publishedOn);
          return publishedOn >= r.startDate && publishedOn <= r.endDate;
        });
        if (range) range.hits++;
      }
      setPrevResults(dateRanges);
    },
    [prevDateResults],
  );

  React.useEffect(() => {
    // wait for startDate, and also do not want to fetch if results are returned
    if (!startDate || currDateResults.length) return;
    createDateRanges(startDate);
    // only want to run when start date or source ids change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.startDate, currDateResults, prevDateResults]);

  return (
    <styled.PreviousResults>
      <p className="no-results">
        There are no results for your specified filter. If there are results for your filter in the
        past 7 days, they will be listed below.
      </p>
      {prevResults
        .filter((x) => x.hits > 0)
        .map((x) => {
          return (
            <Row
              key={x.startDate.toISOString()}
              className="prev-result-row"
              onClick={() => {
                const newFilter = {
                  ...filter,
                  startDate: x.startDate.toISOString(),
                  endDate: x.endDate.toISOString(),
                };
                storeFilter(newFilter);
                executeSearch(newFilter);
              }}
            >
              <div className="hits">{x.hits}</div>
              <div className="date">{dayjs(x.startDate).format('dddd, MMMM DD, YYYY')}</div>
            </Row>
          );
        })}
    </styled.PreviousResults>
  );
};
