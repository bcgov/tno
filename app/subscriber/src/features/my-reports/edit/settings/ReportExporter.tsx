import { TableBuilder, TableExporter } from 'components/export';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { ReportSectionTypeName } from 'tno-core';

export const ReportExporter = () => {
  const { values } = useFormikContext<IReportForm>();
  const tblRef = React.useRef(null);

  return (
    <>
      <TableBuilder
        ref={tblRef}
        columns={[{ label: 'section' }, { label: 'filter/folder' }, { label: 'keywords' }]}
        isHidden
        rowData={values.sections
          .filter((section) =>
            [
              ReportSectionTypeName.Content,
              ReportSectionTypeName.Gallery,
              ReportSectionTypeName.MediaAnalytics,
            ].includes(section.sectionType),
          )
          .map((section, index) => ({
            data: [
              section.settings.label ? section.settings.label : 'NO NAME SECTION',
              section.filter?.name ?? section.folder?.name,
              section.filter?.settings.search ?? section.folder?.filter?.settings.search,
            ],
          }))}
      />
      <TableExporter
        label="Export report sections"
        ref={tblRef.current}
        filename="report-sections.xlsx"
      />
    </>
  );
};
