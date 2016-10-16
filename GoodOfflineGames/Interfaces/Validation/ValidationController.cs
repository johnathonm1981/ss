﻿using System.Threading.Tasks;

namespace Interfaces.Validation
{
    public interface IValidateFilenameDelegate
    {
        void ValidateFilename(string uri, string expectedFilename);
    }

    public interface IValidateSizeDelegate
    {
        void ValidateSize(string uri, long expectedSize);
    }

    public interface IValidateChunkDelegate
    {
        Task ValidateChunk(System.IO.Stream fileStream, long from, long to, string expectedMd5);
    }

    public interface IValidateDelegate
    {
        Task Validate(string uri);
    }

    public interface IValidationController:
        IValidateFilenameDelegate,
        IValidateSizeDelegate,
        IValidateChunkDelegate,
        IValidateDelegate
    {
        // ...
    }
}
