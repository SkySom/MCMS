using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mcms.IO.Core;
using Mcms.IO.Core.Artifacts;
using Mcms.IO.Core.Extensions;
using Mcms.IO.Core.Protocol.Writing;
using Mcms.IO.Data;
using Microsoft.Extensions.Logging;

namespace Mcms.IO.Intermediary
{
    public class IntermediaryIOWriter
        : IIOWriter
    {
        private readonly ILogger<IntermediaryIOWriter> _logger;

        public IntermediaryIOWriter(ILogger<IntermediaryIOWriter> logger)
        {
            _logger = logger;
        }

        public async Task WriteTo(ExternalRelease externalRelease, IArtifact artifact, WriteContext context)
        {
            var mappingFileContents = new LinkedList<string>();

            externalRelease.Packages.ForEachWithProgressCallback(package =>
            {
                package.Classes.ForEachWithProgressCallback(cls =>
                    {
                        mappingFileContents.AddLast(
                            $"CLASS\t{cls.Input}\t{package.Output.Replace(".", "/")}/{cls.Output}");

                        cls.Methods.ForEachWithProgressCallback(method =>
                            {
                                mappingFileContents.AddLast(
                                    $"METHOD\t{cls.Input}\t{method.Descriptor}\t{method.Input}\t{method.Output}");

                                //Intermediary has no parameters in its syntax.!
                                //No need to export those.
                            },
                        (count, current, percentage) =>
                        {
                            _logger.LogInformation(
                                $"  > {percentage}% ({current}/{count}): Exporting methods from: {cls.Output} ...");
                        });

                        cls.Fields.ForEachWithProgressCallback(field =>
                            {
                                mappingFileContents.AddLast(
                                    $"FIELD\t{cls.Input}\t{field.Type}\t{field.Input}\t{field.Output}");
                            },
                            (count, current, percentage) =>
                            {
                                _logger.LogInformation(
                                    $"  > {percentage}% ({current}/{count}): Exporting fields from: {cls.Output} ...");
                            });
                    },
                (count, current, percentage) =>
                {
                    _logger.LogInformation(
                        $"  > {percentage}% ({current}/{count}): Exporting classes from: {package.Output} ...");
                });
            },
            (count, current, percentage) =>
            {
                _logger.LogInformation(
                    $"  > {percentage}% ({current}/{count}): Exporting packages from: {externalRelease.Name} ...");
            });
        }
    }
}
