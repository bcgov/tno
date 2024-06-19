import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent } from 'store/hooks';
import { Row } from 'tno-core';

import { IPreviousDate } from './interfaces';
import * as styled from './styled';

export interface IPreviousResultsProps {
  loaded: boolean;
  currDateResults: IContentSearchResult[];
  prevDateResults: IContentSearchResult[];
  setResults: React.Dispatch<React.SetStateAction<IContentSearchResult[]>>;
  startDate: Date;
}
export const PreviousResults: React.FC<IPreviousResultsProps> = ({
  loaded,
  currDateResults,
  prevDateResults,
  startDate,
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
    if (!startDate || !!currDateResults.length) return;
    createDateRanges(startDate);
    // only want to run when start date or source ids change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.startDate, filter.sourceIds, currDateResults, prevDateResults]);

  const createDateRanges = (startDate: Date) => {
    const dayInMillis = 24 * 60 * 60 * 1000; // Hours*Minutes*Seconds*Milliseconds

    // Previous 5 days that will be used to fetch in a filter
    let dateRanges: IPreviousDate[] = [];

    // Generate the previous 7 days from the read date
    for (let i = 1; i <= 7; i++) {
      const currStartDate = new Date(startDate.getTime() - i * dayInMillis);
      const currEndDate = new Date(currStartDate.getTime() + dayInMillis - 1);
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
  };
  return (
    <styled.PreviousResults>
      <p>
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
