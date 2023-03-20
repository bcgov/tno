import { AxiosResponseHeaders, RawAxiosResponseHeaders } from 'axios';

/**
 * Extract the filename from the Content-Disposition header.
 * @param headers Axios headers.
 * @returns filename or undefined.
 */
export const extractFileName = (headers: RawAxiosResponseHeaders | AxiosResponseHeaders) => {
  var disposition = headers['content-disposition'];
  if (disposition && disposition.indexOf('attachment') !== -1) {
    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
    var matches = filenameRegex.exec(disposition);
    if (matches != null && matches[1]) {
      return matches[1].replace(/['"]/g, '');
    }
  }
  return undefined;
};
