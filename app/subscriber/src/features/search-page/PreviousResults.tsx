import { IContentSearchResult } from 'features/utils/interfaces';
import moment, { Moment } from 'moment';
import React from 'react';
import { useContent } from 'store/hooks';
import { Row } from 'tno-core';

import { IPreviousDate } from './interfaces';
import * as styled from './styled';

export interface IPreviousResultsProps {
  currDateResults: IContentSearchResult[];
  prevDateResults: IContentSearchResult[];
  setResults: React.Dispatch<React.SetStateAction<IContentSearchResult[]>>;
  startDate: Moment;
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
    (startDate: Moment) => {
      if (!prevDateResults.length) return setPrevResults([]);
      const date = moment(startDate).startOf('day');
      const dateRanges: IPreviousDate[] = [];
      for (let i = 1; i < 9; i++) {
        const endOfDay = moment(date).endOf('day');
        dateRanges.push({
          startDate: date,
          endDate: endOfDay,
          hits: 0,
        });
        date.add(-1, 'day');
      }

      for (let i = 0; i < prevDateResults.length; i++) {
        const content = prevDateResults[i];
        const range = dateRanges.find((r) => {
          const publishedOn = moment(content.publishedOn);
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
              <div className="date">{moment(x.startDate).format('dddd, MMMM DD, YYYY')}</div>
            </Row>
          );
        })}
    </styled.PreviousResults>
  );
};
