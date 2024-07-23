import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent } from 'store/hooks';
import { Row } from 'tno-core';

import { IPreviousDate } from './interfaces';
import * as styled from './styled';

export interface IPreviousResultsProps {
  currDateResults: IContentSearchResult[];
  prevDateResults: IContentSearchResult[];
  setResults: React.Dispatch<React.SetStateAction<IContentSearchResult[]>>;
  startDate: Date;
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

  React.useEffect(() => {
    // wait for startDate, and also do not want to fetch if results are returned
    if (!startDate || currDateResults.length) return;
    createDateRanges(startDate);
    // only want to run when start date or source ids change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.startDate, currDateResults, prevDateResults]);

  const createDateRanges = React.useCallback(
    (startDate: Date) => {
      if (!prevDateResults.length) return setPrevResults([]);
      const dayInMs = 24 * 60 * 60 * 1000; // Hours*Minutes*Seconds*Milliseconds
      // Previous 7 days that will be used to fetch in a filter
      let dateRanges: IPreviousDate[] = [];
      const days =
        moment(startDate)
          .startOf('day')
          .diff(
            moment(prevDateResults[prevDateResults.length - 1].publishedOn).endOf('day'),
            'days',
          ) + 1;

      // Generate the previous 7 days from the read date
      for (let i = 1; i <= days; i++) {
        const currStartDate = new Date(startDate.getTime() - i * dayInMs);
        const currEndDate = new Date(currStartDate.getTime() + dayInMs - 1);
        const currResults = prevDateResults.filter((res) => {
          const resDate = new Date(res.publishedOn);
          if (
            resDate.getTime() >= currStartDate.getTime() &&
            resDate.getTime() <= currEndDate.getTime()
          ) {
            return true;
          }
          return false;
        });

        dateRanges.push({
          startDate: currStartDate.toISOString(),
          endDate: currEndDate.toISOString(),
          hits: currResults.length,
        });
      }
      setPrevResults(dateRanges);
    },
    [prevDateResults],
  );

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
              key={x.startDate}
              className="prev-result-row"
              onClick={() => {
                const newFilter = { ...filter, startDate: x.startDate, endDate: x.endDate };
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
