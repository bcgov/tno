import { Button } from 'components/button';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { useFilters, useFolders, useLookup } from 'store/hooks';
import {
  Checkbox,
  Col,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  IOptionItem,
  OptionItem,
  Row,
  Settings,
  Show,
} from 'tno-core';

export interface IReportSectionGalleryProps {
  index: number;
}

export const ReportSectionGallery = React.forwardRef<HTMLDivElement, IReportSectionGalleryProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();
    const [{ myFolders: folders }, { findMyFolders }] = useFolders();
    const [{ myFilters: filters }, { findMyFilters }] = useFilters();
    const [{ isReady, settings }] = useLookup();

    const [defaultFrontPageImagesFilterId, setFrontPageImagesFilterId] = React.useState(0);
    const [folderOptions, setFolderOptions] = React.useState<IOptionItem[]>(
      getSortableOptions(folders),
    );
    const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
      getSortableOptions(filters),
    );

    React.useEffect(() => {
      if (isReady) {
        const defaultFrontPageImagesFilterId = settings.find(
          (s) => s.name === Settings.FrontpageFilter,
        )?.value;
        if (defaultFrontPageImagesFilterId)
          setFrontPageImagesFilterId(+defaultFrontPageImagesFilterId);
      }
    }, [isReady, settings]);

    const section = values.sections[index];
    const useDefaultFrontPageImagesFilter =
      values.sections[index].filterId === defaultFrontPageImagesFilterId;

    React.useEffect(() => {
      // TODO: Move to parent component so that it doesn't run multiple times.
      if (!folders.length) {
        findMyFolders()
          .then((folders) => {
            setFolderOptions(getSortableOptions(folders));
          })
          .catch(() => {});
      }
      if (!filters.length) {
        findMyFilters()
          .then((filters) => {
            setFilterOptions(getSortableOptions(filters));
          })
          .catch(() => {});
      }
      // Only do on init.
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikTextArea name={`sections.${index}.description`} label="Summary text:" />
        <Col className="frm-in">
          <label>Data source</label>
          <p>
            Choose the data sources to populate this section of your report. You can select from
            your saved searches and/or your folders.
          </p>
          <Checkbox
            name="frontPageImageFilter"
            label="Use default front page images filter"
            checked={useDefaultFrontPageImagesFilter}
            onClick={() => {
              setFieldValue(
                `sections.${index}.filterId`,
                useDefaultFrontPageImagesFilter ? undefined : defaultFrontPageImagesFilterId,
              );
            }}
          />
          <Show visible={!useDefaultFrontPageImagesFilter}>
            <Row gap="1rem">
              <Col flex="1" className="description">
                <FormikSelect
                  name={`sections.${index}.filterId`}
                  label="My saved searches"
                  options={filterOptions}
                  value={filterOptions.find((o) => o.value === section.filterId) ?? ''}
                  onChange={(newValue: any) => {
                    const option = newValue as OptionItem;
                    const filter = filters.find((f) => f.id === option?.value);
                    if (filter) setFieldValue(`sections.${index}.filter`, filter);
                  }}
                >
                  <Button
                    disabled={!values.sections[index].filterId}
                    onClick={() =>
                      window.open(`/search/advanced/${values.sections[index].filterId}`, '_blank')
                    }
                  >
                    <FaArrowAltCircleRight />
                  </Button>
                </FormikSelect>
                {section.filter?.description && <p>{section.filter?.description}</p>}
              </Col>
              <Col flex="1" className="description">
                <FormikSelect
                  name={`sections.${index}.folderId`}
                  label="My folders"
                  options={folderOptions}
                  value={folderOptions.find((o) => o.value === section.folderId) ?? ''}
                  onChange={(newValue: any) => {
                    const option = newValue as OptionItem;
                    const folder = folders.find((f) => f.id === option?.value);
                    if (folder) setFieldValue(`sections.${index}.folder`, folder);
                  }}
                >
                  <Button
                    disabled={!values.sections[index].folderId}
                    onClick={() =>
                      window.open(`/folders/${values.sections[index].folderId}`, '_blank')
                    }
                  >
                    <FaArrowAltCircleRight />
                  </Button>
                </FormikSelect>
                {section.folder?.description && <p>{section.folder?.description}</p>}
              </Col>
            </Row>
          </Show>
        </Col>
      </>
    );
  },
);
