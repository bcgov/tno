import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { generateQuery, IFilterSettingsModel } from 'tno-core';

import { useContent } from './useContent';

// Code responsible for fetching results from the server
// @param filter: IFilterSettingsModel - filter settings to be used in the dependency array of fetchResults
// Returns an object with the following properties:
// - isLoading: boolean
// - fetchResults: function
// - currDateResults: IContentSearchResult[]
// - prevDateResults: IContentSearchResult[]
export const useFetchResults = () => {
  const [currDateResults, setCurrDateResults] = React.useState<IContentSearchResult[]>([]);
  const [prevDateResults, setPrevDateResults] = React.useState<IContentSearchResult[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);
  const [, { findContentWithElasticsearch }] = useContent();

  // TODO: Replace in search page and ensure offset is covered
  const fetchResults = React.useCallback(
    async (filter: IFilterSettingsModel) => {
      if (!filter.startDate) {
        return;
      }
      const dayInMs = 24 * 60 * 60 * 1000; // Hours*Minutes*Seconds*Milliseconds
      const currStartDate = new Date(filter.startDate);
      const prevStartDate = new Date(currStartDate.getTime() - 5 * dayInMs);
      const currEndDate = new Date(currStartDate.getTime() + dayInMs - 1);
      const query = generateQuery({
        ...filter,
        startDate: prevStartDate.toISOString(),
        endDate: currEndDate.toISOString(),
      });
      try {
        setIsLoading(true);
        const res: any = await findContentWithElasticsearch(query, false);
        const currResults: IContentSearchResult[] = [],
          prevResults: IContentSearchResult[] = [];

        res.hits.hits.forEach((h: { _source: IContentSearchResult }) => {
          const resDate = new Date(h._source.publishedOn);
          if (
            resDate.getTime() >= currStartDate.getTime() &&
            resDate.getTime() <= currEndDate.getTime()
          ) {
            // result occurred during currently selected date
            currResults.push(h._source);
          } else if (
            // result occurred sometime in past 5 days
            resDate.getTime() >= prevStartDate.getTime() &&
            resDate.getTime() <= currEndDate.getTime()
          ) {
            prevResults.push(h._source);
          }
        });
        setCurrDateResults(currResults);
        setPrevDateResults(prevResults);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [findContentWithElasticsearch],
  );

  return { isLoading, fetchResults, currDateResults, prevDateResults, setIsLoading };
};
