using System;

namespace TNO.Services.Transcription.Exceptions
{
    public class FileMissingException : Exception
    {
        public long ContentId { get; }
        public string Path { get; }

        public FileMissingException(long contentId, string path)
            : base($"File missing for Content ID: {contentId}. Path: {path}")
        {
            ContentId = contentId;
            Path = path;
        }
    }

    public class EmptyTranscriptException : Exception
    {
        public long ContentId { get; }

        public EmptyTranscriptException(long contentId)
            : base($"Content did not generate a transcript. Content ID: {contentId}")
        {
            ContentId = contentId;
        }
    }

    public class ContentNotFoundException : Exception
    {
        public long ContentId { get; }

        public ContentNotFoundException(long contentId)
            : base($"Content no longer exists. Content ID: {contentId}")
        {
            ContentId = contentId;
        }
    }



}